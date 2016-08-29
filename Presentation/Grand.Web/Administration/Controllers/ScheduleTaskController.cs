﻿using System;
using System.Linq;
using System.Web.Mvc;
using Grand.Admin.Models.Tasks;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Tasks;
using Grand.Web.Framework.Kendoui;
using Grand.Core.Domain.Tasks;

namespace Grand.Admin.Controllers
{
    public partial class ScheduleTaskController : BaseAdminController
    {
        #region Fields
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IPermissionService _permissionService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        #endregion

        #region Constructors
        public ScheduleTaskController(
            IScheduleTaskService scheduleTaskService,
            IPermissionService permissionService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService)
        {
            this._scheduleTaskService = scheduleTaskService;
            this._permissionService = permissionService;
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
                Type = task.Type,
                Enabled = task.Enabled,
                StopOnError = task.StopOnError,
                LastStartUtc = task.LastStartUtc.ToString(),
                LastEndUtc = task.LastNonSuccessEndUtc.ToString(),
                LastSuccessUtc = task.LastSuccessUtc.ToString(),
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
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageScheduleTasks))
                return AccessDeniedView();
            return View();
        }

        [HttpPost]
        public ActionResult List(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageScheduleTasks))
                return AccessDeniedView();

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
        public ActionResult EditScheduler(string id)
        {
            var task = _scheduleTaskService.GetTaskById(id);
            var model = new ScheduleTaskModel();
            {
                model.Id = task.Id;
                model.ScheduleTaskName = task.ScheduleTaskName;
                model.Type = task.Type;
                model.Enabled = task.Enabled;
                model.StopOnError = task.StopOnError;
                model.LastStartUtc = task.LastStartUtc.ToString();
                model.LastEndUtc = task.LastNonSuccessEndUtc.ToString();
                model.LastSuccessUtc = task.LastSuccessUtc.ToString();
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
        public ActionResult EditScheduler(ScheduleTaskModel model)
        {
            if (ModelState.IsValid)
            {
                var scheduleTask = _scheduleTaskService.GetTaskById(model.Id);
                scheduleTask.Enabled = model.Enabled;
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
            return View(model);
        }

        public ActionResult RunNow(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageScheduleTasks))
                return AccessDeniedView();
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