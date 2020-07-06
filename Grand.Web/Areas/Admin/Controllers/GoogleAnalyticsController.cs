using Grand.Services.Configuration;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;


namespace Grand.Web.Areas.Admin.Controllers
{
    public partial class GoogleAnalyticsController : BaseAdminController
    {
        private readonly IGoogleAnalyticsService _googleAnalyticsService;

        public GoogleAnalyticsController(
            IGoogleAnalyticsService googleAnalyticsService)
        {
            _googleAnalyticsService = googleAnalyticsService;
        }
       
        public async Task<IActionResult> DashboardGeneralData(DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!startDate.HasValue)
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
