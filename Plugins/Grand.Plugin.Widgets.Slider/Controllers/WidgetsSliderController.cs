using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Plugin.Widgets.Slider.Models;
using Grand.Plugin.Widgets.Slider.Services;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Security;
using Grand.Services.Stores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Plugin.Widgets.Slider.Controllers
{
    [AuthorizeAdmin]
    [Area("Admin")]
    [PermissionAuthorize(PermissionSystemName.Widgets)]
    public class WidgetsSliderController : BasePluginController
    {
        private readonly IStoreService _storeService;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly ISliderService _sliderService;
        private readonly ILanguageService _languageService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;

        public WidgetsSliderController(
            IStoreService storeService,
            IPictureService pictureService,
            ILocalizationService localizationService,
            ISliderService sliderService,
            ILanguageService languageService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService)
        {
            _storeService = storeService;
            _pictureService = pictureService;
            _localizationService = localizationService;
            _sliderService = sliderService;
            _languageService = languageService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
        }

        protected virtual async Task PrepareAllCategoriesModel(SlideModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableCategories.Add(new SelectListItem
            {
                Text = "[None]",
                Value = ""
            });
            var categories = await _categoryService.GetAllCategories(showHidden: true);
            foreach (var c in categories)
            {
                model.AvailableCategories.Add(new SelectListItem
                {
                    Text = _categoryService.GetFormattedBreadCrumb(c, categories),
                    Value = c.Id.ToString()
                });
            }
        }
        protected virtual async Task PrepareAllManufacturersModel(SlideModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableManufacturers.Add(new SelectListItem
            {
                Text = "[None]",
                Value = ""
            });
            var manufacturers = await _manufacturerService.GetAllManufacturers(showHidden: true);
            foreach (var m in manufacturers)
            {
                model.AvailableManufacturers.Add(new SelectListItem
                {
                    Text = m.Name,
                    Value = m.Id.ToString()
                });
            }
        }
        public IActionResult Configure()
        {
            return View("~/Plugins/Widgets.Slider/Views/List.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> List(DataSourceRequest command)
        {
            var sliders = await _sliderService.GetPictureSliders();

            var items = new List<SlideListModel>();
            foreach (var x in sliders)
            {
                var model = x.ToListModel();
                var picture = await _pictureService.GetPictureById(x.PictureId);
                if (picture != null)
                {
                    model.PictureUrl = await _pictureService.GetPictureUrl(picture, 150);
                }
                items.Add(model);
            }
            var gridModel = new DataSourceResult
            {
                Data = items,
                Total = sliders.Count
            };
            return Json(gridModel);
        }

        public async Task<IActionResult> Create()
        {
            var model = new SlideModel();
            //locales
            await AddLocales(_languageService, model.Locales);
            //Stores
            await model.PrepareStoresMappingModel(null, false, _storeService);
            //Categories 
            await PrepareAllCategoriesModel(model);
            //Manufacturers
            await PrepareAllManufacturersModel(model);
            return View("~/Plugins/Widgets.Slider/Views/Create.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(SlideModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var pictureSlider = model.ToEntity();
                pictureSlider.Locales = model.Locales.ToLocalizedProperty();
                pictureSlider.Stores = model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>();

                await _sliderService.InsertPictureSlider(pictureSlider);

                SuccessNotification(_localizationService.GetResource("Plugins.Widgets.Slider.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = pictureSlider.Id }) : RedirectToAction("Configure");

            }

            //Stores
            await model.PrepareStoresMappingModel(null, true, _storeService);
            //Categories 
            await PrepareAllCategoriesModel(model);
            //Manufacturers
            await PrepareAllManufacturersModel(model);

            return View("~/Plugins/Widgets.Slider/Views/Create.cshtml", model);
        }
        public async Task<IActionResult> Edit(string id)
        {
            var slide = await _sliderService.GetById(id);
            if (slide == null)
                return RedirectToAction("Configure");

            var model = slide.ToModel();

            //locales
            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = slide.GetLocalized(x => x.Name, languageId, false, false);
                locale.Description = slide.GetLocalized(x => x.Description, languageId, false, false);
            });
            //Stores
            await model.PrepareStoresMappingModel(slide, false, _storeService);
            //Categories 
            await PrepareAllCategoriesModel(model);
            //Manufacturers
            await PrepareAllManufacturersModel(model);

            return View("~/Plugins/Widgets.Slider/Views/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(SlideModel model, bool continueEditing)
        {
            var pictureSlider = await _sliderService.GetById(model.Id);
            if (pictureSlider == null)
                return RedirectToAction("Configure");

            if (ModelState.IsValid)
            {
                pictureSlider = model.ToEntity();
                pictureSlider.Locales = model.Locales.ToLocalizedProperty();
                pictureSlider.Stores = model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>();
                await _sliderService.UpdatePictureSlider(pictureSlider);
                SuccessNotification(_localizationService.GetResource("Plugins.Widgets.Slider.Edited"));
                return continueEditing ? RedirectToAction("Edit", new { id = pictureSlider.Id }) : RedirectToAction("Configure");

            }
            //Stores
            await model.PrepareStoresMappingModel(pictureSlider, true, _storeService);
            //Categories 
            await PrepareAllCategoriesModel(model);
            //Manufacturers
            await PrepareAllManufacturersModel(model);

            return View("~/Plugins/Widgets.Slider/Views/Edit.cshtml", model);
        }

        public async Task<IActionResult> Delete(string id)
        {
            var pictureSlider = await _sliderService.GetById(id);
            if (pictureSlider == null)
                return Json(new DataSourceResult { Errors = "This pictureSlider not exists" });

            await _sliderService.DeleteSlider(pictureSlider);

            return new NullJsonResult();
        }
    }
}
