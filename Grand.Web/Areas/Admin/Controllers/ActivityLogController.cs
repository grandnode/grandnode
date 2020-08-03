using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Security.Authorization;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Models.Logging;
using Grand.Web.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.ActivityLog)]
    public partial class ActivityLogController : BaseAdminController
    {
        #region Fields

        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly IActivityLogViewModelService _activityLogViewModelService;

        #endregion Fields

        #region Constructors

        public ActivityLogController(ICustomerActivityService customerActivityService,
            ILocalizationService localizationService, IActivityLogViewModelService activityLogViewModelService)
        {
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _activityLogViewModelService = activityLogViewModelService;
        }

        #endregion

        #region Activity log types

        public async Task<IActionResult> ListTypes()
        {
            var model = await _activityLogViewModelService.PrepareActivityLogTypeModels();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveTypes(IFormCollection form)
        {
            string formKey = "checkbox_activity_types";
            var checkedActivityTypes = form[formKey].ToString() != null ? form[formKey].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x).ToList() : new List<string>();

            await _activityLogViewModelService.SaveTypes(checkedActivityTypes);

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.ActivityLog.ActivityLogType.Updated"));
            return RedirectToAction("ListTypes");
        }

        #endregion

        #region Activity log

        public async Task<IActionResult> ListLogs()
        {
            var model = await _activityLogViewModelService.PrepareActivityLogSearchModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ListLogs(DataSourceRequest command, ActivityLogSearchModel model)
        {
            var activitymodel = await _activityLogViewModelService.PrepareActivityLogModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = activitymodel.activityLogs,
                Total = activitymodel.totalCount
            };
            return Json(gridModel);
        }

        public async Task<IActionResult> AcivityLogDelete(string id)
        {
            var activityLog = await _customerActivityService.GetActivityById(id);
            if (activityLog == null)
            {
                throw new ArgumentException("No activity log found with the specified id");
            }
            if (ModelState.IsValid)
            {
                await _customerActivityService.DeleteActivity(activityLog);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        public async Task<IActionResult> ClearAll()
        {
            await _customerActivityService.ClearAllActivities();
            return RedirectToAction("ListLogs");
        }

        #endregion

        #region Activity Stats

        public async Task<IActionResult> ListStats()
        {
            var model = await _activityLogViewModelService.PrepareActivityLogSearchModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ListStats(DataSourceRequest command, ActivityLogSearchModel model)
        {
            var activityStatmodel = await _activityLogViewModelService.PrepareActivityStatModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = activityStatmodel.activityStats,
                Total = activityStatmodel.totalCount
            };
            return Json(gridModel);

        }
        #endregion

    }
}
