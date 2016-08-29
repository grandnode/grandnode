﻿using System;
using System.Collections.Generic;
using System.Linq;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Core.Caching;
using Grand.Core.Domain.Seo;
using Grand.Services.Configuration;
using System.Web.Mvc;

namespace Grand.Admin.Controllers
{
	public partial class GoogleAnalyticsController : BaseAdminController
	{
        private readonly ICacheManager _cacheManager;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly IGoogleAnalyticsService _googleAnalyticsService;

        public GoogleAnalyticsController(ICacheManager cacheManager,
            ILocalizationService localizationService, 
            IPermissionService permissionService,
            ILanguageService languageService,
            IGoogleAnalyticsService googleAnalyticsService)
		{
            this._cacheManager = cacheManager;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._languageService = languageService;
            this._googleAnalyticsService = googleAnalyticsService;
           
        }

        #region Utilities


        #endregion

        public ActionResult DashboardGeneralData(DateTime? startDate = null, DateTime? endDate = null)
        {
            if(!startDate.HasValue)
            {
                startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            }
            if (!endDate.HasValue)
            {
                endDate = DateTime.UtcNow;
            }

            var report = _googleAnalyticsService.GetDataByGeneral(startDate.Value, endDate.Value);            
            return PartialView(report);
        }

        public ActionResult DashboardDataBySource(DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!startDate.HasValue)
            {
                startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            }
            if (!endDate.HasValue)
            {
                endDate = DateTime.UtcNow;
            }

            var report = _googleAnalyticsService.GetDataBySource(startDate.Value, endDate.Value);
            return PartialView(report);
        }
        public ActionResult DashboardDataByDevice(DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!startDate.HasValue)
            {
                startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            }
            if (!endDate.HasValue)
            {
                endDate = DateTime.UtcNow;
            }

            var report = _googleAnalyticsService.GetDataByDevice(startDate.Value, endDate.Value);
            return PartialView(report);
        }
        public ActionResult DashboardDataByLocalization(DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!startDate.HasValue)
            {
                startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            }
            if (!endDate.HasValue)
            {
                endDate = DateTime.UtcNow;
            }

            var report = _googleAnalyticsService.GetDataByLocalization(startDate.Value, endDate.Value);
            return PartialView(report);
        }
        public ActionResult DashboardDataByExit(DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!startDate.HasValue)
            {
                startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            }
            if (!endDate.HasValue)
            {
                endDate = DateTime.UtcNow;
            }

            var report = _googleAnalyticsService.GetDataByExit(startDate.Value, endDate.Value);
            return PartialView(report);
        }


    }
}
