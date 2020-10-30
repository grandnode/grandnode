using Grand.Domain.Tasks;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Services.Tasks;
using Grand.Web.Areas.Admin.Extensions.Mapping;
using Grand.Web.Areas.Admin.Models.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        private readonly IStoreService _storeService;

        #endregion

        #region Constructors
        public ScheduleTaskController(
            IScheduleTaskService scheduleTaskService,
            ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper,
            IStoreService storeService)
        {
            _scheduleTaskService = scheduleTaskService;
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;
            _storeService = storeService;
        }
        #endregion

        #region Utility

        [NonAction]
        protected virtual ScheduleTaskModel PrepareScheduleTaskModel(ScheduleTask task)
        {
            var model = task.ToModel(_dateTimeHelper);           
            return model;
        }
        [NonAction]
        protected virtual async Task<ScheduleTaskModel> PrepareStores(ScheduleTaskModel model)
        {
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.Select"), Value = "" });
            foreach (var s in await _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Shortcut, Value = s.Id.ToString() });

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
            var model = task.ToModel(_dateTimeHelper);
            model = await PrepareStores(model);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> EditScheduler(ScheduleTaskModel model, bool continueEditing)
        {
            var scheduleTask = await _scheduleTaskService.GetTaskById(model.Id);
            if (ModelState.IsValid)
            {
                scheduleTask = model.ToEntity(scheduleTask);
                await _scheduleTaskService.UpdateTask(scheduleTask);
                SuccessNotification(_localizationService.GetResource("Admin.System.ScheduleTasks.Updated"));
                if (continueEditing)
                {                    
                    return await EditScheduler(model.Id);
                }
                return RedirectToAction("List");
            }
            model.ScheduleTaskName = scheduleTask.ScheduleTaskName;
            model.Type = scheduleTask.Type;
            model = await PrepareStores(model);
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