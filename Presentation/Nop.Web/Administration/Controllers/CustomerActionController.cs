using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Admin.Extensions;
using Nop.Admin.Models.Customers;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Security;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Controllers
{
    public partial class CustomerActionController : BaseAdminController
	{
		#region Fields

		private readonly ICustomerService _customerService;
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
        #endregion

        #region Constructors

        public CustomerActionController(ICustomerService customerService,
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
            ISpecificationAttributeService specificationAttributeService)
		{
            this._customerService = customerService;
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
        }

        #endregion


        #region Customer Actions

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

		public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();
            
			return View();
		}

		[HttpPost]
		public ActionResult List(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customeractions = _customerActionService.GetCustomerActions();
            var gridModel = new DataSourceResult
			{
                Data = customeractions.Select(x=> new { Id = x.Id, Name = x.Name, Active = x.Active }),
                Total = customeractions.Count()
			};
			return new JsonResult
			{
				Data = gridModel
			};
		}

        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();
            
            var model = new CustomerActionModel();
            model.Active = true;
            model.StartDateTimeUtc = DateTime.UtcNow;
            model.EndDateTimeUtc = DateTime.UtcNow.AddMonths(1);
            foreach(var item in _customerActionService.GetCustomerActionType())
            {
                model.ActionType.Add(new SelectListItem()
                {
                    Text = item.Name,
                    Value = item.Id.ToString()
                });
            }

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(CustomerActionModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();
            
            if (ModelState.IsValid)
            {
                var customeraction = model.ToEntity();
                _customerActionService.InsertCustomerAction(customeraction);

                //activity log
                _customerActivityService.InsertActivity("AddNewCustomerAction", customeraction.Id, _localizationService.GetResource("ActivityLog.AddNewCustomerAction"), customeraction.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerAction.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = customeraction.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

		public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customerAction = _customerActionService.GetCustomerActionById(id);
            if (customerAction == null)
                //No customer role found with the specified id
                return RedirectToAction("List");

            var model = customerAction.ToModel();
            foreach (var item in _customerActionService.GetCustomerActionType())
            {
                model.ActionType.Add(new SelectListItem()
                {
                    Text = item.Name,
                    Value = item.Id.ToString()
                });
            }

            return View(model);
		}

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Edit(CustomerActionModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();
            
            var customeraction = _customerActionService.GetCustomerActionById(model.Id);
            if (customeraction == null)
                //No customer role found with the specified id
                return RedirectToAction("List");

            try
            {
                if (ModelState.IsValid)
                {

                    customeraction = model.ToEntity(customeraction);
                    _customerActionService.UpdateCustomerAction(customeraction);

                    //activity log
                    _customerActivityService.InsertActivity("EditCustomerAction", customeraction.Id, _localizationService.GetResource("ActivityLog.EditCustomerAction"), customeraction.Name);

                    SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerAction.Updated"));
                    return continueEditing ? RedirectToAction("Edit", new { id = customeraction.Id}) : RedirectToAction("List");
                }

                //If we got this far, something failed, redisplay form
                return View(model);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("Edit", new { id = customeraction.Id });
            }
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();
            
            var customerAction = _customerActionService.GetCustomerActionById(id);
            if (customerAction == null)
                //No customer role found with the specified id
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
        public ActionResult Conditions(int customerActionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var conditions = _customerActionService.GetCustomerActionById(customerActionId);
            var gridModel = new DataSourceResult
            {
                Data = conditions.Conditions.Select(x=> new { Id = x.Id, Name = x.Name, Condition = _customerActionService.GetCustomerActionConditionTypeById(x.CustomerActionConditionTypeId).Name}),
                Total = conditions.Conditions.Count()
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        #endregion

        #region Conditions

        public ActionResult AddCondition(int customerActionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customerAction = _customerActionService.GetCustomerActionById(customerActionId);
            var customerActionType = _customerActionService.GetCustomerActionTypeById(customerAction.ActionTypeId);

            var model = new CustomerActionConditionModel();
            model.CustomerActionId = customerActionId;
            foreach(var item in customerActionType.ConditionType)
            {
                model.CustomerActionConditionType.Add(new SelectListItem()
                {
                    Value = item.ToString(),
                    Text = _customerActionService.GetCustomerActionConditionTypeById(item).Name
                });

            }
            return View(model);

        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult AddCondition(CustomerActionConditionModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var customerAction = _customerActionService.GetCustomerActionById(model.CustomerActionId);
                if(customerAction==null)
                {
                    return RedirectToAction("List");
                }

                var condition = new CustomerAction.ActionCondition()
                {
                    Id = customerAction.Conditions.Count > 0 ? customerAction.Conditions.Max(x => x.Id) + 1 : 1,
                    Name = model.Name,
                    CustomerActionConditionTypeId = model.CustomerActionConditionTypeId,                    
                    ConditionId = model.ConditionId,                    
                };
                customerAction.Conditions.Add(condition);
                _customerActionService.UpdateCustomerAction(customerAction);

                _customerActivityService.InsertActivity("AddNewCustomerActionCondition", customerAction.Id, _localizationService.GetResource("ActivityLog.AddNewCustomerAction"), customerAction.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerActionCondition.Added"));

                return continueEditing ? RedirectToAction("EditCondition", new { customerActionId = customerAction.Id, conditionId = condition.Id }) : RedirectToAction("Edit", new { id = customerAction.Id });
            }

            return View(model);
        }

        public ActionResult EditCondition(int customerActionId, int conditionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customerAction = _customerActionService.GetCustomerActionById(customerActionId);
            if (customerAction == null)
                //No customer role found with the specified id
                return RedirectToAction("List");

            var condition = customerAction.Conditions.FirstOrDefault(x => x.Id == conditionId);
            if (condition == null)
                //No customer role found with the specified id
                return RedirectToAction("List");

            var customerActionType = _customerActionService.GetCustomerActionTypeById(customerAction.ActionTypeId);

            var model = condition.ToModel();
            model.CustomerActionId = customerActionId;
            foreach (var item in customerActionType.ConditionType)
            {
                model.CustomerActionConditionType.Add(new SelectListItem()
                {
                    Value = item.ToString(),
                    Text = _customerActionService.GetCustomerActionConditionTypeById(item).Name
                });

            }
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult EditCondition(int customerActionId, int conditionId, CustomerActionConditionModel model, bool continueEditing)
        {
            var customerAction = _customerActionService.GetCustomerActionById(customerActionId);
            if (customerAction == null)
                //No customer role found with the specified id
                return RedirectToAction("List");

            var condition = customerAction.Conditions.FirstOrDefault(x => x.Id == conditionId);
            if (condition == null)
                //No customer role found with the specified id
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
                    return continueEditing ? RedirectToAction("EditCondition", new { customerActionId = customerAction.Id, conditionId = condition.Id }) : RedirectToAction("Edit", new { id = customerAction.Id });
                }

                //If we got this far, something failed, redisplay form
                return View(model);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("Edit", new { id = customerAction.Id });
            }
        }

        [HttpPost]
        public ActionResult ConditionDelete(int Id, int customerActionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customerAction = _customerActionService.GetCustomerActionById(customerActionId);
            var condition = customerAction.Conditions.FirstOrDefault(x => x.Id == Id);
            customerAction.Conditions.Remove(condition);
            _customerActionService.UpdateCustomerAction(customerAction);

            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult ConditionDeletePosition(int id, int customerActionId, int conditionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
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
                condition.ProductAttribute.Remove(condition.ProductAttribute.FirstOrDefault(x=>x.Id == id));
                _customerActionService.UpdateCustomerAction(customerActions);
            }
            if (condition.CustomerActionConditionTypeId == (int)CustomerActionConditionTypeEnum.ProductSpecification)
            {
                condition.ProductSpecifications.Remove(condition.ProductSpecifications.FirstOrDefault(x => x.Id == id));
                _customerActionService.UpdateCustomerAction(customerActions);
            }
            return new NullJsonResult();
        }

        #region Condition Product

        [HttpPost]
        public ActionResult ConditionProduct(int customerActionId, int conditionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customerActions = _customerActionService.GetCustomerActionById(customerActionId);
            var condition = customerActions.Conditions.FirstOrDefault(x => x.Id == conditionId);
            
            var gridModel = new DataSourceResult
            {
                Data = condition != null ? condition.Products.Select(z => new { Id = z, ProductName = _productService.GetProductById(z).Name}) : null,
                Total = customerActions.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }


        public ActionResult ProductAddPopup(int customerActionId, int conditionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var model = new CustomerActionConditionModel.AddProductToConditionModel();
            model.CustomerActionConditionId = conditionId;
            model.CustomerActionId = customerActionId;
            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            var categories = _categoryService.GetAllCategories(showHidden: true);
            foreach (var c in categories)
                model.AvailableCategories.Add(new SelectListItem { Text = c.GetFormattedBreadCrumb(categories), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var m in _manufacturerService.GetAllManufacturers(showHidden: true))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var v in _vendorService.GetAllVendors(showHidden: true))
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            return View(model);
        }

        [HttpPost]
        public ActionResult ProductAddPopupList(DataSourceRequest command, CustomerActionConditionModel.AddProductToConditionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var products = _productService.SearchProducts(
                categoryIds: new List<int> { model.SearchCategoryId },
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
        public ActionResult ProductAddPopup(string btnId, CustomerActionConditionModel.AddProductToConditionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (model.SelectedProductIds != null)
            {
                foreach (int id in model.SelectedProductIds)
                {
                    var customerAction = _customerActionService.GetCustomerActionById(model.CustomerActionId);
                    if(customerAction!=null)
                    {
                        var condition = customerAction.Conditions.FirstOrDefault(x => x.Id == model.CustomerActionConditionId);
                        if(condition!=null)
                        {
                            if(condition.Products.Where(x=>x == id).Count()==0)
                            {
                                condition.Products.Add(id);
                                _customerActionService.UpdateCustomerAction(customerAction);
                            }
                        }
                    }
                }
            }
            ViewBag.RefreshPage = true;
            ViewBag.btnId = btnId;
            return View(model);
        }
        #endregion

        #region Condition Category

        [HttpPost]
        public ActionResult ConditionCategory(int customerActionId, int conditionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customerActions = _customerActionService.GetCustomerActionById(customerActionId);
            var condition = customerActions.Conditions.FirstOrDefault(x => x.Id == conditionId);

            var gridModel = new DataSourceResult
            {
                Data = condition != null ? condition.Categories.Select(z => new { Id = z, CategoryName = _categoryService.GetCategoryById(z).Name }) : null,
                Total = customerActions.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        public ActionResult CategoryAddPopup(int customerActionId, int conditionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            var model = new CustomerActionConditionModel.AddCategoryConditionModel();
            model.CustomerActionConditionId = conditionId;
            model.CustomerActionId = customerActionId;
            return View(model);
        }

        [HttpPost]
        public ActionResult CategoryAddPopupList(DataSourceRequest command, CustomerActionConditionModel.AddCategoryConditionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var categories = _categoryService.GetAllCategories(model.SearchCategoryName,
                command.Page - 1, command.PageSize, true);
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
        public ActionResult CategoryAddPopup(string btnId, CustomerActionConditionModel.AddCategoryConditionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return AccessDeniedView();

            if (model.SelectedCategoryIds != null)
            {
                foreach (int id in model.SelectedCategoryIds)
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
            ViewBag.btnId = btnId;
            return View(model);
        }


        #endregion

        #region Condition Manufacturer
        [HttpPost]
        public ActionResult ConditionManufacturer(int customerActionId, int conditionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customerActions = _customerActionService.GetCustomerActionById(customerActionId);
            var condition = customerActions.Conditions.FirstOrDefault(x => x.Id == conditionId);

            var gridModel = new DataSourceResult
            {
                Data = condition != null ? condition.Manufacturers.Select(z => new { Id = z, ManufacturerName = _manufacturerService.GetManufacturerById(z).Name }) : null,
                Total = customerActions.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        public ActionResult ManufacturerAddPopup(int customerActionId, int conditionId)
        {
            var model = new CustomerActionConditionModel.AddManufacturerConditionModel();
            model.CustomerActionConditionId = conditionId;
            model.CustomerActionId = customerActionId;
            return View(model);

        }

        [HttpPost]
        public ActionResult ManufacturerAddPopupList(DataSourceRequest command, CustomerActionConditionModel.AddManufacturerConditionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return AccessDeniedView();

            var manufacturers = _manufacturerService.GetAllManufacturers(model.SearchManufacturerName,
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
        public ActionResult ManufacturerAddPopup(string btnId, CustomerActionConditionModel.AddManufacturerConditionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                return AccessDeniedView();

            if (model.SelectedManufacturerIds != null)
            {
                foreach (int id in model.SelectedManufacturerIds)
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
            ViewBag.btnId = btnId;
            return View(model);
        }


        #endregion

        #region Condition Vendor

        [HttpPost]
        public ActionResult ConditionVendor(int customerActionId, int conditionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customerActions = _customerActionService.GetCustomerActionById(customerActionId);
            var condition = customerActions.Conditions.FirstOrDefault(x => x.Id == conditionId);

            var gridModel = new DataSourceResult
            {
                Data = condition != null ? condition.Vendors.Select(z => new { Id = z, VendorName = _vendorService.GetVendorById(z).Name }) : null,
                Total = customerActions.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        [HttpPost]
        public ActionResult ConditionVendorInsert(CustomerActionConditionModel.AddVendorConditionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customerAction = _customerActionService.GetCustomerActionById(model.CustomerActionId);
            if (customerAction != null)
            {
                var condition = customerAction.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                if (condition != null)
                {
                    if (condition.Vendors.Where(x => x == model.Id).Count() == 0)
                    {
                        condition.Vendors.Add(model.VendorId);
                        _customerActionService.UpdateCustomerAction(customerAction);
                    }
                }
            }
            return new NullJsonResult();
        }


        [HttpGet]
        public ActionResult Vendors()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();
            var customerVendors = _vendorService.GetAllVendors().Select(x => new { Id = x.Id, Name = x.Name });
            return Json
                (
                    customerVendors,
                    JsonRequestBehavior.AllowGet
                );
        }

        #endregion

        #region Condition Product Attributes

        [HttpPost]
        public ActionResult ConditionProductAttribute(int customerActionId, int conditionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customerActions = _customerActionService.GetCustomerActionById(customerActionId);
            var condition = customerActions.Conditions.FirstOrDefault(x => x.Id == conditionId);

            var gridModel = new DataSourceResult
            {
                Data = condition != null ? condition.ProductAttribute.Select(z => new { Id = z.Id, ProductAttributeId = z.ProductAttributeId, ProductAttributeName = _productAttributeService.GetProductAttributeById(z.ProductAttributeId).Name, Name = z.Name }) : null,
                Total = customerActions.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }
       
        [HttpPost]
        public ActionResult ConditionProductAttributeInsert(CustomerActionConditionModel.AddProductAttributeConditionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customerAction = _customerActionService.GetCustomerActionById(model.CustomerActionId);
            if (customerAction != null)
            {
                var condition = customerAction.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                if (condition != null)
                {
                    var _pv = new CustomerAction.ActionCondition.ProductAttributeValue() {
                        Id = condition.ProductAttribute.Count > 0 ? condition.ProductAttribute.Max(x=>x.Id) + 1: 1,
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
        public ActionResult ConditionProductAttributeUpdate(CustomerActionConditionModel.AddProductAttributeConditionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
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
        public ActionResult ProductAttributes()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();
            var customerAttr = _productAttributeService.GetAllProductAttributes().Select(x => new { Id = x.Id, Name = x.Name });
            return Json
                (
                    customerAttr,
                    JsonRequestBehavior.AllowGet
                );
        }
        #endregion

        #region Condition Product Specification

        [HttpPost]
        public ActionResult ConditionProductSpecification(int customerActionId, int conditionId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customerActions = _customerActionService.GetCustomerActionById(customerActionId);
            var condition = customerActions.Conditions.FirstOrDefault(x => x.Id == conditionId);

            var gridModel = new DataSourceResult
            {
                Data = condition != null ? condition.ProductSpecifications
                        .Select(z => new { Id = z.Id, SpecificationId = z.ProductSpecyficationId,
                            SpecificationName = _specificationAttributeService.GetSpecificationAttributeById(z.ProductSpecyficationId).Name,
                            SpecificationValueName = z.ProductSpecyficationValueId > 0 ? _specificationAttributeService.GetSpecificationAttributeById(z.ProductSpecyficationId).SpecificationAttributeOptions.FirstOrDefault(x=>x.Id == z.ProductSpecyficationValueId).Name: "Undefined"
                        }) : null,
                Total = customerActions.Conditions.Where(x => x.Id == conditionId).Count()
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }


        [HttpPost]
        public ActionResult ConditionProductSpecificationInsert(CustomerActionConditionModel.AddProductSpecificationConditionModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customerAction = _customerActionService.GetCustomerActionById(model.CustomerActionId);
            if (customerAction != null)
            {
                var condition = customerAction.Conditions.FirstOrDefault(x => x.Id == model.ConditionId);
                if (condition != null)
                {
                    if(condition.ProductSpecifications.Where(x=>x.ProductSpecyficationId == model.SpecificationId && x.ProductSpecyficationValueId == model.SpecificationValueId).Count() == 0)
                    {
                        var _ps = new CustomerAction.ActionCondition.ProductSpecification()
                        {
                            Id = condition.ProductSpecifications.Count > 0 ? condition.ProductSpecifications.Max(x => x.Id) + 1 : 1,
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
        public ActionResult ProductSpecification()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();
            var customerAttr = _specificationAttributeService.GetSpecificationAttributes().Select(x => new { Id = x.Id, Name = x.Name });
            return Json
                (
                    customerAttr,
                    JsonRequestBehavior.AllowGet
                );
        }
        [HttpGet]
        public ActionResult ProductSpecificationValue(int specificationId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            if(specificationId==0)
                return new NullJsonResult();

            var customerSpec = _specificationAttributeService.GetSpecificationAttributeById(specificationId).SpecificationAttributeOptions.Select(x => new { Id = x.Id, Name = x.Name });

            return Json
                (
                    customerSpec,
                    JsonRequestBehavior.AllowGet
                );
        }


        #endregion

        #endregion

    }
}
