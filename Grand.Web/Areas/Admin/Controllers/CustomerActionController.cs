using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
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
            ISpecificationAttributeService specificationAttributeService)
        {
            this._customerActionViewModelService = customerActionViewModelService;
            this._customerService = customerService;
            this._customerAttributeService = customerAttributeService;
            this._customerTagService = customerTagService;
            this._localizationService = localizationService;
            this._customerActivityService = customerActivityService;
            this._productService = productService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._storeService = storeService;
            this._vendorService = vendorService;
            this._customerActionService = customerActionService;
            this._productAttributeService = productAttributeService;
            this._specificationAttributeService = specificationAttributeService;
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
        public IActionResult List(DataSourceRequest command)
        {
            var customeractions = _customerActionService.GetCustomerActions();
            var gridModel = new DataSourceResult
            {
                Data = customeractions.Select(x => new { Id = x.Id, Name = x.Name, Active = x.Active, ActionType = _customerActionService.GetCustomerActionTypeById(x.ActionTypeId).Name }),
                Total = customeractions.Count()
            };
            return Json(gridModel);
        }

        public IActionResult Create()
        {
            var model = _customerActionViewModelService.PrepareCustomerActionModel();
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(CustomerActionModel model, bool continueEditing)
        {
            CheckValidateModel(model);
            if (ModelState.IsValid)
            {
                var customeraction = _customerActionViewModelService.InsertCustomerActionModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerAction.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = customeraction.Id }) : RedirectToAction("List");
            }
            _customerActionViewModelService.PrepareReactObjectModel(model);
            return View(model);
        }

        public IActionResult Edit(string id)
        {
            var customerAction = _customerActionService.GetCustomerActionById(id);
            if (customerAction == null)
                return RedirectToAction("List");

            var model = customerAction.ToModel();
            model.ConditionCount = customerAction.Conditions.Count();
            _customerActionViewModelService.PrepareReactObjectModel(model);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(CustomerActionModel model, bool continueEditing)
        {
            CheckValidateModel(model);

            var customeraction = _customerActionService.GetCustomerActionById(model.Id);
            if (customeraction == null)
                return RedirectToAction("List");
            try
            {
                if (ModelState.IsValid)
                {
                    customeraction = _customerActionViewModelService.UpdateCustomerActionModel(customeraction, model);
                    SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerAction.Updated"));
                    return continueEditing ? RedirectToAction("Edit", new { id = customeraction.Id }) : RedirectToAction("List");
                }
                _customerActionViewModelService.PrepareReactObjectModel(model);
                return View(model);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("Edit", new { id = customeraction.Id });
            }
        }

        [HttpPost]
        public IActionResult History(DataSourceRequest command, string customerActionId)
        {
            var history = _customerActionService.GetAllCustomerActionHistory(customerActionId,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = history.Select(_customerActionViewModelService.PrepareHistoryModelForList),
                Total = history.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            var customerAction = _customerActionService.GetCustomerActionById(id);
            if (customerAction == null)
                return RedirectToAction("List");

            try
            {
                //activity log
                _customerActivityService.InsertActivity("DeleteCustomerAction", customerAction.Id, _localizationService.GetResource("ActivityLog.DeleteCustomerAction"), customerAction.Name);
                _customerActionService.DeleteCustomerAction(customerAction);
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
        public IActionResult Conditions(string customerActionId)
        {
            var conditions = _customerActionService.GetCustomerActionById(customerActionId);
            var gridModel = new DataSourceResult
            {
                Data = conditions.Conditions.Select(x => new { Id = x.Id, Name = x.Name, Condition = x.CustomerActionConditionType.ToString() }),
                Total = conditions.Conditions.Count()
            };
            return Json(gridModel);
        }

        #endregion

        #region Conditions

        public IActionResult AddCondition(string customerActionId)
        {
            var model = _customerActionViewModelService.PrepareCustomerActionConditionModel(customerActionId);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult AddCondition(CustomerActionConditionModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var customerAction = _customerActionViewModelService.InsertCustomerActionConditionModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerActionCondition.Added"));
                return continueEditing ? RedirectToAction("EditCondition", new { customerActionId = customerAction.customerActionId, cid = customerAction.conditionId }) : RedirectToAction("Edit", new { id = customerAction.customerActionId });
            }
            return View(model);
        }

        public IActionResult EditCondition(string customerActionId, string cid)
        {
            var customerAction = _customerActionService.GetCustomerActionById(customerActionId);
            if (customerAction == null)
                return RedirectToAction("List");
            var actionType = _customerActionService.GetCustomerActionTypeById(customerAction.ActionTypeId);
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
        public IActionResult EditCondition(string customerActionId, string cid, CustomerActionConditionModel model, bool continueEditing)
        {
            var customerAction = _customerActionService.GetCustomerActionById(customerActionId);
            if (customerAction == null)
                return RedirectToAction("List");

            var condition = customerAction.Conditions.FirstOrDefault(x => x.Id == cid);
            if (condition == null)
                return RedirectToAction("List");
            try
            {
                if (ModelState.IsValid)
                {
                    _customerActionViewModelService.UpdateCustomerActionConditionModel(customerAction, condition, model);
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
        public IActionResult ConditionDelete(string Id, string customerActionId)
        {
            _customerActionViewModelService.ConditionDelete(Id, customerActionId);
            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult ConditionDeletePosition(string id, string customerActionId, string conditionId)
        {
            _customerActionViewModelService.ConditionDeletePosition(id, customerActionId, conditionId);

            return new NullJsonResult();
        }
        #endregion

        #region Condition Product

        [HttpPost]
        public IActionResult ConditionProduct(string customerActionId, string conditionId)
        {
            var customerActions = _customerActionService.GetCustomerActionById(customerActionId);
            var condition = customerActions.Conditions.FirstOrDefault(x => x.Id == conditionId);

            var gridModel = new DataSourceResult
            {
                Data = condition != null ? condition.Products.Select(z => new { Id = z, ProductName = _productService.GetProductById(z) != null ? _productService.GetProductById(z).Name : "" }) : null,
                Total = customerActions.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }


        public IActionResult ProductAddPopup(string customerActionId, string conditionId)
        {
            var model = _customerActionViewModelService.PrepareAddProductToConditionModel(customerActionId, conditionId);
            return View(model);
        }

        [HttpPost]
        public IActionResult ProductAddPopupList(DataSourceRequest command, CustomerActionConditionModel.AddProductToConditionModel model)
        {
            var products = _customerActionViewModelService.PrepareProductModel(model, command.Page, command.PageSize);
            var gridModel = new DataSourceResult();
            gridModel.Data = products.products.ToList();
            gridModel.Total = products.totalCount;

            return Json(gridModel);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public IActionResult ProductAddPopup(CustomerActionConditionModel.AddProductToConditionModel model)
        {
            if (model.SelectedProductIds != null)
            {
                _customerActionViewModelService.InsertProductToConditionModel(model);
            }
            ViewBag.RefreshPage = true;
            return View(model);
        }
        #endregion

        #region Condition Category

        [HttpPost]
        public IActionResult ConditionCategory(string customerActionId, string conditionId)
        {
            var customerActions = _customerActionService.GetCustomerActionById(customerActionId);
            var condition = customerActions.Conditions.FirstOrDefault(x => x.Id == conditionId);

            var gridModel = new DataSourceResult
            {
                Data = condition != null ? condition.Categories.Select(z => new { Id = z, CategoryName = _categoryService.GetCategoryById(z) != null ? _categoryService.GetCategoryById(z).Name : "" }) : null,
                Total = customerActions.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        public IActionResult CategoryAddPopup(string customerActionId, string conditionId)
        {
            var model = new CustomerActionConditionModel.AddCategoryConditionModel();
            model.CustomerActionConditionId = conditionId;
            model.CustomerActionId = customerActionId;
            return View(model);
        }

        [HttpPost]
        public IActionResult CategoryAddPopupList(DataSourceRequest command, CustomerActionConditionModel.AddCategoryConditionModel model)
        {
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
        public IActionResult CategoryAddPopup(CustomerActionConditionModel.AddCategoryConditionModel model)
        {
            if (model.SelectedCategoryIds != null)
            {
                _customerActionViewModelService.InsertCategoryConditionModel(model);
            }
            ViewBag.RefreshPage = true;
            return View(model);
        }

        #endregion

        #region Condition Manufacturer
        [HttpPost]
        public IActionResult ConditionManufacturer(string customerActionId, string conditionId)
        {
            var customerActions = _customerActionService.GetCustomerActionById(customerActionId);
            var condition = customerActions.Conditions.FirstOrDefault(x => x.Id == conditionId);

            var gridModel = new DataSourceResult
            {
                Data = condition != null ? condition.Manufacturers.Select(z => new { Id = z, ManufacturerName = _manufacturerService.GetManufacturerById(z) != null ? _manufacturerService.GetManufacturerById(z).Name : "" }) : null,
                Total = customerActions.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        public IActionResult ManufacturerAddPopup(string customerActionId, string conditionId)
        {
            var model = new CustomerActionConditionModel.AddManufacturerConditionModel();
            model.CustomerActionConditionId = conditionId;
            model.CustomerActionId = customerActionId;
            return View(model);

        }

        [HttpPost]
        public IActionResult ManufacturerAddPopupList(DataSourceRequest command, CustomerActionConditionModel.AddManufacturerConditionModel model)
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
        public IActionResult ManufacturerAddPopup(CustomerActionConditionModel.AddManufacturerConditionModel model)
        {
            if (model.SelectedManufacturerIds != null)
            {
                _customerActionViewModelService.InsertManufacturerConditionModel(model);
            }
            ViewBag.RefreshPage = true;            
            return View(model);
        }

        #endregion

        #region Condition Vendor

        [HttpPost]
        public IActionResult ConditionVendor(string customerActionId, string conditionId)
        {
            var customerActions = _customerActionService.GetCustomerActionById(customerActionId);
            var condition = customerActions.Conditions.FirstOrDefault(x => x.Id == conditionId);

            var gridModel = new DataSourceResult
            {
                Data = condition != null ? condition.Vendors.Select(z => new { Id = z, VendorName = _vendorService.GetVendorById(z).Name }) : null,
                Total = customerActions.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ConditionVendorInsert(CustomerActionConditionModel.AddVendorConditionModel model)
        {
            _customerActionViewModelService.InsertVendorConditionModel(model);
            return new NullJsonResult();
        }

        [HttpGet]
        public IActionResult Vendors()
        {
            var customerVendors = _vendorService.GetAllVendors().Select(x => new { Id = x.Id, Name = x.Name });
            return Json(customerVendors);
        }
        #endregion

        #region Condition Customer role

        [HttpPost]
        public IActionResult ConditionCustomerRole(string customerActionId, string conditionId)
        {
            var customerActions = _customerActionService.GetCustomerActionById(customerActionId);
            var condition = customerActions.Conditions.FirstOrDefault(x => x.Id == conditionId);

            var gridModel = new DataSourceResult
            {
                Data = condition != null ? condition.CustomerRoles.Select(z => new { Id = z, CustomerRole = _customerService.GetCustomerRoleById(z) != null ? _customerService.GetCustomerRoleById(z).Name : "" }) : null,
                Total = customerActions.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ConditionCustomerRoleInsert(CustomerActionConditionModel.AddCustomerRoleConditionModel model)
        {
            _customerActionViewModelService.InsertCustomerRoleConditionModel(model);
            return new NullJsonResult();
        }


        [HttpGet]
        public IActionResult CustomerRoles()
        {
            var customerRole = _customerService.GetAllCustomerRoles().Select(x => new { Id = x.Id, Name = x.Name });
            return Json(customerRole);
        }

        #endregion

        #region Stores

        [HttpGet]
        public IActionResult Stores()
        {
            var stores = _storeService.GetAllStores().Select(x => new { Id = x.Id, Name = x.Name });
            return Json(stores);
        }

        [HttpPost]
        public IActionResult ConditionStore(string customerActionId, string conditionId)
        {
            var customerActions = _customerActionService.GetCustomerActionById(customerActionId);
            var condition = customerActions.Conditions.FirstOrDefault(x => x.Id == conditionId);

            var gridModel = new DataSourceResult
            {
                Data = condition != null ? condition.Stores.Select(z => new { Id = z, Store = _storeService.GetStoreById(z) != null ? _storeService.GetStoreById(z).Name : "" }) : null,
                Total = customerActions.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ConditionStoreInsert(CustomerActionConditionModel.AddStoreConditionModel model)
        {
            _customerActionViewModelService.InsertStoreConditionModel(model);
            return new NullJsonResult();
        }

        #endregion

        #region Customer Tags

        [HttpPost]
        public IActionResult ConditionCustomerTag(string customerActionId, string conditionId)
        {
            var customerActions = _customerActionService.GetCustomerActionById(customerActionId);
            var condition = customerActions.Conditions.FirstOrDefault(x => x.Id == conditionId);

            var gridModel = new DataSourceResult
            {
                Data = condition != null ? condition.CustomerTags.Select(z => new { Id = z, CustomerTag = _customerTagService.GetCustomerTagById(z) != null ? _customerTagService.GetCustomerTagById(z).Name : "" }) : null,
                Total = customerActions.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ConditionCustomerTagInsert(CustomerActionConditionModel.AddCustomerTagConditionModel model)
        {
            _customerActionViewModelService.InsertCustomerTagConditionModel(model);
            return new NullJsonResult();
        }

        [HttpGet]
        public IActionResult CustomerTags()
        {
            var customerTag = _customerTagService.GetAllCustomerTags().Select(x => new { Id = x.Id, Name = x.Name });
            return Json(customerTag);
        }
        #endregion

        #region Condition Product Attributes

        [HttpPost]
        public IActionResult ConditionProductAttribute(string customerActionId, string conditionId)
        {
            var customerActions = _customerActionService.GetCustomerActionById(customerActionId);
            var condition = customerActions.Conditions.FirstOrDefault(x => x.Id == conditionId);

            var gridModel = new DataSourceResult
            {
                Data = condition != null ? condition.ProductAttribute.Select(z => new { Id = z.Id, ProductAttributeId = z.ProductAttributeId, ProductAttributeName = _productAttributeService.GetProductAttributeById(z.ProductAttributeId).Name, Name = z.Name }) : null,
                Total = customerActions.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ConditionProductAttributeInsert(CustomerActionConditionModel.AddProductAttributeConditionModel model)
        {
            _customerActionViewModelService.InsertProductAttributeConditionModel(model);
            return new NullJsonResult();
        }
        [HttpPost]
        public IActionResult ConditionProductAttributeUpdate(CustomerActionConditionModel.AddProductAttributeConditionModel model)
        {
            _customerActionViewModelService.UpdateProductAttributeConditionModel(model);
            return new NullJsonResult();
        }

        [HttpGet]
        public IActionResult ProductAttributes()
        {
            var customerAttr = _productAttributeService.GetAllProductAttributes().Select(x => new { Id = x.Id, Name = x.Name });
            return Json(customerAttr);
        }
        #endregion

        #region Condition Product Specification

        [HttpPost]
        public IActionResult ConditionProductSpecification(string customerActionId, string conditionId)
        {
            var customerActions = _customerActionService.GetCustomerActionById(customerActionId);
            var condition = customerActions.Conditions.FirstOrDefault(x => x.Id == conditionId);

            var gridModel = new DataSourceResult
            {
                Data = condition != null ? condition.ProductSpecifications
                        .Select(z => new
                        {
                            Id = z.Id,
                            SpecificationId = z.ProductSpecyficationId,
                            SpecificationName = _specificationAttributeService.GetSpecificationAttributeById(z.ProductSpecyficationId).Name,
                            SpecificationValueName = !String.IsNullOrEmpty(z.ProductSpecyficationValueId) ? _specificationAttributeService.GetSpecificationAttributeById(z.ProductSpecyficationId).SpecificationAttributeOptions.FirstOrDefault(x => x.Id == z.ProductSpecyficationValueId).Name : "Undefined"
                        }) : null,
                Total = customerActions.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ConditionProductSpecificationInsert(CustomerActionConditionModel.AddProductSpecificationConditionModel model)
        {
            _customerActionViewModelService.InsertProductSpecificationConditionModel(model);
            return new NullJsonResult();
        }

        [HttpGet]
        public IActionResult ProductSpecification()
        {
            var customerAttr = _specificationAttributeService.GetSpecificationAttributes().Select(x => new { Id = x.Id, Name = x.Name });
            return Json(customerAttr);
        }

        [HttpGet]
        public IActionResult ProductSpecificationValue(string specificationId)
        {
            if (String.IsNullOrEmpty(specificationId))
                return new NullJsonResult();

            var customerSpec = _specificationAttributeService.GetSpecificationAttributeById(specificationId).SpecificationAttributeOptions.Select(x => new { Id = x.Id, Name = x.Name });

            return Json(customerSpec);
        }

        #endregion

        #region Condition Customer Register

        [HttpPost]
        public IActionResult ConditionCustomerRegister(string customerActionId, string conditionId)
        {
            var customerActions = _customerActionService.GetCustomerActionById(customerActionId);
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
        public IActionResult ConditionCustomerRegisterInsert(CustomerActionConditionModel.AddCustomerRegisterConditionModel model)
        {
            _customerActionViewModelService.InsertCustomerRegisterConditionModel(model);
            return new NullJsonResult();
        }
        [HttpPost]
        public IActionResult ConditionCustomerRegisterUpdate(CustomerActionConditionModel.AddCustomerRegisterConditionModel model)
        {
            _customerActionViewModelService.UpdateCustomerRegisterConditionModel(model);
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
        public IActionResult ConditionCustomCustomerAttribute(string customerActionId, string conditionId)
        {
            var customerActions = _customerActionService.GetCustomerActionById(customerActionId);
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
        public IActionResult ConditionCustomCustomerAttributeInsert(CustomerActionConditionModel.AddCustomCustomerAttributeConditionModel model)
        {
            _customerActionViewModelService.InsertCustomCustomerAttributeConditionModel(model);
            return new NullJsonResult();
        }
        [HttpPost]
        public IActionResult ConditionCustomCustomerAttributeUpdate(CustomerActionConditionModel.AddCustomCustomerAttributeConditionModel model)
        {
            _customerActionViewModelService.UpdateCustomCustomerAttributeConditionModel(model);
            return new NullJsonResult();
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

        #region Url Referrer

        [HttpPost]
        public IActionResult ConditionUrlReferrer(string customerActionId, string conditionId)
        {
            var customerActions = _customerActionService.GetCustomerActionById(customerActionId);
            var condition = customerActions.Conditions.FirstOrDefault(x => x.Id == conditionId);

            var gridModel = new DataSourceResult
            {
                Data = condition != null ? condition.UrlReferrer.Select(z => new { Id = z.Id, Name = z.Name }) : null,
                Total = customerActions.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ConditionUrlReferrerInsert(CustomerActionConditionModel.AddUrlConditionModel model)
        {
            _customerActionViewModelService.InsertUrlConditionModel(model);
            return new NullJsonResult();
        }
        [HttpPost]
        public IActionResult ConditionUrlReferrerUpdate(CustomerActionConditionModel.AddUrlConditionModel model)
        {
            _customerActionViewModelService.UpdateUrlConditionModel(model);
            return new NullJsonResult();
        }

        #endregion

        #region Url Current

        [HttpPost]
        public IActionResult ConditionUrlCurrent(string customerActionId, string conditionId)
        {
            var customerActions = _customerActionService.GetCustomerActionById(customerActionId);
            var condition = customerActions.Conditions.FirstOrDefault(x => x.Id == conditionId);

            var gridModel = new DataSourceResult
            {
                Data = condition != null ? condition.UrlCurrent.Select(z => new { Id = z.Id, Name = z.Name }) : null,
                Total = customerActions.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ConditionUrlCurrentInsert(CustomerActionConditionModel.AddUrlConditionModel model)
        {
            _customerActionViewModelService.InsertUrlCurrentConditionModel(model);
            return new NullJsonResult();
        }
        [HttpPost]
        public IActionResult ConditionUrlCurrentUpdate(CustomerActionConditionModel.AddUrlConditionModel model)
        {
            _customerActionViewModelService.UpdateUrlCurrentConditionModel(model);
            return new NullJsonResult();
        }

        #endregion
    }
}
