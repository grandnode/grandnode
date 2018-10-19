using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Helpers;
using Grand.Services.Knowledgebase;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    public partial class ActivityLogController : BaseAdminController
    {
        #region Fields

        private readonly ICustomerActivityService _customerActivityService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerService _customerService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IKnowledgebaseService _knowledgebaseService;
        private readonly IProductService _productService;
        #endregion Fields

        #region Constructors

        public ActivityLogController(ICustomerActivityService customerActivityService,
            IDateTimeHelper dateTimeHelper, ILocalizationService localizationService,
            IPermissionService permissionService, ICustomerService customerService,
            ICategoryService categoryService, IManufacturerService manufacturerService,
            IProductService productService, IKnowledgebaseService knowledgebaseService)
        {
            this._customerActivityService = customerActivityService;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._customerService = customerService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._productService = productService;
            this._knowledgebaseService = knowledgebaseService;
        }

        #endregion

        #region Activity log types

        public IActionResult ListTypes()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActivityLog))
                return AccessDeniedView();

            var model = _customerActivityService
                .GetAllActivityTypes()
                .Select(x => x.ToModel())
                .ToList();
            return View(model);
        }

        [HttpPost]
        public IActionResult SaveTypes(IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActivityLog))
                return AccessDeniedView();
            
            string formKey = "checkbox_activity_types";
            var checkedActivityTypes = form[formKey].ToString() != null ? form[formKey].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x).ToList() : new List<string>();

            var activityTypes = _customerActivityService.GetAllActivityTypes();
            foreach (var activityType in activityTypes)
            {
                activityType.Enabled = checkedActivityTypes.Contains(activityType.Id);
                _customerActivityService.UpdateActivityType(activityType);
            }
            SuccessNotification(_localizationService.GetResource("Admin.Configuration.ActivityLog.ActivityLogType.Updated"));
            return RedirectToAction("ListTypes");
        }

        #endregion

        #region Activity log

        public IActionResult ListLogs()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActivityLog))
                return AccessDeniedView();

            var activityLogSearchModel = new ActivityLogSearchModel();
            activityLogSearchModel.ActivityLogType.Add(new SelectListItem
            {
                Value = "",
                Text = "All"
            });


            foreach (var at in _customerActivityService.GetAllActivityTypes())
            {
                activityLogSearchModel.ActivityLogType.Add(new SelectListItem
                {
                    Value = at.Id.ToString(),
                    Text = at.Name
                });
            }
            return View(activityLogSearchModel);
        }

        [HttpPost]
        public IActionResult ListLogs(DataSourceRequest command, ActivityLogSearchModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActivityLog))
                return AccessDeniedView();

            DateTime? startDateValue = (model.CreatedOnFrom == null) ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnFrom.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.CreatedOnTo == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnTo.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var activityLog = _customerActivityService.GetAllActivities(startDateValue, endDateValue, null, model.ActivityLogTypeId, model.IpAddress, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = activityLog.Select(x =>
                {
                    var customer = _customerService.GetCustomerById(x.CustomerId);
                    var m = x.ToModel();
                    m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                    m.ActivityLogTypeName = _customerActivityService.GetActivityTypeById(m.ActivityLogTypeId)?.Name;
                    m.CustomerEmail = customer != null ? customer.Email : "NULL";
                    return m;

                }),
                Total = activityLog.TotalCount
            };
            return Json(gridModel);
        }

        public IActionResult AcivityLogDelete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActivityLog))
                return AccessDeniedView();

            var activityLog = _customerActivityService.GetActivityById(id);
            if (activityLog == null)
            {
                throw new ArgumentException("No activity log found with the specified id");
            }
            _customerActivityService.DeleteActivity(activityLog);

            return new NullJsonResult();
        }

        public IActionResult ClearAll()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActivityLog))
                return AccessDeniedView();

            _customerActivityService.ClearAllActivities();
            return RedirectToAction("ListLogs");
        }

        #endregion

        #region Acticity Stats

        public IActionResult ListStats()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActivityLog))
                return AccessDeniedView();

            var activityLogSearchModel = new ActivityLogSearchModel();
            activityLogSearchModel.ActivityLogType.Add(new SelectListItem
            {
                Value = "",
                Text = "All"
            });


            foreach (var at in _customerActivityService.GetAllActivityTypes())
            {
                activityLogSearchModel.ActivityLogType.Add(new SelectListItem
                {
                    Value = at.Id.ToString(),
                    Text = at.Name
                });
            }
            return View(activityLogSearchModel);
        }

        [HttpPost]
        public IActionResult ListStats(DataSourceRequest command, ActivityLogSearchModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActivityLog))
                return AccessDeniedView();

            DateTime? startDateValue = (model.CreatedOnFrom == null) ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnFrom.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.CreatedOnTo == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnTo.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var activityLog = _customerActivityService.GetStatsActivities(startDateValue, endDateValue, model.ActivityLogTypeId, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = activityLog.Select(x =>
                {
                    var activityLogType = _customerActivityService.GetActivityTypeById(x.ActivityLogTypeId);
                    string _name = "-empty-";
                    if(activityLogType!=null)
                    {
                        IList<string> systemKeywordsCategory = new List<string>();
                        systemKeywordsCategory.Add("PublicStore.ViewCategory");
                        systemKeywordsCategory.Add("EditCategory");
                        systemKeywordsCategory.Add("AddNewCategory");

                        if (systemKeywordsCategory.Contains(activityLogType.SystemKeyword))
                        {
                            var category = _categoryService.GetCategoryById(x.EntityKeyId);
                            if (category != null)
                                _name = category.Name;
                        }

                        IList<string> systemKeywordsManufacturer = new List<string>();
                        systemKeywordsManufacturer.Add("PublicStore.ViewManufacturer");
                        systemKeywordsManufacturer.Add("EditManufacturer");
                        systemKeywordsManufacturer.Add("AddNewManufacturer");

                        if (systemKeywordsManufacturer.Contains(activityLogType.SystemKeyword))
                        {
                            var manufacturer = _manufacturerService.GetManufacturerById(x.EntityKeyId);
                            if (manufacturer != null)
                                _name = manufacturer.Name;
                        }

                        IList<string> systemKeywordsProduct = new List<string>();
                        systemKeywordsProduct.Add("PublicStore.ViewProduct");
                        systemKeywordsProduct.Add("EditProduct");
                        systemKeywordsProduct.Add("AddNewProduct");

                        if (systemKeywordsProduct.Contains(activityLogType.SystemKeyword))
                        {
                            var product = _productService.GetProductById(x.EntityKeyId);
                            if (product != null)
                                _name = product.Name;
                        }
                        IList<string> systemKeywordsUrl = new List<string>();
                        systemKeywordsUrl.Add("PublicStore.Url");
                        if (systemKeywordsUrl.Contains(activityLogType.SystemKeyword))
                        {
                            _name = x.EntityKeyId;
                        }

                        IList<string> systemKeywordsKnowledgebaseCategory = new List<string>();
                        systemKeywordsKnowledgebaseCategory.Add("CreateKnowledgebaseCategory");
                        systemKeywordsKnowledgebaseCategory.Add("UpdateKnowledgebaseCategory");
                        systemKeywordsKnowledgebaseCategory.Add("DeleteKnowledgebaseCategory");

                        if (systemKeywordsKnowledgebaseCategory.Contains(activityLogType.SystemKeyword))
                        {
                            var category = _knowledgebaseService.GetKnowledgebaseCategory(x.EntityKeyId);
                            if (category != null)
                                _name = category.Name;
                        }

                        IList<string> systemKeywordsKnowledgebaseArticle = new List<string>();
                        systemKeywordsKnowledgebaseArticle.Add("CreateKnowledgebaseArticle");
                        systemKeywordsKnowledgebaseArticle.Add("UpdateKnowledgebaseArticle");
                        systemKeywordsKnowledgebaseArticle.Add("DeleteKnowledgebaseArticle");

                        if (systemKeywordsKnowledgebaseArticle.Contains(activityLogType.SystemKeyword))
                        {
                            var article = _knowledgebaseService.GetKnowledgebaseArticle(x.EntityKeyId);
                            if (article != null)
                                _name = article.Name;
                        }
                    }

                    var m = x.ToModel();
                    m.ActivityLogTypeName = activityLogType!=null ? activityLogType.Name : "-empty-";
                    m.Name = _name;
                    return m;

                }),
                Total = activityLog.TotalCount
            };
            return Json(gridModel);
        }


        #endregion
    
    }
}
