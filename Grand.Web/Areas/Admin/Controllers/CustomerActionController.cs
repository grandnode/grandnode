using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Services.Vendors;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Customers;
using Grand.Web.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Services.Helpers;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Actions)]
    public partial class CustomerActionController : BaseAdminController
    {
        #region Fields
        private readonly ICustomerActionViewModelService _customerActionViewModelService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerAttributeService _customerAttributeService;
        private readonly ICustomerTagService _customerTagService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IStoreService _storeService;
        private readonly IVendorService _vendorService;
        private readonly ICustomerActionService _customerActionService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion

        #region Constructors

        public CustomerActionController(
            ICustomerActionViewModelService customerActionViewModelService,
            ICustomerService customerService,
            ICustomerAttributeService customerAttributeService,
            ICustomerTagService customerTagService,
            ILocalizationService localizationService,
            ICustomerActivityService customerActivityService,
            IProductService productService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IStoreService storeService,
            IVendorService vendorService,
            ICustomerActionService customerActionService,
            IProductAttributeService productAttributeService,
            ISpecificationAttributeService specificationAttributeService,
            IDateTimeHelper dateTimeHelper)
        {
            _customerActionViewModelService = customerActionViewModelService;
            _customerService = customerService;
            _customerAttributeService = customerAttributeService;
            _customerTagService = customerTagService;
            _localizationService = localizationService;
            _customerActivityService = customerActivityService;
            _productService = productService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _storeService = storeService;
            _vendorService = vendorService;
            _customerActionService = customerActionService;
            _productAttributeService = productAttributeService;
            _specificationAttributeService = specificationAttributeService;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion
        #region Utilities
        protected void CheckValidateModel(CustomerActionModel model)
        {
            if ((model.ReactionType == CustomerReactionTypeEnum.Banner) && String.IsNullOrEmpty(model.BannerId))
                ModelState.AddModelError("error", "Banner is required");
            if ((model.ReactionType == CustomerReactionTypeEnum.InteractiveForm) && String.IsNullOrEmpty(model.InteractiveFormId))
                ModelState.AddModelError("error", "Interactive form is required");
            if ((model.ReactionType == CustomerReactionTypeEnum.Email) && String.IsNullOrEmpty(model.MessageTemplateId))
                ModelState.AddModelError("error", "Email is required");
            if ((model.ReactionType == CustomerReactionTypeEnum.AssignToCustomerRole) && String.IsNullOrEmpty(model.CustomerRoleId))
                ModelState.AddModelError("error", "Customer role is required");
            if ((model.ReactionType == CustomerReactionTypeEnum.AssignToCustomerTag) && String.IsNullOrEmpty(model.CustomerTagId))
                ModelState.AddModelError("error", "Tag is required");
        }
        #endregion

        #region Customer Actions

        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List() => View();

        [HttpPost]
        public async Task<IActionResult> List(DataSourceRequest command)
        {
            var customeractions = await _customerActionService.GetCustomerActions();
            var actions = await _customerActionService.GetCustomerActionType();
            var gridModel = new DataSourceResult
            {
                Data = customeractions.Select(x => new { Id = x.Id, Name = x.Name, Active = x.Active, ActionType = actions.FirstOrDefault(y=>y.Id == x.ActionTypeId)?.Name }),
                Total = customeractions.Count()
            };
            return Json(gridModel);
        }

        public async Task<IActionResult> Create()
        {
            var model = await _customerActionViewModelService.PrepareCustomerActionModel();
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(CustomerActionModel model, bool continueEditing)
        {
            CheckValidateModel(model);
            if (ModelState.IsValid)
            {
                var customeraction = await _customerActionViewModelService.InsertCustomerActionModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerAction.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = customeraction.Id }) : RedirectToAction("List");
            }
            await _customerActionViewModelService.PrepareReactObjectModel(model);
            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var customerAction = await _customerActionService.GetCustomerActionById(id);
            if (customerAction == null)
                return RedirectToAction("List");

            var model = customerAction.ToModel(_dateTimeHelper);
            model.ConditionCount = customerAction.Conditions.Count();
            await _customerActionViewModelService.PrepareReactObjectModel(model);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(CustomerActionModel model, bool continueEditing)
        {
            CheckValidateModel(model);

            var customeraction = await _customerActionService.GetCustomerActionById(model.Id);
            if (customeraction == null)
                return RedirectToAction("List");
            try
            {
                if (ModelState.IsValid)
                {
                    customeraction = await _customerActionViewModelService.UpdateCustomerActionModel(customeraction, model);
                    SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerAction.Updated"));
                    return continueEditing ? RedirectToAction("Edit", new { id = customeraction.Id }) : RedirectToAction("List");
                }
                await _customerActionViewModelService.PrepareReactObjectModel(model);
                return View(model);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("Edit", new { id = customeraction.Id });
            }
        }

        [HttpPost]
        public async Task<IActionResult> History(DataSourceRequest command, string customerActionId)
        {
            var history = await _customerActionService.GetAllCustomerActionHistory(customerActionId,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);
            var items = new List<SerializeCustomerActionHistory>();
            foreach (var item in history)
            {
                items.Add(await _customerActionViewModelService.PrepareHistoryModelForList(item));
            }
            var gridModel = new DataSourceResult
            {
                Data = items,
                Total = history.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var customerAction = await _customerActionService.GetCustomerActionById(id);
            if (customerAction == null)
                return RedirectToAction("List");

            try
            {
                //activity log
                await _customerActivityService.InsertActivity("DeleteCustomerAction", customerAction.Id, _localizationService.GetResource("ActivityLog.DeleteCustomerAction"), customerAction.Name);
                await _customerActionService.DeleteCustomerAction(customerAction);
                SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerAction.Deleted"));
                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
                return RedirectToAction("Edit", new { id = customerAction.Id });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Conditions(string customerActionId)
        {
            var conditions = await _customerActionService.GetCustomerActionById(customerActionId);
            var gridModel = new DataSourceResult
            {
                Data = conditions.Conditions.Select(x => new { Id = x.Id, Name = x.Name, Condition = x.CustomerActionConditionType.ToString() }),
                Total = conditions.Conditions.Count()
            };
            return Json(gridModel);
        }

        #endregion

        #region Conditions

        public async Task<IActionResult> AddCondition(string customerActionId)
        {
            var model = await _customerActionViewModelService.PrepareCustomerActionConditionModel(customerActionId);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> AddCondition(CustomerActionConditionModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var customerAction = await _customerActionViewModelService.InsertCustomerActionConditionModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerActionCondition.Added"));
                return continueEditing ? RedirectToAction("EditCondition", new { customerActionId = customerAction.customerActionId, cid = customerAction.conditionId }) : RedirectToAction("Edit", new { id = customerAction.customerActionId });
            }
            return View(model);
        }

        public async Task<IActionResult> EditCondition(string customerActionId, string cid)
        {
            var customerAction = await _customerActionService.GetCustomerActionById(customerActionId);
            if (customerAction == null)
                return RedirectToAction("List");
            var actionType = await _customerActionService.GetCustomerActionTypeById(customerAction.ActionTypeId);
            var condition = customerAction.Conditions.FirstOrDefault(x => x.Id == cid);
            if (condition == null)
                return RedirectToAction("List");

            var model = condition.ToModel();
            model.CustomerActionId = customerActionId;

            foreach (var item in actionType.ConditionType)
            {
                model.CustomerActionConditionType.Add(new SelectListItem()
                {
                    Value = item.ToString(),
                    Text = ((CustomerActionConditionTypeEnum)item).ToString()
                });
            }

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> EditCondition(string customerActionId, string cid, CustomerActionConditionModel model, bool continueEditing)
        {
            var customerAction = await _customerActionService.GetCustomerActionById(customerActionId);
            if (customerAction == null)
                return RedirectToAction("List");

            var condition = customerAction.Conditions.FirstOrDefault(x => x.Id == cid);
            if (condition == null)
                return RedirectToAction("List");
            try
            {
                if (ModelState.IsValid)
                {
                    await _customerActionViewModelService.UpdateCustomerActionConditionModel(customerAction, condition, model);
                    SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerActionCondition.Updated"));
                    return continueEditing ? RedirectToAction("EditCondition", new { customerActionId = customerAction.Id, cid = condition.Id }) : RedirectToAction("Edit", new { id = customerAction.Id });
                }
                return View(model);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("Edit", new { id = customerAction.Id });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConditionDelete(string Id, string customerActionId)
        {
            await _customerActionViewModelService.ConditionDelete(Id, customerActionId);
            return new NullJsonResult();
        }

        [HttpPost]
        public async Task<IActionResult> ConditionDeletePosition(string id, string customerActionId, string conditionId)
        {
            await _customerActionViewModelService.ConditionDeletePosition(id, customerActionId, conditionId);

            return new NullJsonResult();
        }
        #endregion

        #region Condition Product

        [HttpPost]
        public async Task<IActionResult> ConditionProduct(string customerActionId, string conditionId)
        {
            var customerActions = await _customerActionService.GetCustomerActionById(customerActionId);
            var condition = customerActions.Conditions.FirstOrDefault(x => x.Id == conditionId);
            var items = new List<(string Id, string ProductName)>();
            foreach (var item in condition.Products)
            {
                var product = (await _productService.GetProductById(item))?.Name;
                items.Add((item, product));
            }
            var gridModel = new DataSourceResult
            {
                Data = items.Select(x=>new { Id = x.Id, ProductName = x.ProductName }),
                Total = customerActions.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }


        public async Task<IActionResult> ProductAddPopup(string customerActionId, string conditionId)
        {
            var model = await _customerActionViewModelService.PrepareAddProductToConditionModel(customerActionId, conditionId);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ProductAddPopupList(DataSourceRequest command, CustomerActionConditionModel.AddProductToConditionModel model)
        {
            var products = await _customerActionViewModelService.PrepareProductModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult();
            gridModel.Data = products.products.ToList();
            gridModel.Total = products.totalCount;

            return Json(gridModel);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public async Task<IActionResult> ProductAddPopup(CustomerActionConditionModel.AddProductToConditionModel model)
        {
            if (model.SelectedProductIds != null)
            {
                await _customerActionViewModelService.InsertProductToConditionModel(model);
            }
            ViewBag.RefreshPage = true;
            return View(model);
        }
        #endregion

        #region Condition Category

        [HttpPost]
        public async Task<IActionResult> ConditionCategory(string customerActionId, string conditionId)
        {
            var customerActions = await _customerActionService.GetCustomerActionById(customerActionId);
            var condition = customerActions.Conditions.FirstOrDefault(x => x.Id == conditionId);
            var items = new List<(string Id, string CategoryName)>();
            foreach (var item in condition.Categories)
            {
                var category = (await _categoryService.GetCategoryById(item))?.Name;
                items.Add((item, category));
            }
            var gridModel = new DataSourceResult
            {
                Data = items.Select(x => new { Id = x.Id, CategoryName = x.CategoryName }),
                Total = customerActions.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        public IActionResult CategoryAddPopup(string customerActionId, string conditionId)
        {
            var model = new CustomerActionConditionModel.AddCategoryConditionModel
            {
                CustomerActionConditionId = conditionId,
                CustomerActionId = customerActionId
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CategoryAddPopupList(DataSourceRequest command, CustomerActionConditionModel.AddCategoryConditionModel model)
        {
            var categories = await _categoryService.GetAllCategories(model.SearchCategoryName,
                pageIndex: command.Page - 1, pageSize: command.PageSize, showHidden: true);
            var items = new List<CategoryModel>();
            foreach (var x in categories)
            {
                var categoryModel = x.ToModel();
                categoryModel.Breadcrumb = await _categoryService.GetFormattedBreadCrumb(x);
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
        public async Task<IActionResult> CategoryAddPopup(CustomerActionConditionModel.AddCategoryConditionModel model)
        {
            if (model.SelectedCategoryIds != null)
            {
                await _customerActionViewModelService.InsertCategoryConditionModel(model);
            }
            ViewBag.RefreshPage = true;
            return View(model);
        }

        #endregion

        #region Condition Manufacturer
        [HttpPost]
        public async Task<IActionResult> ConditionManufacturer(string customerActionId, string conditionId)
        {
            var customerActions = await _customerActionService.GetCustomerActionById(customerActionId);
            var condition = customerActions.Conditions.FirstOrDefault(x => x.Id == conditionId);

            var items = new List<(string Id, string ManufacturerName)>();
            foreach (var item in condition.Manufacturers)
            {
                var manufacturer = (await _manufacturerService.GetManufacturerById(item))?.Name;
                items.Add((item, manufacturer));
            }
            var gridModel = new DataSourceResult
            {
                Data = items.Select(x => new { Id = x.Id, ManufacturerName = x.ManufacturerName }),
                Total = customerActions.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        public IActionResult ManufacturerAddPopup(string customerActionId, string conditionId)
        {
            var model = new CustomerActionConditionModel.AddManufacturerConditionModel
            {
                CustomerActionConditionId = conditionId,
                CustomerActionId = customerActionId
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ManufacturerAddPopupList(DataSourceRequest command, CustomerActionConditionModel.AddManufacturerConditionModel model)
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
        public async Task<IActionResult> ManufacturerAddPopup(CustomerActionConditionModel.AddManufacturerConditionModel model)
        {
            if (model.SelectedManufacturerIds != null)
            {
                await _customerActionViewModelService.InsertManufacturerConditionModel(model);
            }
            ViewBag.RefreshPage = true;            
            return View(model);
        }

        #endregion

        #region Condition Vendor

        [HttpPost]
        public async Task<IActionResult> ConditionVendor(string customerActionId, string conditionId)
        {
            var customerActions = await _customerActionService.GetCustomerActionById(customerActionId);
            var condition = customerActions.Conditions.FirstOrDefault(x => x.Id == conditionId);
            var vendors = await _vendorService.GetAllVendors();
            var gridModel = new DataSourceResult
            {
                Data = condition != null ? condition.Vendors.Select(z => new { Id = z, VendorName = vendors.FirstOrDefault(y=>y.Id == z).Name }) : null,
                Total = customerActions.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        [HttpPost]
        public async Task<IActionResult> ConditionVendorInsert(CustomerActionConditionModel.AddVendorConditionModel model)
        {
            await _customerActionViewModelService.InsertVendorConditionModel(model);
            return new NullJsonResult();
        }

        [HttpGet]
        public async Task<IActionResult> Vendors()
        {
            var customerVendors = (await _vendorService.GetAllVendors()).Select(x => new { Id = x.Id, Name = x.Name });
            return Json(customerVendors);
        }
        #endregion

        #region Condition Customer role

        [HttpPost]
        public async Task<IActionResult> ConditionCustomerRole(string customerActionId, string conditionId)
        {
            var customerActions = await _customerActionService.GetCustomerActionById(customerActionId);
            var condition = customerActions.Conditions.FirstOrDefault(x => x.Id == conditionId);
            var items = new List<(string Id, string CustomerRole)>();
            foreach (var item in condition.CustomerRoles)
            {
                var roles = (await _customerService.GetCustomerRoleById(item))?.Name;
                items.Add((item, roles));
            }
            var gridModel = new DataSourceResult
            {
                Data = items.Select(x => new { Id = x.Id, CustomerRole = x.CustomerRole }),
                Total = customerActions.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        [HttpPost]
        public async Task<IActionResult> ConditionCustomerRoleInsert(CustomerActionConditionModel.AddCustomerRoleConditionModel model)
        {
            await _customerActionViewModelService.InsertCustomerRoleConditionModel(model);
            return new NullJsonResult();
        }

        [HttpGet]
        public async Task<IActionResult> CustomerRoles()
        {
            var customerRole = (await _customerService.GetAllCustomerRoles()).Select(x => new { Id = x.Id, Name = x.Name });
            return Json(customerRole);
        }

        #endregion

        #region Stores

        [HttpGet]
        public async Task<IActionResult> Stores()
        {
            var stores = (await _storeService.GetAllStores()).Select(x => new { Id = x.Id, Name = x.Shortcut });
            return Json(stores);
        }

        [HttpPost]
        public async Task<IActionResult> ConditionStore(string customerActionId, string conditionId)
        {
            var customerActions = await _customerActionService.GetCustomerActionById(customerActionId);
            var condition = customerActions.Conditions.FirstOrDefault(x => x.Id == conditionId);
            var items = new List<(string Id, string Store)>();
            foreach (var item in condition.Stores)
            {
                var store = (await _storeService.GetStoreById(item))?.Shortcut;
                items.Add((item, store));
            }
            var gridModel = new DataSourceResult
            {
                Data = items.Select(x => new { Id = x.Id, Store = x.Store }),
                Total = customerActions.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        [HttpPost]
        public async Task<IActionResult> ConditionStoreInsert(CustomerActionConditionModel.AddStoreConditionModel model)
        {
            await _customerActionViewModelService.InsertStoreConditionModel(model);
            return new NullJsonResult();
        }

        #endregion

        #region Customer Tags

        [HttpPost]
        public async Task<IActionResult> ConditionCustomerTag(string customerActionId, string conditionId)
        {
            var customerActions = await _customerActionService.GetCustomerActionById(customerActionId);
            var condition = customerActions.Conditions.FirstOrDefault(x => x.Id == conditionId);
            var items = new List<(string Id, string CustomerTag)>();
            foreach (var item in condition.CustomerTags)
            {
                var tag = (await _customerTagService.GetCustomerTagById(item))?.Name;
                items.Add((item, tag));
            }
            var gridModel = new DataSourceResult
            {
                Data = items.Select(x => new { Id = x.Id, CustomerTag = x.CustomerTag }),
                Total = customerActions.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        [HttpPost]
        public async Task<IActionResult> ConditionCustomerTagInsert(CustomerActionConditionModel.AddCustomerTagConditionModel model)
        {
            await _customerActionViewModelService.InsertCustomerTagConditionModel(model);
            return new NullJsonResult();
        }

        [HttpGet]
        public async Task<IActionResult> CustomerTags()
        {
            var customerTag = (await _customerTagService.GetAllCustomerTags()).Select(x => new { Id = x.Id, Name = x.Name });
            return Json(customerTag);
        }
        #endregion

        #region Condition Product Attributes

        [HttpPost]
        public async Task<IActionResult> ConditionProductAttribute(string customerActionId, string conditionId)
        {
            var customerActions = await _customerActionService.GetCustomerActionById(customerActionId);
            var condition = customerActions.Conditions.FirstOrDefault(x => x.Id == conditionId);
            var items = new List<(string Id, string ProductAttributeId, string ProductAttributeName)>();
            foreach (var item in condition.ProductAttribute)
            {
                var pa = (await _productAttributeService.GetProductAttributeById(item.ProductAttributeId))?.Name;
                items.Add((item.Id, item.ProductAttributeId, pa));
            }
            var gridModel = new DataSourceResult
            {
                Data = items.Select(x => new { Id = x.Id, ProductAttributeId = x.ProductAttributeId, ProductAttributeName = x.ProductAttributeName }),
                Total = customerActions.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        [HttpPost]
        public async Task<IActionResult> ConditionProductAttributeInsert(CustomerActionConditionModel.AddProductAttributeConditionModel model)
        {
            await _customerActionViewModelService.InsertProductAttributeConditionModel(model);
            return new NullJsonResult();
        }
        [HttpPost]
        public async Task<IActionResult> ConditionProductAttributeUpdate(CustomerActionConditionModel.AddProductAttributeConditionModel model)
        {
            await _customerActionViewModelService.UpdateProductAttributeConditionModel(model);
            return new NullJsonResult();
        }

        [HttpGet]
        public async Task<IActionResult> ProductAttributes()
        {
            var customerAttr = (await _productAttributeService.GetAllProductAttributes()).Select(x => new { Id = x.Id, Name = x.Name });
            return Json(customerAttr);
        }
        #endregion

        #region Condition Product Specification

        [HttpPost]
        public async Task<IActionResult> ConditionProductSpecification(string customerActionId, string conditionId)
        {
            var customerActions = await _customerActionService.GetCustomerActionById(customerActionId);
            var condition = customerActions.Conditions.FirstOrDefault(x => x.Id == conditionId);
            var specs = await _specificationAttributeService.GetSpecificationAttributes();
            var gridModel = new DataSourceResult
            {
                Data = condition != null ? condition.ProductSpecifications
                        .Select(z => new
                        {
                            Id = z.Id,
                            SpecificationId = z.ProductSpecyficationId,
                            SpecificationName = specs.FirstOrDefault(x=>x.Id == z.ProductSpecyficationId)?.Name,
                            SpecificationValueName = !String.IsNullOrEmpty(z.ProductSpecyficationValueId) ? specs.FirstOrDefault(x => x.Id == z.ProductSpecyficationId).SpecificationAttributeOptions.FirstOrDefault(x => x.Id == z.ProductSpecyficationValueId).Name : "Undefined"
                        }) : null,
                Total = customerActions.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        [HttpPost]
        public async Task<IActionResult> ConditionProductSpecificationInsert(CustomerActionConditionModel.AddProductSpecificationConditionModel model)
        {
            await _customerActionViewModelService.InsertProductSpecificationConditionModel(model);
            return new NullJsonResult();
        }

        [HttpGet]
        public async Task<IActionResult> ProductSpecification()
        {
            var customerAttr = (await _specificationAttributeService.GetSpecificationAttributes()).Select(x => new { Id = x.Id, Name = x.Name });
            return Json(customerAttr);
        }

        [HttpGet]
        public async Task<IActionResult> ProductSpecificationValue(string specificationId)
        {
            if (String.IsNullOrEmpty(specificationId))
                return new NullJsonResult();

            var customerSpec = (await _specificationAttributeService.GetSpecificationAttributeById(specificationId)).SpecificationAttributeOptions.Select(x => new { Id = x.Id, Name = x.Name });
            return Json(customerSpec);
        }

        #endregion

        #region Condition Customer Register

        [HttpPost]
        public async Task<IActionResult> ConditionCustomerRegister(string customerActionId, string conditionId)
        {
            var customerActions = await _customerActionService.GetCustomerActionById(customerActionId);
            var condition = customerActions.Conditions.FirstOrDefault(x => x.Id == conditionId);

            var gridModel = new DataSourceResult
            {
                Data = condition != null ? condition.CustomerRegistration.Select(z => new
                {
                    Id = z.Id,
                    CustomerRegisterName = z.RegisterField,
                    CustomerRegisterValue = z.RegisterValue
                })
                    : null,
                Total = customerActions.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        [HttpPost]
        public async Task<IActionResult> ConditionCustomerRegisterInsert(CustomerActionConditionModel.AddCustomerRegisterConditionModel model)
        {
            await _customerActionViewModelService.InsertCustomerRegisterConditionModel(model);
            return new NullJsonResult();
        }
        [HttpPost]
        public async Task<IActionResult> ConditionCustomerRegisterUpdate(CustomerActionConditionModel.AddCustomerRegisterConditionModel model)
        {
            await _customerActionViewModelService.UpdateCustomerRegisterConditionModel(model);
            return new NullJsonResult();
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
        public async Task<IActionResult> ConditionCustomCustomerAttribute(string customerActionId, string conditionId)
        {
            var customerActions = await _customerActionService.GetCustomerActionById(customerActionId);
            var condition = customerActions.Conditions.FirstOrDefault(x => x.Id == conditionId);

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
                Total = customerActions.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        [HttpPost]
        public async Task<IActionResult> ConditionCustomCustomerAttributeInsert(CustomerActionConditionModel.AddCustomCustomerAttributeConditionModel model)
        {
            await _customerActionViewModelService.InsertCustomCustomerAttributeConditionModel(model);
            return new NullJsonResult();
        }
        [HttpPost]
        public async Task<IActionResult> ConditionCustomCustomerAttributeUpdate(CustomerActionConditionModel.AddCustomCustomerAttributeConditionModel model)
        {
            await _customerActionViewModelService.UpdateCustomCustomerAttributeConditionModel(model);
            return new NullJsonResult();
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

        #region Url Referrer

        [HttpPost]
        public async Task<IActionResult> ConditionUrlReferrer(string customerActionId, string conditionId)
        {
            var customerActions = await _customerActionService.GetCustomerActionById(customerActionId);
            var condition = customerActions.Conditions.FirstOrDefault(x => x.Id == conditionId);

            var gridModel = new DataSourceResult
            {
                Data = condition != null ? condition.UrlReferrer.Select(z => new { Id = z.Id, Name = z.Name }) : null,
                Total = customerActions.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        [HttpPost]
        public async Task<IActionResult> ConditionUrlReferrerInsert(CustomerActionConditionModel.AddUrlConditionModel model)
        {
            await _customerActionViewModelService.InsertUrlConditionModel(model);
            return new NullJsonResult();
        }
        [HttpPost]
        public async Task<IActionResult> ConditionUrlReferrerUpdate(CustomerActionConditionModel.AddUrlConditionModel model)
        {
            await _customerActionViewModelService.UpdateUrlConditionModel(model);
            return new NullJsonResult();
        }

        #endregion

        #region Url Current

        [HttpPost]
        public async Task<IActionResult> ConditionUrlCurrent(string customerActionId, string conditionId)
        {
            var customerActions = await _customerActionService.GetCustomerActionById(customerActionId);
            var condition = customerActions.Conditions.FirstOrDefault(x => x.Id == conditionId);

            var gridModel = new DataSourceResult
            {
                Data = condition != null ? condition.UrlCurrent.Select(z => new { Id = z.Id, Name = z.Name }) : null,
                Total = customerActions.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        [HttpPost]
        public async Task<IActionResult> ConditionUrlCurrentInsert(CustomerActionConditionModel.AddUrlConditionModel model)
        {
            await _customerActionViewModelService.InsertUrlCurrentConditionModel(model);
            return new NullJsonResult();
        }
        [HttpPost]
        public async Task<IActionResult> ConditionUrlCurrentUpdate(CustomerActionConditionModel.AddUrlConditionModel model)
        {
            await _customerActionViewModelService.UpdateUrlCurrentConditionModel(model);
            return new NullJsonResult();
        }

        #endregion
    }
}
