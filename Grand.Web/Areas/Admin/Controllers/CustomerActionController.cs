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
    public partial class CustomerActionController : BaseAdminController
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
        private readonly ICustomerActionService _customerActionService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IBannerService _bannerService;
        private readonly IInteractiveFormService _interactiveFormService;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IDateTimeHelper _dateTimeHelper;
        #endregion

        #region Constructors

        public CustomerActionController(ICustomerService customerService,
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
            ICustomerActionService customerActionService,
            IProductAttributeService productAttributeService,
            ISpecificationAttributeService specificationAttributeService,
            IBannerService bannerService,
            IInteractiveFormService interactiveFormService,
            IMessageTemplateService messageTemplateService,
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
            this._customerActionService = customerActionService;
            this._productAttributeService = productAttributeService;
            this._specificationAttributeService = specificationAttributeService;
            this._bannerService = bannerService;
            this._interactiveFormService = interactiveFormService;
            this._messageTemplateService = messageTemplateService;
            this._dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected virtual void PrepareReactObjectModel(CustomerActionModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            var banners = _bannerService.GetAllBanners();
            foreach (var item in banners)
            {
                model.Banners.Add(new SelectListItem
                {
                    Text = item.Name,
                    Value = item.Id.ToString()
                });

            }
            var message = _messageTemplateService.GetAllMessageTemplates("");
            foreach (var item in message)
            {
                model.MessageTemplates.Add(new SelectListItem
                {
                    Text = item.Name,
                    Value = item.Id.ToString()
                });
            }
            var customerRole = _customerService.GetAllCustomerRoles();
            foreach (var item in customerRole)
            {
                model.CustomerRoles.Add(new SelectListItem
                {
                    Text = item.Name,
                    Value = item.Id.ToString()
                });
            }

            var customerTag = _customerTagService.GetAllCustomerTags();
            foreach (var item in customerTag)
            {
                model.CustomerTags.Add(new SelectListItem
                {
                    Text = item.Name,
                    Value = item.Id.ToString()
                });
            }

            foreach (var item in _customerActionService.GetCustomerActionType())
            {
                model.ActionType.Add(new SelectListItem()
                {
                    Text = item.Name,
                    Value = item.Id.ToString()
                });
            }

            foreach (var item in _interactiveFormService.GetAllForms())
            {
                model.InteractiveForms.Add(new SelectListItem()
                {
                    Text = item.Name,
                    Value = item.Id.ToString()
                });

            }


        }

        public class SerializeCustomerActionHistory
        {
            public string Email { get; set; }
            public DateTime CreateDateUtc { get; set; }

        }
        protected virtual SerializeCustomerActionHistory PrepareHistoryModelForList(CustomerActionHistory history)
        {
            var customer = _customerService.GetCustomerById(history.CustomerId);
            return new SerializeCustomerActionHistory
            {
                Email = customer != null ? String.IsNullOrEmpty(customer.Email) ? "(unknown)" : customer.Email : "(unknown)",
                CreateDateUtc = _dateTimeHelper.ConvertToUserTime(history.CreateDateUtc, DateTimeKind.Utc),
            };
        }

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

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public IActionResult List(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

            var model = new CustomerActionModel();
            model.Active = true;
            model.StartDateTimeUtc = DateTime.UtcNow;
            model.EndDateTimeUtc = DateTime.UtcNow.AddMonths(1);
            model.ReactionTypeId = (int)CustomerReactionTypeEnum.Banner;
            PrepareReactObjectModel(model);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(CustomerActionModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();
            CheckValidateModel(model);
            if (ModelState.IsValid)
            {
                var customeraction = model.ToEntity();
                _customerActionService.InsertCustomerAction(customeraction);
                _customerActivityService.InsertActivity("AddNewCustomerAction", customeraction.Id, _localizationService.GetResource("ActivityLog.AddNewCustomerAction"), customeraction.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerAction.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = customeraction.Id }) : RedirectToAction("List");
            }
            PrepareReactObjectModel(model);
            return View(model);
        }

        public IActionResult Edit(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

            var customerAction = _customerActionService.GetCustomerActionById(id);
            if (customerAction == null)
                return RedirectToAction("List");

            var model = customerAction.ToModel();
            model.ConditionCount = customerAction.Conditions.Count();
            PrepareReactObjectModel(model);


            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(CustomerActionModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

            CheckValidateModel(model);

            var customeraction = _customerActionService.GetCustomerActionById(model.Id);
            if (customeraction == null)
                return RedirectToAction("List");
            try
            {
                if (ModelState.IsValid)
                {
                    if (customeraction.Conditions.Count() > 0)
                        model.ActionTypeId = customeraction.ActionTypeId;
                    if (String.IsNullOrEmpty(model.ActionTypeId))
                        model.ActionTypeId = customeraction.ActionTypeId;

                    customeraction = model.ToEntity(customeraction);


                    _customerActionService.UpdateCustomerAction(customeraction);

                    _customerActivityService.InsertActivity("EditCustomerAction", customeraction.Id, _localizationService.GetResource("ActivityLog.EditCustomerAction"), customeraction.Name);

                    SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerAction.Updated"));
                    return continueEditing ? RedirectToAction("Edit", new { id = customeraction.Id }) : RedirectToAction("List");
                }
                PrepareReactObjectModel(model);
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
            //we use own own binder for searchCustomerRoleIds property 
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

            var history = _customerActionService.GetAllCustomerActionHistory(customerActionId,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = history.Select(PrepareHistoryModelForList),
                Total = history.TotalCount
            };

            return Json(gridModel);
        }


        [HttpPost]
        public IActionResult Delete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

            var customerAction = _customerActionService.GetCustomerActionById(customerActionId);
            var actionType = _customerActionService.GetCustomerActionTypeById(customerAction.ActionTypeId);

            var model = new CustomerActionConditionModel();
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
        public IActionResult AddCondition(CustomerActionConditionModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var customerAction = _customerActionService.GetCustomerActionById(model.CustomerActionId);
                if (customerAction == null)
                {
                    return RedirectToAction("List");
                }

                var condition = new CustomerAction.ActionCondition()
                {
                    Name = model.Name,
                    CustomerActionConditionTypeId = model.CustomerActionConditionTypeId,
                    ConditionId = model.ConditionId,
                };
                customerAction.Conditions.Add(condition);
                _customerActionService.UpdateCustomerAction(customerAction);

                _customerActivityService.InsertActivity("AddNewCustomerActionCondition", customerAction.Id, _localizationService.GetResource("ActivityLog.AddNewCustomerAction"), customerAction.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerActionCondition.Added"));

                return continueEditing ? RedirectToAction("EditCondition", new { customerActionId = customerAction.Id, cid = condition.Id }) : RedirectToAction("Edit", new { id = customerAction.Id });
            }

            return View(model);
        }

        public IActionResult EditCondition(string customerActionId, string cid)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

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
                    condition = model.ToEntity(condition);
                    _customerActionService.UpdateCustomerAction(customerAction);
                    //activity log
                    _customerActivityService.InsertActivity("EditCustomerActionCondition", customerAction.Id, _localizationService.GetResource("ActivityLog.EditCustomerActionCondition"), customerAction.Name);
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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

            var customerAction = _customerActionService.GetCustomerActionById(customerActionId);
            var condition = customerAction.Conditions.FirstOrDefault(x => x.Id == Id);
            customerAction.Conditions.Remove(condition);
            _customerActionService.UpdateCustomerAction(customerAction);

            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult ConditionDeletePosition(string id, string customerActionId, string conditionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

            var customerActions = _customerActionService.GetCustomerActionById(customerActionId);
            var condition = customerActions.Conditions.FirstOrDefault(x => x.Id == conditionId);

            if (condition.CustomerActionConditionTypeId == (int)CustomerActionConditionTypeEnum.Product)
            {
                condition.Products.Remove(id);
                _customerActionService.UpdateCustomerAction(customerActions);
            }
            if (condition.CustomerActionConditionTypeId == (int)CustomerActionConditionTypeEnum.Category)
            {
                condition.Categories.Remove(id);
                _customerActionService.UpdateCustomerAction(customerActions);
            }
            if (condition.CustomerActionConditionTypeId == (int)CustomerActionConditionTypeEnum.Manufacturer)
            {
                condition.Manufacturers.Remove(id);
                _customerActionService.UpdateCustomerAction(customerActions);
            }
            if (condition.CustomerActionConditionTypeId == (int)CustomerActionConditionTypeEnum.Vendor)
            {
                condition.Vendors.Remove(id);
                _customerActionService.UpdateCustomerAction(customerActions);
            }
            if (condition.CustomerActionConditionTypeId == (int)CustomerActionConditionTypeEnum.ProductAttribute)
            {
                condition.ProductAttribute.Remove(condition.ProductAttribute.FirstOrDefault(x => x.Id == id));
                _customerActionService.UpdateCustomerAction(customerActions);
            }
            if (condition.CustomerActionConditionTypeId == (int)CustomerActionConditionTypeEnum.ProductSpecification)
            {
                condition.ProductSpecifications.Remove(condition.ProductSpecifications.FirstOrDefault(x => x.Id == id));
                _customerActionService.UpdateCustomerAction(customerActions);
            }
            if (condition.CustomerActionConditionTypeId == (int)CustomerActionConditionTypeEnum.CustomerRole)
            {
                condition.CustomerRoles.Remove(id);
                _customerActionService.UpdateCustomerAction(customerActions);
            }
            if (condition.CustomerActionConditionTypeId == (int)CustomerActionConditionTypeEnum.CustomerTag)
            {
                condition.CustomerTags.Remove(id);
                _customerActionService.UpdateCustomerAction(customerActions);
            }

            if (condition.CustomerActionConditionTypeId == (int)CustomerActionConditionTypeEnum.CustomCustomerAttribute)
            {
                condition.CustomCustomerAttributes.Remove(condition.CustomCustomerAttributes.FirstOrDefault(x => x.Id == id));
                _customerActionService.UpdateCustomerAction(customerActions);
            }
            if (condition.CustomerActionConditionTypeId == (int)CustomerActionConditionTypeEnum.CustomerRegisterField)
            {
                condition.CustomerRegistration.Remove(condition.CustomerRegistration.FirstOrDefault(x => x.Id == id));
                _customerActionService.UpdateCustomerAction(customerActions);
            }
            if (condition.CustomerActionConditionTypeId == (int)CustomerActionConditionTypeEnum.UrlCurrent)
            {
                condition.UrlCurrent.Remove(condition.UrlCurrent.FirstOrDefault(x => x.Id == id));
                _customerActionService.UpdateCustomerAction(customerActions);
            }
            if (condition.CustomerActionConditionTypeId == (int)CustomerActionConditionTypeEnum.UrlReferrer)
            {
                condition.UrlReferrer.Remove(condition.UrlReferrer.FirstOrDefault(x => x.Id == id));
                _customerActionService.UpdateCustomerAction(customerActions);
            }
            if (condition.CustomerActionConditionTypeId == (int)CustomerActionConditionTypeEnum.Store)
            {
                condition.Stores.Remove(id);
                _customerActionService.UpdateCustomerAction(customerActions);
            }


            return new NullJsonResult();
        }
        #endregion

        #region Condition Product

        [HttpPost]
        public IActionResult ConditionProduct(string customerActionId, string conditionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

            var model = new CustomerActionConditionModel.AddProductToConditionModel();
            model.CustomerActionConditionId = conditionId;
            model.CustomerActionId = customerActionId;
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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
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
        public IActionResult ProductAddPopup(CustomerActionConditionModel.AddProductToConditionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

            if (model.SelectedProductIds != null)
            {
                foreach (string id in model.SelectedProductIds)
                {
                    var customerAction = _customerActionService.GetCustomerActionById(model.CustomerActionId);
                    if (customerAction != null)
                    {
                        var condition = customerAction.Conditions.FirstOrDefault(x => x.Id == model.CustomerActionConditionId);
                        if (condition != null)
                        {
                            if (condition.Products.Where(x => x == id).Count() == 0)
                            {
                                condition.Products.Add(id);
                                _customerActionService.UpdateCustomerAction(customerAction);
                            }
                        }
                    }
                }
            }
            ViewBag.RefreshPage = true;
            return View(model);
        }
        #endregion

        #region Condition Category

        [HttpPost]
        public IActionResult ConditionCategory(string customerActionId, string conditionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

            var model = new CustomerActionConditionModel.AddCategoryConditionModel();
            model.CustomerActionConditionId = conditionId;
            model.CustomerActionId = customerActionId;
            return View(model);
        }

        [HttpPost]
        public IActionResult CategoryAddPopupList(DataSourceRequest command, CustomerActionConditionModel.AddCategoryConditionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
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
        public IActionResult CategoryAddPopup(CustomerActionConditionModel.AddCategoryConditionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

            if (model.SelectedCategoryIds != null)
            {
                foreach (string id in model.SelectedCategoryIds)
                {
                    var customerAction = _customerActionService.GetCustomerActionById(model.CustomerActionId);
                    if (customerAction != null)
                    {
                        var condition = customerAction.Conditions.FirstOrDefault(x => x.Id == model.CustomerActionConditionId);
                        if (condition != null)
                        {
                            if (condition.Categories.Where(x => x == id).Count() == 0)
                            {
                                condition.Categories.Add(id);
                                _customerActionService.UpdateCustomerAction(customerAction);
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
        public IActionResult ConditionManufacturer(string customerActionId, string conditionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
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
        public IActionResult ManufacturerAddPopup(CustomerActionConditionModel.AddManufacturerConditionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

            if (model.SelectedManufacturerIds != null)
            {
                foreach (string id in model.SelectedManufacturerIds)
                {
                    var customerAction = _customerActionService.GetCustomerActionById(model.CustomerActionId);
                    if (customerAction != null)
                    {
                        var condition = customerAction.Conditions.FirstOrDefault(x => x.Id == model.CustomerActionConditionId);
                        if (condition != null)
                        {
                            if (condition.Manufacturers.Where(x => x == id).Count() == 0)
                            {
                                condition.Manufacturers.Add(id);
                                _customerActionService.UpdateCustomerAction(customerAction);
                            }
                        }
                    }

                }
            }
            ViewBag.RefreshPage = true;            
            return View(model);
        }


        #endregion

        #region Condition Vendor

        [HttpPost]
        public IActionResult ConditionVendor(string customerActionId, string conditionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

            var customerAction = _customerActionService.GetCustomerActionById(model.CustomerActionId);
            if (customerAction != null)
            {
                var condition = customerAction.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                if (condition != null)
                {
                    if (condition.Vendors.Where(x => x == model.VendorId).Count() == 0)
                    {
                        condition.Vendors.Add(model.VendorId);
                        _customerActionService.UpdateCustomerAction(customerAction);
                    }
                }
            }
            return new NullJsonResult();
        }


        [HttpGet]
        public IActionResult Vendors()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();
            var customerVendors = _vendorService.GetAllVendors().Select(x => new { Id = x.Id, Name = x.Name });
            return Json(customerVendors);
        }
        #endregion

        #region Condition Customer role

        [HttpPost]
        public IActionResult ConditionCustomerRole(string customerActionId, string conditionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

            var customerAction = _customerActionService.GetCustomerActionById(model.CustomerActionId);
            if (customerAction != null)
            {
                var condition = customerAction.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                if (condition != null)
                {
                    if (condition.CustomerRoles.Where(x => x == model.CustomerRoleId).Count() == 0)
                    {
                        condition.CustomerRoles.Add(model.CustomerRoleId);
                        _customerActionService.UpdateCustomerAction(customerAction);
                    }
                }
            }
            return new NullJsonResult();
        }


        [HttpGet]
        public IActionResult CustomerRoles()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();
            var customerRole = _customerService.GetAllCustomerRoles().Select(x => new { Id = x.Id, Name = x.Name });
            return Json(customerRole);
        }

        #endregion

        #region Stores

        [HttpGet]
        public IActionResult Stores()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();
            var stores = _storeService.GetAllStores().Select(x => new { Id = x.Id, Name = x.Name });
            return Json(stores);
        }

        [HttpPost]
        public IActionResult ConditionStore(string customerActionId, string conditionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

            var customerAction = _customerActionService.GetCustomerActionById(model.CustomerActionId);
            if (customerAction != null)
            {
                var condition = customerAction.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                if (condition != null)
                {
                    if (condition.Stores.Where(x => x == model.StoreId).Count() == 0)
                    {
                        condition.Stores.Add(model.StoreId);
                        _customerActionService.UpdateCustomerAction(customerAction);
                    }
                }
            }
            return new NullJsonResult();
        }


        #endregion

        #region Customer Tags

        [HttpPost]
        public IActionResult ConditionCustomerTag(string customerActionId, string conditionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

            var customerAction = _customerActionService.GetCustomerActionById(model.CustomerActionId);
            if (customerAction != null)
            {
                var condition = customerAction.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                if (condition != null)
                {
                    if (condition.CustomerTags.Where(x => x == model.CustomerTagId).Count() == 0)
                    {
                        condition.CustomerTags.Add(model.CustomerTagId);
                        _customerActionService.UpdateCustomerAction(customerAction);
                    }
                }
            }
            return new NullJsonResult();
        }


        [HttpGet]
        public IActionResult CustomerTags()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();
            var customerTag = _customerTagService.GetAllCustomerTags().Select(x => new { Id = x.Id, Name = x.Name });
            return Json(customerTag);
        }
        #endregion


        #region Condition Product Attributes

        [HttpPost]
        public IActionResult ConditionProductAttribute(string customerActionId, string conditionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

            var customerAction = _customerActionService.GetCustomerActionById(model.CustomerActionId);
            if (customerAction != null)
            {
                var condition = customerAction.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                if (condition != null)
                {
                    var _pv = new CustomerAction.ActionCondition.ProductAttributeValue()
                    {
                        ProductAttributeId = model.ProductAttributeId,
                        Name = model.Name
                    };
                    condition.ProductAttribute.Add(_pv);
                    _customerActionService.UpdateCustomerAction(customerAction);
                }
            }
            return new NullJsonResult();
        }
        [HttpPost]
        public IActionResult ConditionProductAttributeUpdate(CustomerActionConditionModel.AddProductAttributeConditionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

            var customerAction = _customerActionService.GetCustomerActionById(model.CustomerActionId);
            if (customerAction != null)
            {
                var condition = customerAction.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                if (condition != null)
                {
                    var pva = condition.ProductAttribute.FirstOrDefault(x => x.Id == model.Id);
                    pva.ProductAttributeId = model.ProductAttributeId;
                    pva.Name = model.Name;
                    _customerActionService.UpdateCustomerAction(customerAction);
                }
            }
            return new NullJsonResult();
        }



        [HttpGet]
        public IActionResult ProductAttributes()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();
            var customerAttr = _productAttributeService.GetAllProductAttributes().Select(x => new { Id = x.Id, Name = x.Name });
            return Json(customerAttr);
        }
        #endregion

        #region Condition Product Specification

        [HttpPost]
        public IActionResult ConditionProductSpecification(string customerActionId, string conditionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

            var customerAction = _customerActionService.GetCustomerActionById(model.CustomerActionId);
            if (customerAction != null)
            {
                var condition = customerAction.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                if (condition != null)
                {
                    if (condition.ProductSpecifications.Where(x => x.ProductSpecyficationId == model.SpecificationId && x.ProductSpecyficationValueId == model.SpecificationValueId).Count() == 0)
                    {
                        var _ps = new CustomerAction.ActionCondition.ProductSpecification()
                        {
                            ProductSpecyficationId = model.SpecificationId,
                            ProductSpecyficationValueId = model.SpecificationValueId
                        };
                        condition.ProductSpecifications.Add(_ps);
                        _customerActionService.UpdateCustomerAction(customerAction);
                    }
                }
            }
            return new NullJsonResult();
        }

        [HttpGet]
        public IActionResult ProductSpecification()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();
            var customerAttr = _specificationAttributeService.GetSpecificationAttributes().Select(x => new { Id = x.Id, Name = x.Name });
            return Json(customerAttr);
        }

        [HttpGet]
        public IActionResult ProductSpecificationValue(string specificationId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

            var customerAction = _customerActionService.GetCustomerActionById(model.CustomerActionId);
            if (customerAction != null)
            {
                var condition = customerAction.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                if (condition != null)
                {
                    var _cr = new CustomerAction.ActionCondition.CustomerRegister()
                    {
                        RegisterField = model.CustomerRegisterName,
                        RegisterValue = model.CustomerRegisterValue,
                    };
                    condition.CustomerRegistration.Add(_cr);
                    _customerActionService.UpdateCustomerAction(customerAction);
                }
            }
            return new NullJsonResult();
        }
        [HttpPost]
        public IActionResult ConditionCustomerRegisterUpdate(CustomerActionConditionModel.AddCustomerRegisterConditionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

            var customerAction = _customerActionService.GetCustomerActionById(model.CustomerActionId);
            if (customerAction != null)
            {
                var condition = customerAction.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                if (condition != null)
                {
                    var cr = condition.CustomerRegistration.FirstOrDefault(x => x.Id == model.Id);
                    cr.RegisterField = model.CustomerRegisterName;
                    cr.RegisterValue = model.CustomerRegisterValue;
                    _customerActionService.UpdateCustomerAction(customerAction);
                }
            }
            return new NullJsonResult();
        }

        [HttpGet]
        public IActionResult CustomerRegisterFields()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
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
        public IActionResult ConditionCustomCustomerAttribute(string customerActionId, string conditionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

            var customerAction = _customerActionService.GetCustomerActionById(model.CustomerActionId);
            if (customerAction != null)
            {
                var condition = customerAction.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                if (condition != null)
                {
                    var _cr = new CustomerAction.ActionCondition.CustomerRegister()
                    {
                        RegisterField = model.CustomerAttributeName,
                        RegisterValue = model.CustomerAttributeValue,
                    };
                    condition.CustomCustomerAttributes.Add(_cr);
                    _customerActionService.UpdateCustomerAction(customerAction);
                }
            }
            return new NullJsonResult();
        }
        [HttpPost]
        public IActionResult ConditionCustomCustomerAttributeUpdate(CustomerActionConditionModel.AddCustomCustomerAttributeConditionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

            var customerAction = _customerActionService.GetCustomerActionById(model.CustomerActionId);
            if (customerAction != null)
            {
                var condition = customerAction.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                if (condition != null)
                {
                    var cr = condition.CustomCustomerAttributes.FirstOrDefault(x => x.Id == model.Id);
                    cr.RegisterField = model.CustomerAttributeName;
                    cr.RegisterValue = model.CustomerAttributeValue;
                    _customerActionService.UpdateCustomerAction(customerAction);
                }
            }
            return new NullJsonResult();
        }

        [HttpGet]
        public IActionResult CustomCustomerAttributeFields()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
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


        #region Url Referrer

        [HttpPost]
        public IActionResult ConditionUrlReferrer(string customerActionId, string conditionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

            var customerAction = _customerActionService.GetCustomerActionById(model.CustomerActionId);
            if (customerAction != null)
            {
                var condition = customerAction.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                if (condition != null)
                {
                    var _url = new CustomerAction.ActionCondition.Url()
                    {
                        Name = model.Name
                    };
                    condition.UrlReferrer.Add(_url);
                    _customerActionService.UpdateCustomerAction(customerAction);
                }
            }
            return new NullJsonResult();
        }
        [HttpPost]
        public IActionResult ConditionUrlReferrerUpdate(CustomerActionConditionModel.AddUrlConditionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

            var customerAction = _customerActionService.GetCustomerActionById(model.CustomerActionId);
            if (customerAction != null)
            {
                var condition = customerAction.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                if (condition != null)
                {
                    var _url = condition.UrlReferrer.FirstOrDefault(x => x.Id == model.Id);
                    _url.Name = model.Name;
                    _customerActionService.UpdateCustomerAction(customerAction);
                }
            }
            return new NullJsonResult();
        }

        #endregion

        #region Url Current

        [HttpPost]
        public IActionResult ConditionUrlCurrent(string customerActionId, string conditionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

            var customerAction = _customerActionService.GetCustomerActionById(model.CustomerActionId);
            if (customerAction != null)
            {
                var condition = customerAction.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                if (condition != null)
                {
                    var _url = new CustomerAction.ActionCondition.Url()
                    {
                        Name = model.Name
                    };
                    condition.UrlCurrent.Add(_url);
                    _customerActionService.UpdateCustomerAction(customerAction);
                }
            }
            return new NullJsonResult();
        }
        [HttpPost]
        public IActionResult ConditionUrlCurrentUpdate(CustomerActionConditionModel.AddUrlConditionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageActions))
                return AccessDeniedView();

            var customerAction = _customerActionService.GetCustomerActionById(model.CustomerActionId);
            if (customerAction != null)
            {
                var condition = customerAction.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                if (condition != null)
                {
                    var _url = condition.UrlCurrent.FirstOrDefault(x => x.Id == model.Id);
                    _url.Name = model.Name;
                    _customerActionService.UpdateCustomerAction(customerAction);
                }
            }
            return new NullJsonResult();
        }

        #endregion


    }
}
