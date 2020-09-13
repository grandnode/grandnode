using Grand.Domain.Tasks;
using Grand.Framework.Kendoui;
using Grand.Framework.Security.Authorization;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Tasks;
using Grand.Web.Areas.Admin.Models.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.ScheduleTasks)]
    public partial class ScheduleTaskController : BaseAdminController
    {
        #region Fields

        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion

        #region Constructors
        public ScheduleTaskController(
            IScheduleTaskService scheduleTaskService,
            ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper)
        {
            _scheduleTaskService = scheduleTaskService;
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;
        }
        #endregion

        #region Utility
        [NonAction]
        protected virtual ScheduleTaskModel PrepareScheduleTaskModel(ScheduleTask task)
        {
            var model = new ScheduleTaskModel {
                Id = task.Id,
                ScheduleTaskName = task.ScheduleTaskName,
                LeasedByMachineName = task.LeasedByMachineName,
                Type = task.Type,
                Enabled = task.Enabled,
                StopOnError = task.StopOnError,
                LastStartUtc = task.LastStartUtc.HasValue ? _dateTimeHelper.ConvertToUserTime(task.LastStartUtc.Value, DateTimeKind.Utc) : default(DateTime?),
                LastEndUtc = task.LastNonSuccessEndUtc.HasValue ? _dateTimeHelper.ConvertToUserTime(task.LastNonSuccessEndUtc.Value, DateTimeKind.Utc) : default(DateTime?),
                LastSuccessUtc = task.LastSuccessUtc.HasValue ? _dateTimeHelper.ConvertToUserTime(task.LastSuccessUtc.Value, DateTimeKind.Utc) : default(DateTime?),
                TimeInterval = task.TimeInterval,
            };
            return model;
        }
        #endregion

        #region Methods
        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List() => View();

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> List(DataSourceRequest command)
        {
            //get all tasks and then change their type inside PrepareSCheduleTaskModel and return as List<ScheduleTaskModel>
            var models = (await _scheduleTaskService.GetAllTasks())
                .Select(PrepareScheduleTaskModel)
                .ToList();
            var gridModel = new DataSourceResult {
                Data = models,
                Total = models.Count
            };
            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        [HttpGet]
        public async Task<IActionResult> EditScheduler(string id)
        {
            var task = await _scheduleTaskService.GetTaskById(id);
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
                model.TimeInterval = task.TimeInterval;
            }
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        public async Task<IActionResult> EditScheduler(ScheduleTaskModel model)
        {
            var scheduleTask = await _scheduleTaskService.GetTaskById(model.Id);
            if (ModelState.IsValid)
            {
                scheduleTask.Enabled = model.Enabled;
                scheduleTask.LeasedByMachineName = model.LeasedByMachineName;
                scheduleTask.StopOnError = model.StopOnError;
                scheduleTask.TimeInterval = model.TimeInterval;
                await _scheduleTaskService.UpdateTask(scheduleTask);
                SuccessNotification(_localizationService.GetResource("Admin.System.ScheduleTasks.Updated"));
                return await EditScheduler(model.Id);
            }
            model.ScheduleTaskName = scheduleTask.ScheduleTaskName;
            model.Type = scheduleTask.Type;

            ErrorNotification(ModelState);

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        public async Task<IActionResult> RunNow(string id)
        {
            try
            {
                var scheduleTask = await _scheduleTaskService.GetTaskById(id);
                if (scheduleTask == null) throw new Exception("Schedule task cannot be loaded");
                var typeofTask = Type.GetType(scheduleTask.Type);
                var task = HttpContext.RequestServices.GetServices<IScheduleTask>().FirstOrDefault(x => x.GetType() == typeofTask);
                if (task != null)
                {
                    scheduleTask.LastStartUtc = DateTime.UtcNow;
                    try
                    {
                        await task.Execute();
                        scheduleTask.LastSuccessUtc = DateTime.UtcNow;
                        scheduleTask.LastNonSuccessEndUtc = null;
                        SuccessNotification(_localizationService.GetResource("Admin.System.ScheduleTasks.RunNow.Done"));
                    }
                    catch (Exception exc)
                    {
                        scheduleTask.LastNonSuccessEndUtc = DateTime.UtcNow;
                        SuccessNotification(_localizationService.GetResource("Admin.System.ScheduleTasks.RunNow.Done"));

                        ErrorNotification($"Error while running the {scheduleTask.ScheduleTaskName} schedule task {exc.Message}");
                    }
                    await _scheduleTaskService.UpdateTask(scheduleTask);
                }
                else
                {

                    ErrorNotification($"Task {typeofTask.Name} has not been registered");
                }
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