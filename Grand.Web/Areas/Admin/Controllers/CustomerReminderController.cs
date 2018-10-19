using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Framework.Controllers;
using Grand.Framework.Extensions;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Messages;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Services.Vendors;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Customers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    public partial class CustomerReminderController : BaseAdminController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly ICustomerAttributeService _customerAttributeService;
        private readonly ICustomerTagService _customerTagService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IPermissionService _permissionService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IStoreService _storeService;
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerReminderService _customerReminderService;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion

        #region Constructors

        public CustomerReminderController(ICustomerService customerService,
            ICustomerAttributeService customerAttributeService,
            ICustomerTagService customerTagService,
            ILocalizationService localizationService,
            ICustomerActivityService customerActivityService,
            IPermissionService permissionService,
            IProductService productService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IStoreService storeService,
            IVendorService vendorService,
            IWorkContext workContext,
            ICustomerReminderService customerReminderService,
            IMessageTemplateService messageTemplateService,
            IEmailAccountService emailAccountService,
            IDateTimeHelper dateTimeHelper)
        {
            this._customerService = customerService;
            this._customerAttributeService = customerAttributeService;
            this._customerTagService = customerTagService;
            this._localizationService = localizationService;
            this._customerActivityService = customerActivityService;
            this._permissionService = permissionService;
            this._productService = productService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._storeService = storeService;
            this._vendorService = vendorService;
            this._workContext = workContext;
            this._customerReminderService = customerReminderService;
            this._messageTemplateService = messageTemplateService;
            this._emailAccountService = emailAccountService;
            this._dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Utilities

        protected virtual void PrepareModel(CustomerReminderModel.ReminderLevelModel model, CustomerReminder customerReminder)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            var emailAccounts = _emailAccountService.GetAllEmailAccounts();
            foreach (var item in emailAccounts)
            {
                model.EmailAccounts.Add(new SelectListItem
                {
                    Text = item.Email,
                    Value = item.Id.ToString()
                });
            }
            var messageTokenProvider = Grand.Core.Infrastructure.EngineContext.Current.Resolve<IMessageTokenProvider>();
            model.AllowedTokens = FormatTokens(messageTokenProvider.GetListOfCustomerReminderAllowedTokens(customerReminder.ReminderRule));


        }
        public class SerializeCustomerReminderHistoryModel
        {
            public string Id { get; set; }
            public string Email { get; set; }
            public DateTime SendDate { get; set; }
            public int Level { get; set; }
            public bool OrderId { get; set; }
        }
        protected virtual SerializeCustomerReminderHistoryModel PrepareHistoryModelForList(SerializeCustomerReminderHistory history)
        {
            return new SerializeCustomerReminderHistoryModel
            {
                Id = history.Id,
                Email = _customerService.GetCustomerById(history.CustomerId).Email,
                SendDate = _dateTimeHelper.ConvertToUserTime(history.SendDate, DateTimeKind.Utc),
                Level = history.Level,
                OrderId = !String.IsNullOrEmpty(history.OrderId)
            };
        }


        private string FormatTokens(string[] tokens)
        {
            return string.Join(", ", tokens);
        }

        #endregion


        #region Customer reminders

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public IActionResult List(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            var customeractions = _customerReminderService.GetCustomerReminders();
            var gridModel = new DataSourceResult
            {
                Data = customeractions.Select(x => new { Id = x.Id, Name = x.Name, Active = x.Active, Rule = x.ReminderRule.ToString() }),
                Total = customeractions.Count()
            };
            return Json(gridModel);
        }

        public IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();
            var model = new CustomerReminderModel();
            model.StartDateTimeUtc = DateTime.UtcNow;
            model.EndDateTimeUtc = DateTime.UtcNow.AddMonths(1);
            model.LastUpdateDate = DateTime.UtcNow.AddDays(-7);
            model.Active = true;
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(CustomerReminderModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var customerreminder = model.ToEntity();
                _customerReminderService.InsertCustomerReminder(customerreminder);

                //activity log
                _customerActivityService.InsertActivity("AddNewCustomerReminder", customerreminder.Id, _localizationService.GetResource("ActivityLog.AddNewCustomerReminder"), customerreminder.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerReminder.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = customerreminder.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        public IActionResult Edit(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            var customerReminder = _customerReminderService.GetCustomerReminderById(id);
            if (customerReminder == null)
                return RedirectToAction("List");
            var model = customerReminder.ToModel();
            model.ConditionCount = customerReminder.Conditions.Count();
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(CustomerReminderModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            var customerreminder = _customerReminderService.GetCustomerReminderById(model.Id);
            if (customerreminder == null)
                return RedirectToAction("List");
            try
            {
                if (ModelState.IsValid)
                {
                    if (customerreminder.Conditions.Count() > 0)
                        model.ReminderRuleId = customerreminder.ReminderRuleId;
                    if (model.ReminderRuleId == 0)
                        model.ReminderRuleId = customerreminder.ReminderRuleId;

                    customerreminder = model.ToEntity(customerreminder);
                    _customerReminderService.UpdateCustomerReminder(customerreminder);
                    _customerActivityService.InsertActivity("EditCustomerReminder", customerreminder.Id, _localizationService.GetResource("ActivityLog.EditCustomerReminder"), customerreminder.Name);

                    SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerReminder.Updated"));
                    return continueEditing ? RedirectToAction("Edit", new { id = customerreminder.Id }) : RedirectToAction("List");
                }
                return View(model);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("Edit", new { id = customerreminder.Id });
            }
        }

        [HttpPost]
        public IActionResult Run(string Id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();
            var model = _customerReminderService.GetCustomerReminderById(Id);
            if (model == null)
                return RedirectToAction("List");

            if (model.ReminderRule == CustomerReminderRuleEnum.AbandonedCart)
                _customerReminderService.Task_AbandonedCart(model.Id);

            if (model.ReminderRule == CustomerReminderRuleEnum.Birthday)
                _customerReminderService.Task_Birthday(model.Id);

            if (model.ReminderRule == CustomerReminderRuleEnum.LastActivity)
                _customerReminderService.Task_LastActivity(model.Id);

            if (model.ReminderRule == CustomerReminderRuleEnum.LastPurchase)
                _customerReminderService.Task_LastPurchase(model.Id);

            if (model.ReminderRule == CustomerReminderRuleEnum.RegisteredCustomer)
                _customerReminderService.Task_RegisteredCustomer(model.Id);

            SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerReminder.Run"));
            return RedirectToAction("Edit", new { id = Id });
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            var customerReminder = _customerReminderService.GetCustomerReminderById(id);
            if (customerReminder == null)
                return RedirectToAction("List");
            try
            {
                _customerActivityService.InsertActivity("DeleteCustomerReminder", customerReminder.Id, _localizationService.GetResource("ActivityLog.DeleteCustomerReminder"), customerReminder.Name);
                _customerReminderService.DeleteCustomerReminder(customerReminder);
                SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerReminder.Deleted"));
                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
                return RedirectToAction("Edit", new { id = customerReminder.Id });
            }
        }

        [HttpPost]
        public IActionResult History(DataSourceRequest command, string customerReminderId)
        {
            //we use own own binder for searchCustomerRoleIds property 
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            var history = _customerReminderService.GetAllCustomerReminderHistory(customerReminderId,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = history.Select(PrepareHistoryModelForList),
                Total = history.TotalCount
            };

            return Json(gridModel);
        }

        #endregion

        #region Condition

        [HttpPost]
        public IActionResult Conditions(string customerReminderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            var customerReminder = _customerReminderService.GetCustomerReminderById(customerReminderId);
            var gridModel = new DataSourceResult
            {
                Data = customerReminder.Conditions.Select(x => new
                { Id = x.Id, Name = x.Name, Condition = x.Condition.ToString() }),
                Total = customerReminder.Conditions.Count()
            };
            return Json(gridModel);
        }

        public IActionResult AddCondition(string customerReminderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            var customerReminder = _customerReminderService.GetCustomerReminderById(customerReminderId);

            var model = new CustomerReminderModel.ConditionModel();
            model.CustomerReminderId = customerReminderId;
            foreach (CustomerReminderConditionTypeEnum item in Enum.GetValues(typeof(CustomerReminderConditionTypeEnum)))
            {
                if (customerReminder.ReminderRule == CustomerReminderRuleEnum.AbandonedCart || customerReminder.ReminderRule == CustomerReminderRuleEnum.CompletedOrder
                    || customerReminder.ReminderRule == CustomerReminderRuleEnum.UnpaidOrder)
                    model.ConditionType.Add(new SelectListItem()
                    {
                        Value = ((int)item).ToString(),
                        Text = item.ToString()
                    });
                else
                {
                    if (item != CustomerReminderConditionTypeEnum.Product &&
                        item != CustomerReminderConditionTypeEnum.Manufacturer &&
                        item != CustomerReminderConditionTypeEnum.Category)
                    {
                        model.ConditionType.Add(new SelectListItem()
                        {
                            Value = ((int)item).ToString(),
                            Text = item.ToString()
                        });

                    }
                }
            }
            return View(model);

        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult AddCondition(CustomerReminderModel.ConditionModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var customerReminder = _customerReminderService.GetCustomerReminderById(model.CustomerReminderId);
                if (customerReminder == null)
                {
                    return RedirectToAction("List");
                }

                var condition = new CustomerReminder.ReminderCondition()
                {
                    Name = model.Name,
                    ConditionTypeId = model.ConditionTypeId,
                    ConditionId = model.ConditionId,
                };
                customerReminder.Conditions.Add(condition);
                _customerReminderService.UpdateCustomerReminder(customerReminder);

                _customerActivityService.InsertActivity("AddNewCustomerReminderCondition", customerReminder.Id, _localizationService.GetResource("ActivityLog.AddNewCustomerReminder"), customerReminder.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerReminder.Condition.Added"));

                return continueEditing ? RedirectToAction("EditCondition", new { customerReminderId = customerReminder.Id, cid = condition.Id }) : RedirectToAction("Edit", new { id = customerReminder.Id });
            }

            return View(model);
        }


        public IActionResult EditCondition(string customerReminderId, string cid)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            var customerReminder = _customerReminderService.GetCustomerReminderById(customerReminderId);
            if (customerReminder == null)
                return RedirectToAction("List");

            var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == cid);
            if (condition == null)
                return RedirectToAction("List");

            var model = condition.ToModel();
            model.CustomerReminderId = customerReminder.Id;
            foreach (CustomerReminderConditionTypeEnum item in Enum.GetValues(typeof(CustomerReminderConditionTypeEnum)))
            {
                model.ConditionType.Add(new SelectListItem()
                {
                    Value = ((int)item).ToString(),
                    Text = item.ToString()
                });
            }
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult EditCondition(string customerReminderId, string cid, CustomerReminderModel.ConditionModel model, bool continueEditing)
        {
            var customerReminder = _customerReminderService.GetCustomerReminderById(customerReminderId);
            if (customerReminder == null)
                return RedirectToAction("List");

            var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == cid);
            if (condition == null)
                return RedirectToAction("List");
            try
            {
                if (ModelState.IsValid)
                {
                    condition = model.ToEntity(condition);
                    _customerReminderService.UpdateCustomerReminder(customerReminder);

                    _customerActivityService.InsertActivity("EditCustomerReminderCondition", customerReminder.Id, _localizationService.GetResource("ActivityLog.EditCustomerReminderCondition"), customerReminder.Name);

                    SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerReminderCondition.Updated"));
                    return continueEditing ? RedirectToAction("EditCondition", new { customerReminderId = customerReminder.Id, cid = condition.Id }) : RedirectToAction("Edit", new { id = customerReminder.Id });
                }
                return View(model);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("Edit", new { id = customerReminder.Id });
            }
        }

        [HttpPost]
        public IActionResult ConditionDelete(string Id, string customerReminderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            var customerReminder = _customerReminderService.GetCustomerReminderById(customerReminderId);
            var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == Id);
            customerReminder.Conditions.Remove(condition);
            _customerReminderService.UpdateCustomerReminder(customerReminder);

            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult ConditionDeletePosition(string id, string customerReminderId, string conditionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            var customerReminder = _customerReminderService.GetCustomerReminderById(customerReminderId);
            var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == conditionId);

            if (condition.ConditionTypeId == (int)CustomerReminderConditionTypeEnum.Product)
            {
                condition.Products.Remove(id);
                _customerReminderService.UpdateCustomerReminder(customerReminder);
            }
            if (condition.ConditionTypeId == (int)CustomerReminderConditionTypeEnum.Category)
            {
                condition.Categories.Remove(id);
                _customerReminderService.UpdateCustomerReminder(customerReminder);
            }
            if (condition.ConditionTypeId == (int)CustomerReminderConditionTypeEnum.Manufacturer)
            {
                condition.Manufacturers.Remove(id);
                _customerReminderService.UpdateCustomerReminder(customerReminder);
            }

            if (condition.ConditionTypeId == (int)CustomerReminderConditionTypeEnum.CustomerRole)
            {
                condition.CustomerRoles.Remove(id);
                _customerReminderService.UpdateCustomerReminder(customerReminder);
            }
            if (condition.ConditionTypeId == (int)CustomerReminderConditionTypeEnum.CustomerTag)
            {
                condition.CustomerTags.Remove(id);
                _customerReminderService.UpdateCustomerReminder(customerReminder);
            }

            if (condition.ConditionTypeId == (int)CustomerReminderConditionTypeEnum.CustomCustomerAttribute)
            {
                condition.CustomCustomerAttributes.Remove(condition.CustomCustomerAttributes.FirstOrDefault(x => x.Id == id));
                _customerReminderService.UpdateCustomerReminder(customerReminder);
            }
            if (condition.ConditionTypeId == (int)CustomerReminderConditionTypeEnum.CustomerRegisterField)
            {
                condition.CustomerRegistration.Remove(condition.CustomerRegistration.FirstOrDefault(x => x.Id == id));
                _customerReminderService.UpdateCustomerReminder(customerReminder);
            }


            return new NullJsonResult();
        }

        #region Condition Category

        [HttpPost]
        public IActionResult ConditionCategory(string customerReminderId, string conditionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            var customerReminder = _customerReminderService.GetCustomerReminderById(customerReminderId);
            var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == conditionId);

            var gridModel = new DataSourceResult
            {
                Data = condition != null ? condition.Categories.Select(z => new { Id = z, CategoryName = _categoryService.GetCategoryById(z) != null ? _categoryService.GetCategoryById(z).Name : "" }) : null,
                Total = customerReminder.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        public IActionResult CategoryAddPopup(string customerReminderId, string conditionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();
            var model = new CustomerReminderModel.ConditionModel.AddCategoryConditionModel();
            model.ConditionId = conditionId;
            model.CustomerReminderId = customerReminderId;
            return View(model);
        }

        [HttpPost]
        public IActionResult CategoryAddPopupList(DataSourceRequest command, CustomerReminderModel.ConditionModel.AddCategoryConditionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            var categories = _categoryService.GetAllCategories(model.SearchCategoryName,
                pageIndex: command.Page - 1, pageSize: command.PageSize, showHidden: true);
            var gridModel = new DataSourceResult
            {
                Data = categories.Select(x =>
                {
                    var categoryModel = x.ToModel();
                    categoryModel.Breadcrumb = x.GetFormattedBreadCrumb(_categoryService);
                    return categoryModel;
                }),
                Total = categories.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public IActionResult CategoryAddPopup(CustomerReminderModel.ConditionModel.AddCategoryConditionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            if (model.SelectedCategoryIds != null)
            {
                foreach (string id in model.SelectedCategoryIds)
                {
                    var customerReminder = _customerReminderService.GetCustomerReminderById(model.CustomerReminderId);
                    if (customerReminder != null)
                    {
                        var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                        if (condition != null)
                        {
                            if (condition.Categories.Where(x => x == id).Count() == 0)
                            {
                                condition.Categories.Add(id);
                                _customerReminderService.UpdateCustomerReminder(customerReminder);
                            }
                        }
                    }
                }
            }
            ViewBag.RefreshPage = true;
            return View(model);
        }


        #endregion


        #region Condition Manufacturer
        [HttpPost]
        public IActionResult ConditionManufacturer(string customerReminderId, string conditionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            var customerReminder = _customerReminderService.GetCustomerReminderById(customerReminderId);
            var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == conditionId);

            var gridModel = new DataSourceResult
            {
                Data = condition != null ? condition.Manufacturers.Select(z => new { Id = z, ManufacturerName = _manufacturerService.GetManufacturerById(z) != null ? _manufacturerService.GetManufacturerById(z).Name : "" }) : null,
                Total = customerReminder.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        public IActionResult ManufacturerAddPopup(string customerReminderId, string conditionId)
        {
            var model = new CustomerReminderModel.ConditionModel.AddManufacturerConditionModel();
            model.ConditionId = conditionId;
            model.CustomerReminderId = customerReminderId;
            return View(model);
        }

        [HttpPost]
        public IActionResult ManufacturerAddPopupList(DataSourceRequest command, CustomerReminderModel.ConditionModel.AddManufacturerConditionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            var manufacturers = _manufacturerService.GetAllManufacturers(model.SearchManufacturerName, "",
                command.Page - 1, command.PageSize, true);
            var gridModel = new DataSourceResult
            {
                Data = manufacturers.Select(x => x.ToModel()),
                Total = manufacturers.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public IActionResult ManufacturerAddPopup(CustomerReminderModel.ConditionModel.AddManufacturerConditionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            if (model.SelectedManufacturerIds != null)
            {
                foreach (string id in model.SelectedManufacturerIds)
                {
                    var customerReminder = _customerReminderService.GetCustomerReminderById(model.CustomerReminderId);
                    if (customerReminder != null)
                    {
                        var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                        if (condition != null)
                        {
                            if (condition.Manufacturers.Where(x => x == id).Count() == 0)
                            {
                                condition.Manufacturers.Add(id);
                                _customerReminderService.UpdateCustomerReminder(customerReminder);
                            }
                        }
                    }

                }
            }

            ViewBag.RefreshPage = true;
            return View(model);
        }


        #endregion

        #region Condition Product

        [HttpPost]
        public IActionResult ConditionProduct(string customerReminderId, string conditionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            var customerReminder = _customerReminderService.GetCustomerReminderById(customerReminderId);
            var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == conditionId);

            var gridModel = new DataSourceResult
            {
                Data = condition != null ? condition.Products.Select(z => new { Id = z, ProductName = _productService.GetProductById(z) != null ? _productService.GetProductById(z).Name : "" }) : null,
                Total = customerReminder.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        public IActionResult ProductAddPopup(string customerReminderId, string conditionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            var model = new CustomerReminderModel.ConditionModel.AddProductToConditionModel();
            model.ConditionId = conditionId;
            model.CustomerReminderId = customerReminderId;
            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            var categories = _categoryService.GetAllCategories(showHidden: true);
            foreach (var c in categories)
                model.AvailableCategories.Add(new SelectListItem { Text = c.GetFormattedBreadCrumb(categories), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var m in _manufacturerService.GetAllManufacturers(showHidden: true))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var v in _vendorService.GetAllVendors(showHidden: true))
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });

            return View(model);
        }

        [HttpPost]
        public IActionResult ProductAddPopupList(DataSourceRequest command, CustomerActionConditionModel.AddProductToConditionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            var searchCategoryIds = new List<string>();
            if (!String.IsNullOrEmpty(model.SearchCategoryId))
                searchCategoryIds.Add(model.SearchCategoryId);

            var products = _productService.SearchProducts(
                categoryIds: searchCategoryIds,
                manufacturerId: model.SearchManufacturerId,
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize,
                showHidden: true
                );
            var gridModel = new DataSourceResult();
            gridModel.Data = products.Select(x => x.ToModel());
            gridModel.Total = products.TotalCount;

            return Json(gridModel);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public IActionResult ProductAddPopup(CustomerReminderModel.ConditionModel.AddProductToConditionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            if (model.SelectedProductIds != null)
            {
                foreach (string id in model.SelectedProductIds)
                {
                    var customerReminder = _customerReminderService.GetCustomerReminderById(model.CustomerReminderId);
                    if (customerReminder != null)
                    {
                        var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                        if (condition != null)
                        {
                            if (condition.Products.Where(x => x == id).Count() == 0)
                            {
                                condition.Products.Add(id);
                                _customerReminderService.UpdateCustomerReminder(customerReminder);
                            }
                        }
                    }
                }
            }
            ViewBag.RefreshPage = true;
            return View(model);
        }


        #endregion

        #region Customer Tags

        [HttpPost]
        public IActionResult ConditionCustomerTag(string customerReminderId, string conditionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            var customerReminder = _customerReminderService.GetCustomerReminderById(customerReminderId);
            var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == conditionId);

            var gridModel = new DataSourceResult
            {
                Data = condition != null ? condition.CustomerTags.Select(z => new { Id = z, CustomerTag = _customerTagService.GetCustomerTagById(z) != null ? _customerTagService.GetCustomerTagById(z).Name : "" }) : null,
                Total = customerReminder.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }



        [HttpPost]
        public IActionResult ConditionCustomerTagInsert(CustomerReminderModel.ConditionModel.AddCustomerTagConditionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            var customerReminder = _customerReminderService.GetCustomerReminderById(model.CustomerReminderId);
            if (customerReminder != null)
            {
                var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                if (condition != null)
                {
                    if (condition.CustomerTags.Where(x => x == model.CustomerTagId).Count() == 0)
                    {
                        condition.CustomerTags.Add(model.CustomerTagId);
                        _customerReminderService.UpdateCustomerReminder(customerReminder);
                    }
                }
            }
            return new NullJsonResult();
        }


        [HttpGet]
        public IActionResult CustomerTags()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();
            var customerTag = _customerTagService.GetAllCustomerTags().Select(x => new { Id = x.Id, Name = x.Name });
            return Json(customerTag);
        }
        #endregion

        #region Condition Customer role

        [HttpPost]
        public IActionResult ConditionCustomerRole(string customerReminderId, string conditionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            var customerReminder = _customerReminderService.GetCustomerReminderById(customerReminderId);
            var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == conditionId);

            var gridModel = new DataSourceResult
            {
                Data = condition != null ? condition.CustomerRoles.Select(z => new { Id = z, CustomerRole = _customerService.GetCustomerRoleById(z) != null ? _customerService.GetCustomerRoleById(z).Name : "" }) : null,
                Total = customerReminder.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ConditionCustomerRoleInsert(CustomerReminderModel.ConditionModel.AddCustomerRoleConditionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            var customerReminder = _customerReminderService.GetCustomerReminderById(model.CustomerReminderId);
            if (customerReminder != null)
            {
                var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                if (condition != null)
                {
                    if (condition.CustomerRoles.Where(x => x == model.CustomerRoleId).Count() == 0)
                    {
                        condition.CustomerRoles.Add(model.CustomerRoleId);
                        _customerReminderService.UpdateCustomerReminder(customerReminder);
                    }
                }
            }
            return new NullJsonResult();
        }


        [HttpGet]
        public IActionResult CustomerRoles()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();
            var customerRole = _customerService.GetAllCustomerRoles().Select(x => new { Id = x.Id, Name = x.Name });
            return Json(customerRole);
        }

        #endregion

        #region Condition Customer Register


        [HttpPost]
        public IActionResult ConditionCustomerRegister(string customerReminderId, string conditionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            var customerReminder = _customerReminderService.GetCustomerReminderById(customerReminderId);
            var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == conditionId);

            var gridModel = new DataSourceResult
            {
                Data = condition != null ? condition.CustomerRegistration.Select(z => new
                {
                    Id = z.Id,
                    CustomerRegisterName = z.RegisterField,
                    CustomerRegisterValue = z.RegisterValue
                })
                    : null,
                Total = customerReminder.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ConditionCustomerRegisterInsert(CustomerReminderModel.ConditionModel.AddCustomerRegisterConditionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            var customerReminder = _customerReminderService.GetCustomerReminderById(model.CustomerReminderId);
            if (customerReminder != null)
            {
                var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                if (condition != null)
                {
                    var _cr = new CustomerReminder.ReminderCondition.CustomerRegister()
                    {
                        RegisterField = model.CustomerRegisterName,
                        RegisterValue = model.CustomerRegisterValue,
                    };
                    condition.CustomerRegistration.Add(_cr);
                    _customerReminderService.UpdateCustomerReminder(customerReminder);
                }
            }
            return new NullJsonResult();
        }
        [HttpPost]
        public IActionResult ConditionCustomerRegisterUpdate(CustomerReminderModel.ConditionModel.AddCustomerRegisterConditionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            var customerReminder = _customerReminderService.GetCustomerReminderById(model.CustomerReminderId);
            if (customerReminder != null)
            {
                var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                if (condition != null)
                {
                    var cr = condition.CustomerRegistration.FirstOrDefault(x => x.Id == model.Id);
                    cr.RegisterField = model.CustomerRegisterName;
                    cr.RegisterValue = model.CustomerRegisterValue;
                    _customerReminderService.UpdateCustomerReminder(customerReminder);
                }
            }
            return new NullJsonResult();
        }

        [HttpGet]
        public IActionResult CustomerRegisterFields()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            var list = new List<Tuple<string, string>>();
            list.Add(Tuple.Create("Gender", "Gender"));
            list.Add(Tuple.Create("Company", "Company"));
            list.Add(Tuple.Create("CountryId", "CountryId"));
            list.Add(Tuple.Create("City", "City"));
            list.Add(Tuple.Create("StateProvinceId", "StateProvinceId"));
            list.Add(Tuple.Create("StreetAddress", "StreetAddress"));
            list.Add(Tuple.Create("ZipPostalCode", "ZipPostalCode"));
            list.Add(Tuple.Create("Phone", "Phone"));
            list.Add(Tuple.Create("Fax", "Fax"));

            var customer = list.Select(x => new { Id = x.Item1, Name = x.Item2 });
            return Json(customer);
        }
        #endregion

        #region Condition Custom Customer Attribute

        private string CustomerAttribute(string registerField)
        {
            string _field = registerField;
            var _rf = registerField.Split(':');
            if (_rf.Count() > 1)
            {
                var ca = _customerAttributeService.GetCustomerAttributeById(_rf.FirstOrDefault());
                if (ca != null)
                {
                    _field = ca.Name;
                    if (ca.CustomerAttributeValues.FirstOrDefault(x => x.Id == _rf.LastOrDefault()) != null)
                    {
                        _field = ca.Name + "->" + ca.CustomerAttributeValues.FirstOrDefault(x => x.Id == _rf.LastOrDefault()).Name;
                    }
                }

            }

            return _field;
        }

        [HttpPost]
        public IActionResult ConditionCustomCustomerAttribute(string customerReminderId, string conditionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            var customerReminder = _customerReminderService.GetCustomerReminderById(customerReminderId);
            var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == conditionId);

            var gridModel = new DataSourceResult
            {
                Data = condition != null ? condition.CustomCustomerAttributes.Select(z => new
                {
                    Id = z.Id,
                    CustomerAttributeId = CustomerAttribute(z.RegisterField),
                    CustomerAttributeName = z.RegisterField,
                    CustomerAttributeValue = z.RegisterValue
                })
                    : null,
                Total = customerReminder.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ConditionCustomCustomerAttributeInsert(CustomerReminderModel.ConditionModel.AddCustomCustomerAttributeConditionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            var customerReminder = _customerReminderService.GetCustomerReminderById(model.CustomerReminderId);
            if (customerReminder != null)
            {
                var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                if (condition != null)
                {
                    var _cr = new CustomerReminder.ReminderCondition.CustomerRegister()
                    {
                        RegisterField = model.CustomerAttributeName,
                        RegisterValue = model.CustomerAttributeValue,
                    };
                    condition.CustomCustomerAttributes.Add(_cr);
                    _customerReminderService.UpdateCustomerReminder(customerReminder);
                }
            }
            return new NullJsonResult();
        }
        [HttpPost]
        public IActionResult ConditionCustomCustomerAttributeUpdate(CustomerReminderModel.ConditionModel.AddCustomCustomerAttributeConditionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            var customerReminder = _customerReminderService.GetCustomerReminderById(model.CustomerReminderId);
            if (customerReminder != null)
            {
                var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                if (condition != null)
                {
                    var cr = condition.CustomCustomerAttributes.FirstOrDefault(x => x.Id == model.Id);
                    cr.RegisterField = model.CustomerAttributeName;
                    cr.RegisterValue = model.CustomerAttributeValue;
                    _customerReminderService.UpdateCustomerReminder(customerReminder);
                }
            }
            return new NullJsonResult();
        }

        [HttpGet]
        public IActionResult CustomCustomerAttributeFields()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();
            var list = new List<Tuple<string, string>>();
            foreach (var item in _customerAttributeService.GetAllCustomerAttributes())
            {
                if (item.AttributeControlType == AttributeControlType.Checkboxes ||
                    item.AttributeControlType == AttributeControlType.DropdownList ||
                    item.AttributeControlType == AttributeControlType.RadioList)
                {
                    foreach (var value in item.CustomerAttributeValues)
                    {
                        list.Add(Tuple.Create(string.Format("{0}:{1}", item.Id, value.Id), item.Name + "->" + value.Name));
                    }
                }
            }
            var customer = list.Select(x => new { Id = x.Item1, Name = x.Item2 });
            return Json(customer);
        }
        #endregion

        #endregion


        #region Levels

        [HttpPost]
        public IActionResult Levels(string customerReminderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            var customerReminder = _customerReminderService.GetCustomerReminderById(customerReminderId);
            var gridModel = new DataSourceResult
            {
                Data = customerReminder.Levels.Select(x => new
                { Id = x.Id, Name = x.Name, Level = x.Level }).OrderBy(x => x.Level),
                Total = customerReminder.Levels.Count()
            };
            return Json(gridModel);
        }

        public IActionResult AddLevel(string customerReminderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();
            var customerReminder = _customerReminderService.GetCustomerReminderById(customerReminderId);
            var model = new CustomerReminderModel.ReminderLevelModel();
            model.CustomerReminderId = customerReminderId;
            PrepareModel(model, customerReminder);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult AddLevel(CustomerReminderModel.ReminderLevelModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();
            var customerReminder = _customerReminderService.GetCustomerReminderById(model.CustomerReminderId);
            if (customerReminder == null)
            {
                return RedirectToAction("List");
            }
            if (customerReminder.Levels.Where(x => x.Level == model.Level).Count() > 0)
            {
                ModelState.AddModelError("Error-LevelExists", _localizationService.GetResource("Admin.Customers.CustomerReminderLevel.Exists"));
            }

            if (ModelState.IsValid)
            {

                var level = new CustomerReminder.ReminderLevel()
                {
                    Name = model.Name,
                    Level = model.Level,
                    BccEmailAddresses = model.BccEmailAddresses,
                    Body = model.Body,
                    EmailAccountId = model.EmailAccountId,
                    Subject = model.Subject,
                    Day = model.Day,
                    Hour = model.Hour,
                    Minutes = model.Minutes,
                };

                customerReminder.Levels.Add(level);
                _customerReminderService.UpdateCustomerReminder(customerReminder);
                _customerActivityService.InsertActivity("AddNewCustomerReminderLevel", customerReminder.Id, _localizationService.GetResource("ActivityLog.AddNewCustomerReminderLevel"), customerReminder.Name);
                SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerReminderLevel.Added"));
                return continueEditing ? RedirectToAction("EditLevel", new { customerReminderId = customerReminder.Id, cid = level.Id }) : RedirectToAction("Edit", new { id = customerReminder.Id });
            }
            PrepareModel(model, customerReminder);
            return View(model);
        }

        public IActionResult EditLevel(string customerReminderId, string cid)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            var customerReminder = _customerReminderService.GetCustomerReminderById(customerReminderId);
            if (customerReminder == null)
            {
                return RedirectToAction("List");
            }

            var level = customerReminder.Levels.FirstOrDefault(x => x.Id == cid);
            if (level == null)
                return RedirectToAction("List");


            var model = level.ToModel();
            model.CustomerReminderId = customerReminderId;
            PrepareModel(model, customerReminder);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult EditLevel(string customerReminderId, string cid, CustomerReminderModel.ReminderLevelModel model, bool continueEditing)
        {
            var customerReminder = _customerReminderService.GetCustomerReminderById(customerReminderId);
            if (customerReminder == null)
                return RedirectToAction("List");

            var level = customerReminder.Levels.FirstOrDefault(x => x.Id == cid);
            if (level == null)
                return RedirectToAction("List");

            if (level.Level != model.Level)
            {
                if (customerReminder.Levels.Where(x => x.Level == model.Level).Count() > 0)
                {
                    ModelState.AddModelError("Error-LevelExists", _localizationService.GetResource("Admin.Customers.CustomerReminderLevel.Exists"));
                }
            }
            try
            {
                if (ModelState.IsValid)
                {
                    level.Level = model.Level;
                    level.Name = model.Name;
                    level.Subject = model.Subject;
                    level.BccEmailAddresses = model.BccEmailAddresses;
                    level.Body = model.Body;
                    level.EmailAccountId = model.EmailAccountId;
                    level.Day = model.Day;
                    level.Hour = model.Hour;
                    level.Minutes = model.Minutes;
                    _customerReminderService.UpdateCustomerReminder(customerReminder);

                    _customerActivityService.InsertActivity("EditCustomerReminderCondition", customerReminder.Id, _localizationService.GetResource("ActivityLog.EditCustomerReminderLevel"), customerReminder.Name);

                    SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerReminderLevel.Updated"));
                    return continueEditing ? RedirectToAction("EditLevel", new { customerReminderId = customerReminder.Id, cid = level.Id }) : RedirectToAction("Edit", new { id = customerReminder.Id });
                }

                PrepareModel(model, customerReminder);
                return View(model);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("Edit", new { id = customerReminder.Id });
            }
        }

        [HttpPost]
        public IActionResult DeleteLevel(string Id, string customerReminderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageReminders))
                return AccessDeniedView();

            var customerReminder = _customerReminderService.GetCustomerReminderById(customerReminderId);
            var level = customerReminder.Levels.FirstOrDefault(x => x.Id == Id);
            customerReminder.Levels.Remove(level);
            _customerReminderService.UpdateCustomerReminder(customerReminder);

            return new NullJsonResult();
        }


        #endregion

    }
}
