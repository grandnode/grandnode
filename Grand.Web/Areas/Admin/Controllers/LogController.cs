using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Grand.Web.Areas.Admin.Models.Logging;
using Grand.Core;
using Grand.Core.Domain.Logging;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Security;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Core.Infrastructure;
using Grand.Services.Customers;

namespace Grand.Web.Areas.Admin.Controllers
{
    public partial class LogController : BaseAdminController
    {
        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPermissionService _permissionService;

        public LogController(ILogger logger, IWorkContext workContext,
            ILocalizationService localizationService, IDateTimeHelper dateTimeHelper,
            IPermissionService permissionService)
        {
            this._logger = logger;
            this._workContext = workContext;
            this._localizationService = localizationService;
            this._dateTimeHelper = dateTimeHelper;
            this._permissionService = permissionService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSystemLog))
                return AccessDeniedView();

            var model = new LogListModel();
            model.AvailableLogLevels = LogLevel.Debug.ToSelectList(false).ToList();
            model.AvailableLogLevels.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });

            return View(model);
        }

        [HttpPost]
        public IActionResult LogList(DataSourceRequest command, LogListModel model)
        {

            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSystemLog))
                return AccessDeniedView();

            DateTime? createdOnFromValue = (model.CreatedOnFrom == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnFrom.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? createdToFromValue = (model.CreatedOnTo == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnTo.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            LogLevel? logLevel = model.LogLevelId > 0 ? (LogLevel?)(model.LogLevelId) : null;


            var logItems = _logger.GetAllLogs(createdOnFromValue, createdToFromValue, model.Message,
                logLevel, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = logItems.Select(x => new LogModel
                {
                    Id = x.Id,
                    LogLevel = x.LogLevel.GetLocalizedEnum(_localizationService, _workContext),
                    ShortMessage = x.ShortMessage,
                    //little hack here:
                    //ensure that FullMessage is not returned
                    //otherwise, we can get the following error if log records have too long FullMessage:
                    //"Error during serialization or deserialization using the JSON JavaScriptSerializer. The length of the string exceeds the value set on the maxJsonLength property. "
                    //also it improves performance
                    //FullMessage = x.FullMessage,
                    FullMessage = "",
                    IpAddress = x.IpAddress,
                    CustomerId = x.CustomerId,
                    PageUrl = x.PageUrl,
                    ReferrerUrl = x.ReferrerUrl,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc)
                }),
                Total = logItems.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("clearall")]
        public IActionResult ClearAll()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSystemLog))
                return AccessDeniedView();

            _logger.ClearLog();

            SuccessNotification(_localizationService.GetResource("Admin.System.Log.Cleared"));
            return RedirectToAction("List");
        }

        public new IActionResult View(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSystemLog))
                return AccessDeniedView();

            var log = _logger.GetLogById(id);
            if (log == null)
                //No log found with the specified id
                return RedirectToAction("List");

            var model = new LogModel
            {
                Id = log.Id,
                LogLevel = log.LogLevel.GetLocalizedEnum(_localizationService, _workContext),
                ShortMessage = log.ShortMessage,
                FullMessage = log.FullMessage,
                IpAddress = log.IpAddress,
                CustomerId = log.CustomerId,
                CustomerEmail = !String.IsNullOrEmpty(log.CustomerId) ? EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(log.CustomerId)?.Email: "",
                PageUrl = log.PageUrl,
                ReferrerUrl = log.ReferrerUrl,
                CreatedOn = _dateTimeHelper.ConvertToUserTime(log.CreatedOnUtc, DateTimeKind.Utc)
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSystemLog))
                return AccessDeniedView();

            var log = _logger.GetLogById(id);
            if (log == null)
                //No log found with the specified id
                return RedirectToAction("List");

            _logger.DeleteLog(log);


            SuccessNotification(_localizationService.GetResource("Admin.System.Log.Deleted"));
            return RedirectToAction("List");
        }

        [HttpPost]
        public IActionResult DeleteSelected(ICollection<string> selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSystemLog))
                return AccessDeniedView();

            if (selectedIds != null)
            {
                var logItems = _logger.GetLogByIds(selectedIds.ToArray());
                foreach (var logItem in logItems)
                    _logger.DeleteLog(logItem);
            }

            return Json(new { Result = true});
        }
    }
}
