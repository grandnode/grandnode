using Grand.Plugin.Widgets.Slider.Models;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Stores;
using Grand.Framework.Controllers;
using Grand.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Grand.Plugin.Widgets.Slider.Services;
using Grand.Framework.Kendoui;
using System.Linq;
using Grand.Plugin.Widgets.Slider.Domain;
using Grand.Services.Security;
using System;
using Grand.Web.Areas.Admin.Extensions;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Services.Catalog;
using Grand.Framework.Mvc;

namespace Grand.Plugin.Widgets.Slider.Controllers
{
    [AuthorizeAdmin]
    [Area("Admin")]
    public class WidgetsSliderController : BasePluginController
    {
        private readonly IStoreService _storeService;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly ISliderService _sliderService;
        private readonly ILanguageService _languageService;
        private readonly IPermissionService _permissionService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;

        public WidgetsSliderController(
            IStoreService storeService,
            IPictureService pictureService,
            ILocalizationService localizationService,
            ISliderService sliderService,
            ILanguageService languageService,
            IPermissionService permissionService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService)
        {
            this._storeService = storeService;
            this._pictureService = pictureService;
            this._localizationService = localizationService;
            this._sliderService = sliderService;
            this._languageService = languageService;
            this._permissionService = permissionService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
        }

        protected virtual void PrepareAllCategoriesModel(SlideModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableCategories.Add(new SelectListItem
            {
                Text = "[None]",
                Value = ""
            });
            var categories = _categoryService.GetAllCategories(showHidden: true);
            foreach (var c in categories)
            {
                model.AvailableCategories.Add(new SelectListItem
                {
                    Text = c.GetFormattedBreadCrumb(categories),
                    Value = c.Id.ToString()
                });
            }
        }
        protected virtual void PrepareAllManufacturersModel(SlideModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableManufacturers.Add(new SelectListItem
            {
                Text = "[None]",
                Value = ""
            });
            var manufacturers = _manufacturerService.GetAllManufacturers(showHidden: true);
            foreach (var m in manufacturers)
            {
                model.AvailableManufacturers.Add(new SelectListItem
                {
                    Text = m.Name,
                    Value = m.Id.ToString()
                });
            }
        }



        [NonAction]
        protected virtual void PrepareStoresMappingModel(SlideModel model, PictureSlider slider, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableStores = _storeService
                .GetAllStores()
                .Select(s => s.ToModel())
                .ToList();
            if (!excludeProperties)
            {
                if (slider != null)
                {
                    model.SelectedStoreIds = slider.Stores.ToArray();
                }
            }
        }


        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            return View("~/Plugins/Widgets.Slider/Views/List.cshtml");
        }
        [HttpPost]
        public IActionResult List(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var sliders = _sliderService.GetPictureSliders();           
            var gridModel = new DataSourceResult
            {
                Data = sliders.Select(x =>
                {
                    var model = x.ToListModel();
                    var picture = _pictureService.GetPictureById(x.PictureId);
                    if (picture != null)
                    {
                        model.PictureUrl = _pictureService.GetPictureUrl(picture, 150);
                    }
                    return model;
                }),
                Total = sliders.Count
            };
            return Json(gridModel);
        }

        public IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var model = new SlideModel();
            //locales
            AddLocales(_languageService, model.Locales);
            //Stores
            PrepareStoresMappingModel(model, null, false);
            //Categories 
            PrepareAllCategoriesModel(model);
            //Manufacturers
            PrepareAllManufacturersModel(model);
            return View("~/Plugins/Widgets.Slider/Views/Create.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(SlideModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var pictureSlider = model.ToEntity();
                pictureSlider.Locales = model.Locales.ToLocalizedProperty();
                pictureSlider.Stores = model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>();

                _sliderService.InsertPictureSlider(pictureSlider);

                SuccessNotification(_localizationService.GetResource("Plugins.Widgets.Slider.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = pictureSlider.Id }) : RedirectToAction("Configure");

            }

            //Stores
            PrepareStoresMappingModel(model, null, true);
            //Categories 
            PrepareAllCategoriesModel(model);
            //Manufacturers
            PrepareAllManufacturersModel(model);

            return View("~/Plugins/Widgets.Slider/Views/Create.cshtml", model);
        }
        public IActionResult Edit(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var slide = _sliderService.GetById(id);
            if (slide == null)
                return RedirectToAction("Configure");

            var model = slide.ToModel();

            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = slide.GetLocalized(x => x.Name, languageId, false, false);
                locale.Description = slide.GetLocalized(x => x.Description, languageId, false, false);
            });
            //Stores
            PrepareStoresMappingModel(model, slide, false);
            //Categories 
            PrepareAllCategoriesModel(model);
            //Manufacturers
            PrepareAllManufacturersModel(model);

            return View("~/Plugins/Widgets.Slider/Views/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(SlideModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var pictureSlider = _sliderService.GetById(model.Id);
            if (pictureSlider == null)
                return RedirectToAction("Configure");

            if (ModelState.IsValid)
            {
                pictureSlider = model.ToEntity();
                pictureSlider.Locales = model.Locales.ToLocalizedProperty();
                pictureSlider.Stores = model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>();
                _sliderService.UpdatePictureSlider(pictureSlider);
                SuccessNotification(_localizationService.GetResource("Plugins.Widgets.Slider.Edited"));
                return continueEditing ? RedirectToAction("Edit", new { id = pictureSlider.Id }) : RedirectToAction("Configure");

            }

            //Stores
            PrepareStoresMappingModel(model, pictureSlider, true);
            //Categories 
            PrepareAllCategoriesModel(model);
            //Manufacturers
            PrepareAllManufacturersModel(model);

            return View("~/Plugins/Widgets.Slider/Views/Edit.cshtml", model);
        }

        public IActionResult Delete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var pictureSlider = _sliderService.GetById(id);
            if (pictureSlider == null)
                return Json(new DataSourceResult { Errors = "This pictureSlider not exists" });

            _sliderService.DeleteSlider(pictureSlider);

            return new NullJsonResult();
        }
    }
}
