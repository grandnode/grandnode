using Grand.Domain.Catalog;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Services.Vendors;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Web.Areas.Admin.Models.Customers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        private readonly IDateTimeHelper _dateTimeHelper;

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
            IEmailAccountService emailAccountService,
            IDateTimeHelper dateTimeHelper)
        {
            _customerReminderViewModelService = customerReminderViewModelService;
            _customerService = customerService;
            _customerAttributeService = customerAttributeService;
            _customerTagService = customerTagService;
            _localizationService = localizationService;
            _manufacturerService = manufacturerService;
            _storeService = storeService;
            _vendorService = vendorService;
            _customerReminderService = customerReminderService;
            _emailAccountService = emailAccountService;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Customer reminders

        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List() => View();

        [HttpPost]
        public async Task<IActionResult> List(DataSourceRequest command)
        {
            var customeractions = await _customerReminderService.GetCustomerReminders();
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
        public async Task<IActionResult> Create(CustomerReminderModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var customerreminder = await _customerReminderViewModelService.InsertCustomerReminderModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerReminder.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = customerreminder.Id }) : RedirectToAction("List");
            }
            //If we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var customerReminder = await _customerReminderService.GetCustomerReminderById(id);
            if (customerReminder == null)
                return RedirectToAction("List");
            var model = customerReminder.ToModel(_dateTimeHelper);
            model.ConditionCount = customerReminder.Conditions.Count();
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(CustomerReminderModel model, bool continueEditing)
        {
            var customerreminder = await _customerReminderService.GetCustomerReminderById(model.Id);
            if (customerreminder == null)
                return RedirectToAction("List");
            try
            {
                if (ModelState.IsValid)
                {
                    customerreminder = await _customerReminderViewModelService.UpdateCustomerReminderModel(customerreminder, model);
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
        public async Task<IActionResult> Run(string Id)
        {
            var reminder = await _customerReminderService.GetCustomerReminderById(Id);
            if (reminder == null)
                return RedirectToAction("List");

            await _customerReminderViewModelService.RunReminder(reminder);
            SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerReminder.Run"));
            return RedirectToAction("Edit", new { id = Id });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var customerReminder = await _customerReminderService.GetCustomerReminderById(id);
            if (customerReminder == null)
                return RedirectToAction("List");
            try
            {
                if (ModelState.IsValid)
                {
                    await _customerReminderViewModelService.DeleteCustomerReminder(customerReminder);
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
        public async Task<IActionResult> History(DataSourceRequest command, string customerReminderId)
        {
            //we use own own binder for searchCustomerRoleIds property 
            var history = await _customerReminderService.GetAllCustomerReminderHistory(customerReminderId,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);
            var items = new List<SerializeCustomerReminderHistoryModel>();
            foreach (var item in history)
            {
                items.Add(await _customerReminderViewModelService.PrepareHistoryModelForList(item));
            }
            var gridModel = new DataSourceResult
            {
                Data = items,
                Total = history.TotalCount
            };
            return Json(gridModel);
        }

        #endregion

        #region Condition

        [HttpPost]
        public async Task<IActionResult> Conditions(string customerReminderId)
        {
            var customerReminder = await _customerReminderService.GetCustomerReminderById(customerReminderId);
            var gridModel = new DataSourceResult
            {
                Data = customerReminder.Conditions.Select(x => new
                { Id = x.Id, Name = x.Name, Condition = x.Condition.ToString() }),
                Total = customerReminder.Conditions.Count()
            };
            return Json(gridModel);
        }

        public async Task<IActionResult> AddCondition(string customerReminderId)
        {
            var customerReminder = await _customerReminderService.GetCustomerReminderById(customerReminderId);
            if (customerReminder == null)
                return RedirectToAction("Edit", new { id = customerReminderId });

            var model = _customerReminderViewModelService.PrepareConditionModel(customerReminder);
            return View(model);

        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> AddCondition(CustomerReminderModel.ConditionModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var customerReminder = await _customerReminderService.GetCustomerReminderById(model.CustomerReminderId);
                if (customerReminder == null)
                {
                    return RedirectToAction("List");
                }
                var condition = await _customerReminderViewModelService.InsertConditionModel(customerReminder, model);
                SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerReminder.Condition.Added"));

                return continueEditing ? RedirectToAction("EditCondition", new { customerReminderId = customerReminder.Id, cid = condition.Id }) : RedirectToAction("Edit", new { id = customerReminder.Id });
            }

            return View(model);
        }

        public async Task<IActionResult> EditCondition(string customerReminderId, string cid)
        {
            var customerReminder = await _customerReminderService.GetCustomerReminderById(customerReminderId);
            if (customerReminder == null)
                return RedirectToAction("List");

            var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == cid);
            if (condition == null)
                return RedirectToAction("List");
            var model = _customerReminderViewModelService.PrepareConditionModel(customerReminder, condition);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> EditCondition(string customerReminderId, string cid, CustomerReminderModel.ConditionModel model, bool continueEditing)
        {
            var customerReminder = await _customerReminderService.GetCustomerReminderById(customerReminderId);
            if (customerReminder == null)
                return RedirectToAction("List");

            var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == cid);
            if (condition == null)
                return RedirectToAction("List");
            try
            {
                if (ModelState.IsValid)
                {
                    condition = await _customerReminderViewModelService.UpdateConditionModel(customerReminder, condition, model);
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
        public async Task<IActionResult> ConditionDelete(string Id, string customerReminderId)
        {
            if (ModelState.IsValid)
            {
                await _customerReminderViewModelService.ConditionDelete(Id, customerReminderId);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);

        }

        [HttpPost]
        public async Task<IActionResult> ConditionDeletePosition(string id, string customerReminderId, string conditionId)
        {
            if (ModelState.IsValid)
            {
                await _customerReminderViewModelService.ConditionDeletePosition(id, customerReminderId, conditionId);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        #region Condition Category

        [HttpPost]
        public async Task<IActionResult> ConditionCategory(string customerReminderId, string conditionId, [FromServices] ICategoryService categoryService)
        {
            var customerReminder = await _customerReminderService.GetCustomerReminderById(customerReminderId);
            var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == conditionId);
            var items = new List<(string Id, string CategoryName)>();
            foreach (var item in condition.Categories)
            {
                var category = await categoryService.GetCategoryById(item);
                items.Add((item, category?.Name));
            }
            var gridModel = new DataSourceResult
            {
                Data = items.Select(x => new { Id = x.Id, CategoryName = x.CategoryName }),
                Total = customerReminder.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        public IActionResult CategoryAddPopup(string customerReminderId, string conditionId)
        {
            var model = new CustomerReminderModel.ConditionModel.AddCategoryConditionModel
            {
                ConditionId = conditionId,
                CustomerReminderId = customerReminderId
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CategoryAddPopupList(DataSourceRequest command, CustomerReminderModel.ConditionModel.AddCategoryConditionModel model, [FromServices] ICategoryService categoryService)
        {
            var categories = await categoryService.GetAllCategories(model.SearchCategoryName,
                pageIndex: command.Page - 1, pageSize: command.PageSize, showHidden: true);
            var items = new List<CategoryModel>();
            foreach (var item in categories)
            {
                var categoryModel = item.ToModel();
                categoryModel.Breadcrumb = await categoryService.GetFormattedBreadCrumb(item);
                items.Add(categoryModel);
            }
            var gridModel = new DataSourceResult
            {
                Data = items,
                Total = categories.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public async Task<IActionResult> CategoryAddPopup(CustomerReminderModel.ConditionModel.AddCategoryConditionModel model)
        {
            if (model.SelectedCategoryIds != null)
            {
                await _customerReminderViewModelService.InsertCategoryConditionModel(model);
            }
            ViewBag.RefreshPage = true;
            return View(model);
        }

        #endregion

        #region Condition Manufacturer
        [HttpPost]
        public async Task<IActionResult> ConditionManufacturer(string customerReminderId, string conditionId)
        {
            var customerReminder = await _customerReminderService.GetCustomerReminderById(customerReminderId);
            var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == conditionId);
            var items = new List<(string Id, string ManufacturerName)>();
            foreach (var item in condition.Manufacturers)
            {
                var manuf = await _manufacturerService.GetManufacturerById(item);
                items.Add((item, manuf?.Name));
            }
            var gridModel = new DataSourceResult
            {
                Data = items.Select(x => new { Id = x.Id, ManufacturerName = x.ManufacturerName }),
                Total = customerReminder.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        public IActionResult ManufacturerAddPopup(string customerReminderId, string conditionId)
        {
            var model = new CustomerReminderModel.ConditionModel.AddManufacturerConditionModel
            {
                ConditionId = conditionId,
                CustomerReminderId = customerReminderId
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ManufacturerAddPopupList(DataSourceRequest command, CustomerReminderModel.ConditionModel.AddManufacturerConditionModel model)
        {
            var manufacturers = await _manufacturerService.GetAllManufacturers(model.SearchManufacturerName, "",
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
        public async Task<IActionResult> ManufacturerAddPopup(CustomerReminderModel.ConditionModel.AddManufacturerConditionModel model)
        {
            if (model.SelectedManufacturerIds != null)
            {
                await _customerReminderViewModelService.InsertManufacturerConditionModel(model);
            }
            ViewBag.RefreshPage = true;
            return View(model);
        }
        #endregion

        #region Condition Product

        [HttpPost]
        public async Task<IActionResult> ConditionProduct(string customerReminderId, string conditionId, [FromServices] IProductService productService)
        {
            var customerReminder = await _customerReminderService.GetCustomerReminderById(customerReminderId);
            var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == conditionId);
            var items = new List<(string Id, string ProductName)>();
            foreach (var item in condition.Products)
            {
                var prod = await productService.GetProductById(item);
                items.Add((item, prod?.Name));
            }
            var gridModel = new DataSourceResult
            {
                Data = items.Select(x => new { Id = x.Id, ProductName = x.ProductName }),
                Total = customerReminder.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        public async Task<IActionResult> ProductAddPopup(string customerReminderId, string conditionId)
        {
            var model = await _customerReminderViewModelService.PrepareProductToConditionModel(customerReminderId, conditionId);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ProductAddPopupList(DataSourceRequest command, CustomerActionConditionModel.AddProductToConditionModel model)
        {
            var gridModel = new DataSourceResult();
            var products = await _customerReminderViewModelService.PrepareProductModel(model, command.Page, command.PageSize);
            gridModel.Data = products.products.ToList();
            gridModel.Total = products.totalCount;

            return Json(gridModel);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public async Task<IActionResult> ProductAddPopup(CustomerReminderModel.ConditionModel.AddProductToConditionModel model)
        {
            if (model.SelectedProductIds != null)
            {
                await _customerReminderViewModelService.InsertProductToConditionModel(model);
            }
            ViewBag.RefreshPage = true;
            return View(model);
        }


        #endregion

        #region Customer Tags

        [HttpPost]
        public async Task<IActionResult> ConditionCustomerTag(string customerReminderId, string conditionId)
        {
            var customerReminder = await _customerReminderService.GetCustomerReminderById(customerReminderId);
            var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == conditionId);
            var items = new List<(string Id, string CustomerTag)>();
            foreach (var item in condition.CustomerTags)
            {
                var tag = await _customerTagService.GetCustomerTagById(item);
                items.Add((item, tag?.Name));
            }

            var gridModel = new DataSourceResult
            {
                Data = items.Select(x => new { Id = x.Id, CustomerTag = x.CustomerTag }),
                Total = customerReminder.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }
        [HttpPost]
        public async Task<IActionResult> ConditionCustomerTagInsert(CustomerReminderModel.ConditionModel.AddCustomerTagConditionModel model)
        {
            if (ModelState.IsValid)
            {
                await _customerReminderViewModelService.InsertCustomerTagConditionModel(model);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }


        [HttpGet]
        public async Task<IActionResult> CustomerTags()
        {
            var customerTag = (await _customerTagService.GetAllCustomerTags()).Select(x => new { Id = x.Id, Name = x.Name });
            return Json(customerTag);
        }
        #endregion

        #region Condition Customer role

        [HttpPost]
        public async Task<IActionResult> ConditionCustomerRole(string customerReminderId, string conditionId)
        {
            var customerReminder = await _customerReminderService.GetCustomerReminderById(customerReminderId);
            var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == conditionId);
            var items = new List<(string Id, string CustomerRole)>();
            foreach (var item in condition.CustomerRoles)
            {
                var role = await _customerService.GetCustomerRoleById(item);
                items.Add((item, role?.Name));
            }
            var gridModel = new DataSourceResult
            {
                Data = items.Select(x => new { Id = x.Id, CustomerRole = x.CustomerRole }),
                Total = customerReminder.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        [HttpPost]
        public async Task<IActionResult> ConditionCustomerRoleInsert(CustomerReminderModel.ConditionModel.AddCustomerRoleConditionModel model)
        {
            if (ModelState.IsValid)
            {
                await _customerReminderViewModelService.InsertCustomerRoleConditionModel(model);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [HttpGet]
        public async Task<IActionResult> CustomerRoles()
        {
            var customerRole = (await _customerService.GetAllCustomerRoles()).Select(x => new { Id = x.Id, Name = x.Name });
            return Json(customerRole);
        }

        #endregion

        #region Condition Customer Register
        [HttpPost]
        public async Task<IActionResult> ConditionCustomerRegister(string customerReminderId, string conditionId)
        {
            var customerReminder = await _customerReminderService.GetCustomerReminderById(customerReminderId);
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
        public async Task<IActionResult> ConditionCustomerRegisterInsert(CustomerReminderModel.ConditionModel.AddCustomerRegisterConditionModel model)
        {
            if (ModelState.IsValid)
            {
                await _customerReminderViewModelService.InsertCustomerRegisterConditionModel(model);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }
        [HttpPost]
        public async Task<IActionResult> ConditionCustomerRegisterUpdate(CustomerReminderModel.ConditionModel.AddCustomerRegisterConditionModel model)
        {
            if (ModelState.IsValid)
            {
                await _customerReminderViewModelService.UpdateCustomerRegisterConditionModel(model);
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

        private async Task<string> CustomerAttribute(string registerField)
        {
            string _field = registerField;
            var _rf = registerField.Split(':');
            if (_rf.Count() > 1)
            {
                var ca = await _customerAttributeService.GetCustomerAttributeById(_rf.FirstOrDefault());
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
        public async Task<IActionResult> ConditionCustomCustomerAttribute(string customerReminderId, string conditionId)
        {
            var customerReminder = await _customerReminderService.GetCustomerReminderById(customerReminderId);
            var condition = customerReminder.Conditions.FirstOrDefault(x => x.Id == conditionId);
            var items = new List<(string Id, string CustomerAttributeId, string CustomerAttributeName, string CustomerAttributeValue)>();
            foreach (var item in condition.CustomCustomerAttributes)
            {
                items.Add((item.Id, await CustomerAttribute(item.RegisterField), item.RegisterField, item.RegisterValue));
            }
            var gridModel = new DataSourceResult
            {
                Data = items.Select(x => new { Id = x.Id, CustomerAttributeId = x.CustomerAttributeId, CustomerAttributeName = x.CustomerAttributeName, CustomerAttributeValue = x.CustomerAttributeValue }),
                Total = customerReminder.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        [HttpPost]
        public async Task<IActionResult> ConditionCustomCustomerAttributeInsert(CustomerReminderModel.ConditionModel.AddCustomCustomerAttributeConditionModel model)
        {
            if (ModelState.IsValid)
            {
                await _customerReminderViewModelService.InsertCustomCustomerAttributeConditionModel(model);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }
        [HttpPost]
        public async Task<IActionResult> ConditionCustomCustomerAttributeUpdate(CustomerReminderModel.ConditionModel.AddCustomCustomerAttributeConditionModel model)
        {
            if (ModelState.IsValid)
            {
                await _customerReminderViewModelService.UpdateCustomCustomerAttributeConditionModel(model);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        [HttpGet]
        public async Task<IActionResult> CustomCustomerAttributeFields()
        {
            var list = new List<Tuple<string, string>>();
            foreach (var item in await _customerAttributeService.GetAllCustomerAttributes())
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
        public async Task<IActionResult> Levels(string customerReminderId)
        {
            var customerReminder = await _customerReminderService.GetCustomerReminderById(customerReminderId);
            var gridModel = new DataSourceResult
            {
                Data = customerReminder.Levels.Select(x => new
                { Id = x.Id, Name = x.Name, Level = x.Level }).OrderBy(x => x.Level),
                Total = customerReminder.Levels.Count()
            };
            return Json(gridModel);
        }

        public async Task<IActionResult> AddLevel(string customerReminderId)
        {
            var customerReminder = await _customerReminderService.GetCustomerReminderById(customerReminderId);
            var model = new CustomerReminderModel.ReminderLevelModel();
            model.CustomerReminderId = customerReminderId;
            await _customerReminderViewModelService.PrepareReminderLevelModel(model, customerReminder);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> AddLevel(CustomerReminderModel.ReminderLevelModel model, bool continueEditing)
        {
            var customerReminder = await _customerReminderService.GetCustomerReminderById(model.CustomerReminderId);
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
            await _customerReminderViewModelService.PrepareReminderLevelModel(model, customerReminder);
            return View(model);
        }

        public async Task<IActionResult> EditLevel(string customerReminderId, string cid)
        {
            var customerReminder = await _customerReminderService.GetCustomerReminderById(customerReminderId);
            if (customerReminder == null)
            {
                return RedirectToAction("List");
            }

            var level = customerReminder.Levels.FirstOrDefault(x => x.Id == cid);
            if (level == null)
                return RedirectToAction("List");

            var model = level.ToModel();
            model.CustomerReminderId = customerReminderId;
            await _customerReminderViewModelService.PrepareReminderLevelModel(model, customerReminder);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> EditLevel(string customerReminderId, string cid, CustomerReminderModel.ReminderLevelModel model, bool continueEditing)
        {
            var customerReminder = await _customerReminderService.GetCustomerReminderById(customerReminderId);
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
                    level = await _customerReminderViewModelService.UpdateReminderLevel(customerReminder, level, model);
                    SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerReminderLevel.Updated"));
                    return continueEditing ? RedirectToAction("EditLevel", new { customerReminderId = customerReminder.Id, cid = level.Id }) : RedirectToAction("Edit", new { id = customerReminder.Id });
                }
                await _customerReminderViewModelService.PrepareReminderLevelModel(model, customerReminder);
                return View(model);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("Edit", new { id = customerReminder.Id });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLevel(string Id, string customerReminderId)
        {
            if (ModelState.IsValid)
            {
                await _customerReminderViewModelService.DeleteLevel(Id, customerReminderId);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }
        #endregion
    }
}