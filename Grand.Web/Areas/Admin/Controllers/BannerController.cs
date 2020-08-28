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
using System.Threading.Tasks;

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
            _bannerService = bannerService;
            _localizationService = localizationService;
            _languageService = languageService;
        }

        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List() => View();

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> List(DataSourceRequest command)
        {
            var banners = await _bannerService.GetAllBanners();
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

        [PermissionAuthorizeAction(PermissionActionName.Create)]
        public async Task<IActionResult> Create()
        {
            var model = new BannerModel();
            //locales
            await AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(BannerModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var banner = model.ToEntity();
                banner.CreatedOnUtc = DateTime.UtcNow;
                await _bannerService.InsertBanner(banner);

                SuccessNotification(_localizationService.GetResource("Admin.Promotions.Banners.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = banner.Id }) : RedirectToAction("List");
            }

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        public async Task<IActionResult> Edit(string id)
        {
            var banner = await _bannerService.GetBannerById(id);
            if (banner == null)
                return RedirectToAction("List");

            var model = banner.ToModel();

            //locales
            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = banner.GetLocalized(x => x.Name, languageId, false, false);
                locale.Body = banner.GetLocalized(x => x.Body, languageId, false, false);
            });

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        [ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public async Task<IActionResult> Edit(BannerModel model, bool continueEditing)
        {
            var banner = await _bannerService.GetBannerById(model.Id);
            if (banner == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                banner = model.ToEntity(banner);
                await _bannerService.UpdateBanner(banner);
                SuccessNotification(_localizationService.GetResource("Admin.Promotions.Banners.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = banner.Id }) : RedirectToAction("List");
            }

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var banner = await _bannerService.GetBannerById(id);
            if (banner == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                await _bannerService.DeleteBanner(banner);
                SuccessNotification(_localizationService.GetResource("Admin.Promotions.Banners.Deleted"));
                return RedirectToAction("List");
            }
            return RedirectToAction("Edit", new { id = id });
        }
    }
}
