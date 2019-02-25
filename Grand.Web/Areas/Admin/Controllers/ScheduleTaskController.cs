using Grand.Core.Domain.Tasks;
using Grand.Framework.Kendoui;
using Grand.Framework.Security.Authorization;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Tasks;
using Grand.Web.Areas.Admin.Models.Tasks;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.ScheduleTasks)]
    public partial class ScheduleTaskController : BaseAdminController
    {
        #region Fields
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        #endregion

        #region Constructors
        public ScheduleTaskController(
            IScheduleTaskService scheduleTaskService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService)
        {
            this._scheduleTaskService = scheduleTaskService;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
        }
        #endregion

        #region Utility
        [NonAction]
        protected virtual ScheduleTaskModel PrepareScheduleTaskModel(ScheduleTask task)
        {
            var model = new ScheduleTaskModel
            {
                Id = task.Id,
                ScheduleTaskName = task.ScheduleTaskName,
                LeasedByMachineName = task.LeasedByMachineName,
                Type = task.Type,
                Enabled = task.Enabled,
                StopOnError = task.StopOnError,
                LastStartUtc = task.LastStartUtc,
                LastEndUtc = task.LastNonSuccessEndUtc,
                LastSuccessUtc = task.LastSuccessUtc,
                TimeIntervalChoice = (int)task.TimeIntervalChoice,
                TimeInterval = task.TimeInterval,
                MinuteOfHour = task.MinuteOfHour,
                HourOfDay = task.HourOfDay,
                DayOfWeek = (int)task.DayOfWeek,
                MonthOptionChoice = (int)task.MonthOptionChoice,
                DayOfMonth = task.DayOfMonth
            };
            return model;
        }
        #endregion

        #region Methods
        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List() => View();

        [HttpPost]
        public IActionResult List(DataSourceRequest command)
        {
            //get all tasks and then change their type inside PrepareSCheduleTaskModel and return as List<ScheduleTaskModel>
            var models = _scheduleTaskService.GetAllTasks()
                .Select(PrepareScheduleTaskModel)
                .ToList();
            var gridModel = new DataSourceResult
            {
                Data = models,
                Total = models.Count
            };
            return Json(gridModel);
        }

        [HttpGet]
        public IActionResult EditScheduler(string id)
        {
            var task = _scheduleTaskService.GetTaskById(id);
            var model = new ScheduleTaskModel();
            {
                model.Id = task.Id;
                model.ScheduleTaskName = task.ScheduleTaskName;
                model.LeasedByMachineName = task.LeasedByMachineName;
                model.Type = task.Type;
                model.Enabled = task.Enabled;
                model.StopOnError = task.StopOnError;
                model.LastStartUtc = task.LastStartUtc;
                model.LastEndUtc = task.LastNonSuccessEndUtc;
                model.LastSuccessUtc = task.LastSuccessUtc;
                model.TimeIntervalChoice = (int)task.TimeIntervalChoice;
                model.TimeInterval = task.TimeInterval;
                model.MinuteOfHour = task.MinuteOfHour;
                model.HourOfDay = task.HourOfDay;
                model.DayOfWeek = (int)task.DayOfWeek;
                model.MonthOptionChoice = (int)task.MonthOptionChoice;
                model.DayOfMonth = task.DayOfMonth;
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult EditScheduler(ScheduleTaskModel model)
        {
            var scheduleTask = _scheduleTaskService.GetTaskById(model.Id);
            if (ModelState.IsValid)
            {
                scheduleTask.Enabled = model.Enabled;
                scheduleTask.LeasedByMachineName = model.LeasedByMachineName;
                scheduleTask.StopOnError = model.StopOnError;
                scheduleTask.TimeIntervalChoice = (TimeIntervalChoice)model.TimeIntervalChoice;
                scheduleTask.TimeInterval = model.TimeInterval;
                scheduleTask.MinuteOfHour = model.MinuteOfHour;
                scheduleTask.HourOfDay = model.HourOfDay;
                scheduleTask.DayOfWeek = (DayOfWeek)model.DayOfWeek;
                scheduleTask.MonthOptionChoice = (MonthOptionChoice)model.MonthOptionChoice;
                scheduleTask.DayOfMonth = model.DayOfMonth;
                _scheduleTaskService.UpdateTask(scheduleTask);
                return EditScheduler(model.Id);
            }
            model.ScheduleTaskName = scheduleTask.ScheduleTaskName;
            model.Type = scheduleTask.Type;

            return View(model);
        }

        public IActionResult RunNow(string id)
        {
            try
            {
                var scheduleTask = _scheduleTaskService.GetTaskById(id);
                if (scheduleTask == null) throw new Exception("Schedule task cannot be loaded");
                RegistryGrandNode.RunTaskNow(scheduleTask);
                SuccessNotification(_localizationService.GetResource("Admin.System.ScheduleTasks.RunNow.Done"));
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
            }
            return RedirectToAction("List");
        }
        #endregion
    }
}