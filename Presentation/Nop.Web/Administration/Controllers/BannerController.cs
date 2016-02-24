using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Nop.Admin.Extensions;
using Nop.Admin.Models.Messages;
using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Core.Domain.Localization;

namespace Nop.Admin.Controllers
{
	public partial class BannerController : BaseAdminController
	{
        private readonly IBannerService _bannerService;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        public BannerController(IBannerService bannerService,
            ILocalizationService localizationService, 
            IPermissionService permissionService,
            ILanguageService languageService)
		{
            this._bannerService = bannerService;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._languageService = languageService;
        }

        #region Utilities

        [NonAction]
        protected virtual List<LocalizedProperty> UpdateLocales(Banner bn, BannerModel model)
        {
            List<LocalizedProperty> localized = new List<LocalizedProperty>();
            foreach (var local in model.Locales)
            {
                localized.Add(new LocalizedProperty()
                {
                    LanguageId = local.LanguageId,
                    LocaleKey = "Name",
                    LocaleValue = local.Name
                });

                localized.Add(new LocalizedProperty()
                {
                    LanguageId = local.LanguageId,
                    LocaleKey = "Body",
                    LocaleValue = local.Body
                });
            }
            return localized;
        }

        #endregion

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

		public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageBanners))
                return AccessDeniedView();

            return View();
		}

        [HttpPost]
        public ActionResult List(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageBanners))
                return AccessDeniedView();

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

        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageBanners))
                return AccessDeniedView();

            var model = new BannerModel();
            //locales
            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(BannerModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageBanners))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var banner = model.ToEntity();
                banner.CreatedOnUtc = DateTime.UtcNow;
                banner.Locales = UpdateLocales(banner, model);

                _bannerService.InsertBanner(banner);

                SuccessNotification(_localizationService.GetResource("Admin.Promotions.Banners.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = banner.Id }) : RedirectToAction("List");
            }

            return View(model);
        }

		public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageBanners))
                return AccessDeniedView();

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
        public ActionResult Edit(BannerModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageBanners))
                return AccessDeniedView();

            var banner = _bannerService.GetBannerById(model.Id);
            if (banner == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                banner = model.ToEntity(banner);
                banner.Locales = UpdateLocales(banner, model);
                _bannerService.UpdateBanner(banner);
            
                SuccessNotification(_localizationService.GetResource("Admin.Promotions.Banners.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = banner.Id }) : RedirectToAction("List");
            }

            return View(model);
		}

		[HttpPost]
        public ActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageBanners))
                return AccessDeniedView();

            var banner = _bannerService.GetBannerById(id);
            if (banner == null)
                return RedirectToAction("List");

            _bannerService.DeleteBanner(banner);

            SuccessNotification(_localizationService.GetResource("Admin.Promotions.Banners.Deleted"));
			return RedirectToAction("List");
		}
	}
}
