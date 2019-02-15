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
            this._customerActivityService = customerActivityService;
            this._localizationService = localizationService;
            this._activityLogViewModelService = activityLogViewModelService;
        }

        #endregion

        #region Activity log types

        public IActionResult ListTypes()
        {
            var model = _activityLogViewModelService.PrepareActivityLogTypeModels();
            return View(model);
        }

        [HttpPost]
        public IActionResult SaveTypes(IFormCollection form)
        {
            string formKey = "checkbox_activity_types";
            var checkedActivityTypes = form[formKey].ToString() != null ? form[formKey].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x).ToList() : new List<string>();

            _activityLogViewModelService.SaveTypes(checkedActivityTypes);

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.ActivityLog.ActivityLogType.Updated"));
            return RedirectToAction("ListTypes");
        }

        #endregion

        #region Activity log

        public IActionResult ListLogs()
        {
            var model = _activityLogViewModelService.PrepareActivityLogSearchModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult ListLogs(DataSourceRequest command, ActivityLogSearchModel model)
        {
            var activitymodel = _activityLogViewModelService.PrepareActivityLogModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = activitymodel.activityLogs,
                Total = activitymodel.totalCount
            };
            return Json(gridModel);
        }

        public IActionResult AcivityLogDelete(string id)
        {
            var activityLog = _customerActivityService.GetActivityById(id);
            if (activityLog == null)
            {
                throw new ArgumentException("No activity log found with the specified id");
            }
            if (ModelState.IsValid)
            {
                _customerActivityService.DeleteActivity(activityLog);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        public IActionResult ClearAll()
        {
            _customerActivityService.ClearAllActivities();
            return RedirectToAction("ListLogs");
        }

        #endregion

        #region Activity Stats

        public IActionResult ListStats()
        {
            var model = _activityLogViewModelService.PrepareActivityLogSearchModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult ListStats(DataSourceRequest command, ActivityLogSearchModel model)
        {
            var activityStatmodel = _activityLogViewModelService.PrepareActivityStatModel(model, command.Page, command.PageSize);
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
