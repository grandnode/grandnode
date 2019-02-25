using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Security.Authorization;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Models.Logging;
using Grand.Web.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.SystemLog)]
    public partial class LogController : BaseAdminController
    {
        private readonly ILogViewModelService _logViewModelService;
        private readonly ILogger _logger;
        private readonly ILocalizationService _localizationService;

        public LogController(ILogViewModelService logViewModelService, ILogger logger, 
            ILocalizationService localizationService)
        {
            this._logViewModelService = logViewModelService;
            this._logger = logger;
            this._localizationService = localizationService;
        }

        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List()
        {
            var model = _logViewModelService.PrepareLogListModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult LogList(DataSourceRequest command, LogListModel model)
        {
            var logItems = _logViewModelService.PrepareLogModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = logItems.logModels.ToList(),
                Total = logItems.totalCount
            };

            return Json(gridModel);
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("clearall")]
        public IActionResult ClearAll()
        {
            _logger.ClearLog();

            SuccessNotification(_localizationService.GetResource("Admin.System.Log.Cleared"));
            return RedirectToAction("List");
        }

        public new IActionResult View(string id)
        {
            var log = _logger.GetLogById(id);
            if (log == null)
                //No log found with the specified id
                return RedirectToAction("List");

            var model = _logViewModelService.PrepareLogModel(log);

            return View(model);
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            var log = _logger.GetLogById(id);
            if (log == null)
                //No log found with the specified id
                return RedirectToAction("List");
            if (ModelState.IsValid)
            {
                _logger.DeleteLog(log);
                SuccessNotification(_localizationService.GetResource("Admin.System.Log.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return this.View(id);
        }

        [HttpPost]
        public IActionResult DeleteSelected(ICollection<string> selectedIds)
        {
            if (ModelState.IsValid)
            {
                if (selectedIds != null)
                {
                    var logItems = _logger.GetLogByIds(selectedIds.ToArray());
                    foreach (var logItem in logItems)
                        _logger.DeleteLog(logItem);
                }
                return Json(new { Result = true });
            }
            return ErrorForKendoGridJson(ModelState);
        }
    }
}
