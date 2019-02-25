using Grand.Core.Domain.Catalog;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Services.Vendors;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Customers;
using Grand.Web.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Reminders)]
    public partial class CustomerReminderController : BaseAdminController
    {
        #region Fields
        private readonly ICustomerReminderViewModelService _customerReminderViewModelService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerAttributeService _customerAttributeService;
        private readonly ICustomerTagService _customerTagService;
        private readonly ILocalizationService _localizationService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IStoreService _storeService;
        private readonly IVendorService _vendorService;
        private readonly ICustomerReminderService _customerReminderService;
        private readonly IEmailAccountService _emailAccountService;

        #endregion

        #region Constructors

        public CustomerReminderController(
            ICustomerReminderViewModelService customerReminderViewModelService,
            ICustomerService customerService,
            ICustomerAttributeService customerAttributeService,
            ICustomerTagService customerTagService,
            ILocalizationService localizationService,
            IManufacturerService manufacturerService,
            IStoreService storeService,
            IVendorService vendorService,
            ICustomerReminderService customerReminderService,
            IEmailAccountService emailAccountService)
        {
            this._customerReminderViewModelService = customerReminderViewModelService;
            this._customerService = customerService;
            this._customerAttributeService = customerAttributeService;
            this._customerTagService = customerTagService;
            this._localizationService = localizationService;
            this._manufacturerService = manufacturerService;
            this._storeService = storeService;
            this._vendorService = vendorService;
            this._customerReminderService = customerReminderService;
            this._emailAccountService = emailAccountService;
        }

        #endregion

        #region Customer reminders

        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List() => View();

        [HttpPost]
        public IActionResult List(DataSourceRequest command)
        {
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
            var model = _customerReminderViewModelService.PrepareCustomerReminderModel();
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(CustomerReminderModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var customerreminder = _customerReminderViewModelService.InsertCustomerReminderModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerReminder.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = customerreminder.Id }) : RedirectToAction("List");
            }
            //If we got this far, something failed, redisplay form
            return View(model);
        }

        public IActionResult Edit(string id)
        {
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
            var customerreminder = _customerReminderService.GetCustomerReminderById(model.Id);
            if (customerreminder == null)
                return RedirectToAction("List");
            try
            {
                if (ModelState.IsValid)
                {
                    customerreminder = _customerReminderViewModelService.UpdateCustomerReminderModel(customerreminder, model);
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
            var reminder = _customerReminderService.GetCustomerReminderById(Id);
            if (reminder == null)
                return RedirectToAction("List");

            _customerReminderViewModelService.RunReminder(reminder);
            SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerReminder.Run"));
            return RedirectToAction("Edit", new { id = Id });
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            var customerReminder = _customerReminderService.GetCustomerReminderById(id);
            if (customerReminder == null)
                return RedirectToAction("List");
            try
            {
                if (ModelState.IsValid)
                {
                    _customerReminderViewModelService.DeleteCustomerReminder(customerReminder);
                    SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerReminder.Deleted"));
                    return RedirectToAction("List");
                }
                else
                {
                    return RedirectToAction("Edit", new { id = id });
                }
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
            var history = _customerReminderService.GetAllCustomerReminderHistory(customerReminderId,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = history.Select(x => _customerReminderViewModelService.PrepareHistoryModelForList(x)),
                Total = history.TotalCount
            };

            return Json(gridModel);
        }

        #endregion

        #region Condition

        [HttpPost]
        public IActionResult Conditions(string customerReminderId)
        {
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
            var customerReminder = _customerReminderService.GetCustomerReminderById(customerReminderId);
            if (customerReminder == null)
                return RedirectToAction("Edit", new { id = customerReminderId });

            var model = _customerReminderViewModelService.PrepareConditionModel(customerReminder);
            return View(model);

        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult AddCondition(CustomerReminderModel.ConditionModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var customerReminder = _customerReminderService.GetCustomerReminderById(model.CustomerReminderId);
                if (customerReminder == null)
                {
                    return RedirectToAction("List");
                }
                var condition = _customerReminderViewModelService.InsertConditionModel(customerReminder, model);
                SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerReminder.Condition.Added"));

                return continueEditing ? RedirectToAction("EditCondition", new { customerReminderId = customerReminder.Id, cid = condition.Id }) : RedirectToAction("Edit", new { id = customerReminder.Id });
            }

            return View(model);
        }

        public IActionResult EditCondition(string customerReminderId, string cid)
        {
            var customerReminder = _customerReminderService.GetCustomerReminderById(customerReminderId);
            if (customerReminder == null)
                return RedirectToAction("List");

            var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == cid);
            if (condition == null)
                return RedirectToAction("List");
            var model = _customerReminderViewModelService.PrepareConditionModel(customerReminder, condition);
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
                    condition = _customerReminderViewModelService.UpdateConditionModel(customerReminder, condition, model);
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
            if (ModelState.IsValid)
            {
                _customerReminderViewModelService.ConditionDelete(Id, customerReminderId);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);

        }

        [HttpPost]
        public IActionResult ConditionDeletePosition(string id, string customerReminderId, string conditionId)
        {
            if (ModelState.IsValid)
            {
                _customerReminderViewModelService.ConditionDeletePosition(id, customerReminderId, conditionId);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        #region Condition Category

        [HttpPost]
        public IActionResult ConditionCategory(string customerReminderId, string conditionId, [FromServices] ICategoryService categoryService)
        {
            var customerReminder = _customerReminderService.GetCustomerReminderById(customerReminderId);
            var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == conditionId);

            var gridModel = new DataSourceResult
            {
                Data = condition != null ? condition.Categories.Select(z => new { Id = z, CategoryName = categoryService.GetCategoryById(z) != null ? categoryService.GetCategoryById(z).Name : "" }) : null,
                Total = customerReminder.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        public IActionResult CategoryAddPopup(string customerReminderId, string conditionId)
        {
            var model = new CustomerReminderModel.ConditionModel.AddCategoryConditionModel();
            model.ConditionId = conditionId;
            model.CustomerReminderId = customerReminderId;
            return View(model);
        }

        [HttpPost]
        public IActionResult CategoryAddPopupList(DataSourceRequest command, CustomerReminderModel.ConditionModel.AddCategoryConditionModel model, [FromServices] ICategoryService categoryService)
        {
            var categories = categoryService.GetAllCategories(model.SearchCategoryName,
                pageIndex: command.Page - 1, pageSize: command.PageSize, showHidden: true);
            var gridModel = new DataSourceResult
            {
                Data = categories.Select(x =>
                {
                    var categoryModel = x.ToModel();
                    categoryModel.Breadcrumb = x.GetFormattedBreadCrumb(categoryService);
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
            if (model.SelectedCategoryIds != null)
            {
                _customerReminderViewModelService.InsertCategoryConditionModel(model);
            }
            ViewBag.RefreshPage = true;
            return View(model);
        }

        #endregion

        #region Condition Manufacturer
        [HttpPost]
        public IActionResult ConditionManufacturer(string customerReminderId, string conditionId)
        {
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
            if (model.SelectedManufacturerIds != null)
            {
                _customerReminderViewModelService.InsertManufacturerConditionModel(model);
            }
            ViewBag.RefreshPage = true;
            return View(model);
        }


        #endregion

        #region Condition Product

        [HttpPost]
        public IActionResult ConditionProduct(string customerReminderId, string conditionId, [FromServices] IProductService productService)
        {
            var customerReminder = _customerReminderService.GetCustomerReminderById(customerReminderId);
            var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == conditionId);

            var gridModel = new DataSourceResult
            {
                Data = condition != null ? condition.Products.Select(z => new { Id = z, ProductName = productService.GetProductById(z) != null ? productService.GetProductById(z).Name : "" }) : null,
                Total = customerReminder.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        public IActionResult ProductAddPopup(string customerReminderId, string conditionId)
        {
            var model = _customerReminderViewModelService.PrepareProductToConditionModel(customerReminderId, conditionId);
            return View(model);
        }

        [HttpPost]
        public IActionResult ProductAddPopupList(DataSourceRequest command, CustomerActionConditionModel.AddProductToConditionModel model)
        {
            var gridModel = new DataSourceResult();
            var products = _customerReminderViewModelService.PrepareProductModel(model, command.Page, command.PageSize);
            gridModel.Data = products.products.ToList();
            gridModel.Total = products.totalCount;

            return Json(gridModel);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public IActionResult ProductAddPopup(CustomerReminderModel.ConditionModel.AddProductToConditionModel model)
        {
            if (model.SelectedProductIds != null)
            {
                _customerReminderViewModelService.InsertProductToConditionModel(model);
            }
            ViewBag.RefreshPage = true;
            return View(model);
        }


        #endregion

        #region Customer Tags

        [HttpPost]
        public IActionResult ConditionCustomerTag(string customerReminderId, string conditionId)
        {
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
            if (ModelState.IsValid)
            {
                _customerReminderViewModelService.InsertCustomerTagConditionModel(model);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }


        [HttpGet]
        public IActionResult CustomerTags()
        {
            var customerTag = _customerTagService.GetAllCustomerTags().Select(x => new { Id = x.Id, Name = x.Name });
            return Json(customerTag);
        }
        #endregion

        #region Condition Customer role

        [HttpPost]
        public IActionResult ConditionCustomerRole(string customerReminderId, string conditionId)
        {
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
            if (ModelState.IsValid)
            {
                _customerReminderViewModelService.InsertCustomerRoleConditionModel(model);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }


        [HttpGet]
        public IActionResult CustomerRoles()
        {
            var customerRole = _customerService.GetAllCustomerRoles().Select(x => new { Id = x.Id, Name = x.Name });
            return Json(customerRole);
        }

        #endregion

        #region Condition Customer Register
        [HttpPost]
        public IActionResult ConditionCustomerRegister(string customerReminderId, string conditionId)
        {
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
            if (ModelState.IsValid)
            {
                _customerReminderViewModelService.InsertCustomerRegisterConditionModel(model);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }
        [HttpPost]
        public IActionResult ConditionCustomerRegisterUpdate(CustomerReminderModel.ConditionModel.AddCustomerRegisterConditionModel model)
        {
            if (ModelState.IsValid)
            {
                _customerReminderViewModelService.UpdateCustomerRegisterConditionModel(model);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [HttpGet]
        public IActionResult CustomerRegisterFields()
        {
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
            if (ModelState.IsValid)
            {
                _customerReminderViewModelService.InsertCustomCustomerAttributeConditionModel(model);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }
        [HttpPost]
        public IActionResult ConditionCustomCustomerAttributeUpdate(CustomerReminderModel.ConditionModel.AddCustomCustomerAttributeConditionModel model)
        {
            if (ModelState.IsValid)
            {
                _customerReminderViewModelService.UpdateCustomCustomerAttributeConditionModel(model);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [HttpGet]
        public IActionResult CustomCustomerAttributeFields()
        {
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
            var customerReminder = _customerReminderService.GetCustomerReminderById(customerReminderId);
            var model = new CustomerReminderModel.ReminderLevelModel();
            model.CustomerReminderId = customerReminderId;
            _customerReminderViewModelService.PrepareReminderLevelModel(model, customerReminder);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult AddLevel(CustomerReminderModel.ReminderLevelModel model, bool continueEditing)
        {
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
                var level = _customerReminderViewModelService.InsertReminderLevel(customerReminder, model);
                SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerReminderLevel.Added"));
                return continueEditing ? RedirectToAction("EditLevel", new { customerReminderId = customerReminder.Id, cid = level.Id }) : RedirectToAction("Edit", new { id = customerReminder.Id });
            }
            _customerReminderViewModelService.PrepareReminderLevelModel(model, customerReminder);
            return View(model);
        }

        public IActionResult EditLevel(string customerReminderId, string cid)
        {
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
            _customerReminderViewModelService.PrepareReminderLevelModel(model, customerReminder);
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
                    level = _customerReminderViewModelService.UpdateReminderLevel(customerReminder, level, model);
                    SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerReminderLevel.Updated"));
                    return continueEditing ? RedirectToAction("EditLevel", new { customerReminderId = customerReminder.Id, cid = level.Id }) : RedirectToAction("Edit", new { id = customerReminder.Id });
                }
                _customerReminderViewModelService.PrepareReminderLevelModel(model, customerReminder);
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
            if (ModelState.IsValid)
            {
                _customerReminderViewModelService.DeleteLevel(Id, customerReminderId);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }
        #endregion
    }
}