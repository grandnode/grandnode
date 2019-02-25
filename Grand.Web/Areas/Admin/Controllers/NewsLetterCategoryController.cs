using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Messages;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.NewsletterSubscribers)]
    public partial class NewsletterCategoryController: BaseAdminController
    {
        #region Fields 

        private readonly INewsletterCategoryService _newsletterCategoryService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreService _storeService;

        #endregion

        #region Ctor

        public NewsletterCategoryController(INewsletterCategoryService newsletterCategoryService, ILanguageService languageService,
            ILocalizationService localizationService, IStoreService storeService)
        {
            this._newsletterCategoryService = newsletterCategoryService;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._storeService = storeService;
        }

        #endregion

        #region Methods

        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List() => View();

        [HttpPost]
        public IActionResult List(DataSourceRequest command)
        {
            var newslettercategories = _newsletterCategoryService.GetAllNewsletterCategory();
            var gridModel = new DataSourceResult
            {
                Data = newslettercategories.Select(x =>
                {
                    return new {
                        Id = x.Id,
                        Name = x.Name,
                        Selected = x.Selected,
                        DisplayOrder = x.DisplayOrder
                    };
                }).OrderBy(x=>x.DisplayOrder),
                Total = newslettercategories.Count
            };
            return Json(gridModel);
        }

        public IActionResult Create()
        {
            var model = new NewsletterCategoryModel();
            //locales
            AddLocales(_languageService, model.Locales);
            //Stores
            model.PrepareStoresMappingModel(null, false, _storeService);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(NewsletterCategoryModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var newsletterCategory = model.ToEntity();
                _newsletterCategoryService.InsertNewsletterCategory(newsletterCategory);
                SuccessNotification(_localizationService.GetResource("Admin.Promotions.NewsletterCategory.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = newsletterCategory.Id }) : RedirectToAction("List");
            }

            //Stores
            model.PrepareStoresMappingModel(null, false, _storeService);

            return View(model);
        }

        public IActionResult Edit(string id)
        {
            var newsletterCategory = _newsletterCategoryService.GetNewsletterCategoryById(id);
            if (newsletterCategory == null)
                return RedirectToAction("List");

            var model = newsletterCategory.ToModel();

            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = newsletterCategory.GetLocalized(x => x.Name, languageId, false, false);
                locale.Description = newsletterCategory.GetLocalized(x => x.Description, languageId, false, false);
            });

            //Stores
            model.PrepareStoresMappingModel(newsletterCategory, false, _storeService);
            return View(model);
        }

        [HttpPost]
        [ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public IActionResult Edit(NewsletterCategoryModel model, bool continueEditing)
        {
            var newsletterCategory = _newsletterCategoryService.GetNewsletterCategoryById(model.Id);
            if (newsletterCategory == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                newsletterCategory = model.ToEntity(newsletterCategory);
                _newsletterCategoryService.UpdateNewsletterCategory(newsletterCategory);

                SuccessNotification(_localizationService.GetResource("Admin.Promotions.NewsletterCategory.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = newsletterCategory.Id }) : RedirectToAction("List");
            }
            //Stores
            model.PrepareStoresMappingModel(newsletterCategory, true, _storeService);

            return View(model);
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            var newsletterCategory = _newsletterCategoryService.GetNewsletterCategoryById(id);
            if (newsletterCategory == null)
                return RedirectToAction("List");

            _newsletterCategoryService.DeleteNewsletterCategory(newsletterCategory);

            SuccessNotification(_localizationService.GetResource("Admin.Promotions.NewsletterCategory.Deleted"));
            return RedirectToAction("List");
        }

        #endregion


    }
}