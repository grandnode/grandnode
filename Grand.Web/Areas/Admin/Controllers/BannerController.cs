using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Messages;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Banners)]
    public partial class BannerController : BaseAdminController
    {
        private readonly IBannerService _bannerService;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;

        public BannerController(IBannerService bannerService,
            ILocalizationService localizationService,
            ILanguageService languageService)
        {
            this._bannerService = bannerService;
            this._localizationService = localizationService;
            this._languageService = languageService;
        }

        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List() => View();

        [HttpPost]
        public IActionResult List(DataSourceRequest command)
        {
            var banners = _bannerService.GetAllBanners();
            var gridModel = new DataSourceResult
            {
                Data = banners.Select(x =>
                {
                    var model = x.ToModel();
                    model.Body = "";
                    return model;
                }),
                Total = banners.Count
            };
            return Json(gridModel);
        }

        public IActionResult Create()
        {
            var model = new BannerModel();
            //locales
            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(BannerModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var banner = model.ToEntity();
                banner.CreatedOnUtc = DateTime.UtcNow;
                _bannerService.InsertBanner(banner);

                SuccessNotification(_localizationService.GetResource("Admin.Promotions.Banners.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = banner.Id }) : RedirectToAction("List");
            }

            return View(model);
        }

        public IActionResult Edit(string id)
        {
            var banner = _bannerService.GetBannerById(id);
            if (banner == null)
                return RedirectToAction("List");

            var model = banner.ToModel();

            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = banner.GetLocalized(x => x.Name, languageId, false, false);
                locale.Body = banner.GetLocalized(x => x.Body, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost]
        [ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public IActionResult Edit(BannerModel model, bool continueEditing)
        {
            var banner = _bannerService.GetBannerById(model.Id);
            if (banner == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                banner = model.ToEntity(banner);
                _bannerService.UpdateBanner(banner);
                SuccessNotification(_localizationService.GetResource("Admin.Promotions.Banners.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = banner.Id }) : RedirectToAction("List");
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            var banner = _bannerService.GetBannerById(id);
            if (banner == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                _bannerService.DeleteBanner(banner);
                SuccessNotification(_localizationService.GetResource("Admin.Promotions.Banners.Deleted"));
                return RedirectToAction("List");
            }
            return RedirectToAction("Edit", new { id = id });
        }
    }
}
