using Grand.Core.Caching;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Grand.Services.Security;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;


namespace Grand.Web.Areas.Admin.Controllers
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

        public async Task<IActionResult> DashboardGeneralData(DateTime? startDate = null, DateTime? endDate = null)
        {
            if(!startDate.HasValue)
            {
                startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            }
            if (!endDate.HasValue)
            {
                endDate = DateTime.UtcNow;
            }

            var report = await _googleAnalyticsService.GetDataByGeneral(startDate.Value, endDate.Value);
            return PartialView(report);
        }

        public async Task<IActionResult> DashboardDataBySource(DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!startDate.HasValue)
            {
                startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            }
            if (!endDate.HasValue)
            {
                endDate = DateTime.UtcNow;
            }

            var report = await _googleAnalyticsService.GetDataBySource(startDate.Value, endDate.Value);
            return PartialView(report);
        }
        public async Task<IActionResult> DashboardDataByDevice(DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!startDate.HasValue)
            {
                startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            }
            if (!endDate.HasValue)
            {
                endDate = DateTime.UtcNow;
            }

            var report = await _googleAnalyticsService.GetDataByDevice(startDate.Value, endDate.Value);
            return PartialView(report);
        }
        public async Task<IActionResult> DashboardDataByLocalization(DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!startDate.HasValue)
            {
                startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            }
            if (!endDate.HasValue)
            {
                endDate = DateTime.UtcNow;
            }

            var report = await _googleAnalyticsService.GetDataByLocalization(startDate.Value, endDate.Value);
            return PartialView(report);
        }
        public async Task<IActionResult> DashboardDataByExit(DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!startDate.HasValue)
            {
                startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            }
            if (!endDate.HasValue)
            {
                endDate = DateTime.UtcNow;
            }

            var report = await _googleAnalyticsService.GetDataByExit(startDate.Value, endDate.Value);
            return PartialView(report);
        }


    }
}
