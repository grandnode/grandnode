using System;
using System.Collections.Generic;
using System.Linq;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Customers;
using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Services.Vendors;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Microsoft.AspNetCore.Mvc;
using Grand.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Extensions;

namespace Grand.Web.Areas.Admin.Controllers
{
    public partial class CustomerTagController : BaseAdminController
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
        private readonly ICustomerTagService _customerTagService;
        #endregion

        #region Constructors

        public CustomerTagController(ICustomerService customerService,
            ILocalizationService localizationService,
            ICustomerActivityService customerActivityService,
            IPermissionService permissionService,
            IProductService productService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IStoreService storeService,
            IVendorService vendorService,
            IWorkContext workContext,
            ICustomerTagService customerTagService)
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
            this._customerTagService = customerTagService;
        }

        #endregion


        [NonAction]
        protected virtual CustomerModel PrepareCustomerModelForList(Customer customer)
        {
            return new CustomerModel
            {
                Id = customer.Id,
                Email = customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest"),
            };
        }

        #region Customer Tags

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public IActionResult List(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customertags = _customerTagService.GetAllCustomerTags();
            var gridModel = new DataSourceResult
            {
                Data = customertags.Select(x => new { Id = x.Id, Name = x.Name, Count = _customerTagService.GetCustomerCount(x.Id) }),
                Total = customertags.Count()
            };
            return Json(gridModel);
        }

        [HttpGet]
        public IActionResult Search(string term)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();
            var customertags = _customerTagService.GetCustomerTagsByName(term).Select(x => x.Name);
            return Json(customertags);
        }



        [HttpPost]
        public IActionResult Customers(string customerTagId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customers = _customerTagService.GetCustomersByTag(customerTagId, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = customers.Select(PrepareCustomerModelForList),
                Total = customers.TotalCount
            };
            return Json(gridModel);
        }

        public IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var model = new CustomerTagModel();
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(CustomerTagModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var customertag = model.ToEntity();
                customertag.Name = customertag.Name.ToLower();
                _customerTagService.InsertCustomerTag(customertag);

                //activity log
                _customerActivityService.InsertActivity("AddNewCustomerTag", customertag.Id, _localizationService.GetResource("ActivityLog.AddNewCustomerTag"), customertag.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerTags.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = customertag.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        public IActionResult Edit(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customerTag = _customerTagService.GetCustomerTagById(id);
            if (customerTag == null)
                //No customer role found with the specified id
                return RedirectToAction("List");

            var model = customerTag.ToModel();
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(CustomerTagModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customertag = _customerTagService.GetCustomerTagById(model.Id);
            if (customertag == null)
                //No customer role found with the specified id
                return RedirectToAction("List");

            try
            {
                if (ModelState.IsValid)
                {

                    customertag = model.ToEntity(customertag);
                    customertag.Name = customertag.Name.ToLower();

                    _customerTagService.UpdateCustomerTag(customertag);

                    //activity log
                    _customerActivityService.InsertActivity("EditCustomerTage", customertag.Id, _localizationService.GetResource("ActivityLog.EditCustomerTag"), customertag.Name);

                    SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerTags.Updated"));
                    return continueEditing ? RedirectToAction("Edit", new { id = customertag.Id }) : RedirectToAction("List");
                }

                //If we got this far, something failed, redisplay form
                return View(model);
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("Edit", new { id = customertag.Id });
            }
        }

        [HttpPost]
        public IActionResult CustomerDelete(string Id, string customerTagId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customertag = _customerTagService.GetCustomerTagById(customerTagId);
            if (customertag == null)
                throw new ArgumentException("No customertag found with the specified id");

            _customerTagService.DeleteTagFromCustomer(customerTagId, Id);

            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var customerTag = _customerTagService.GetCustomerTagById(id);
            if (customerTag == null)
                //No customer role found with the specified id
                return RedirectToAction("List");

            try
            {
                //activity log
                _customerActivityService.InsertActivity("DeleteCustomerTag", customerTag.Id, _localizationService.GetResource("ActivityLog.DeleteCustomerTag"), customerTag.Name);

                _customerTagService.DeleteCustomerTag(customerTag);

                SuccessNotification(_localizationService.GetResource("Admin.Customers.CustomerTags.Deleted"));
                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
                return RedirectToAction("Edit", new { id = customerTag.Id });
            }

        }


        #endregion

        #region Products

        [HttpPost]
        public IActionResult Products(string customerTagId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var products = _customerTagService.GetCustomerTagProducts(customerTagId);

            var gridModel = new DataSourceResult
            {
                Data = products.Select(x => new CustomerRoleProductModel
                {
                    Id = x.Id,
                    Name = _productService.GetProductById(x.ProductId)?.Name,
                    ProductId = x.ProductId,
                    DisplayOrder = x.DisplayOrder
                }),
                Total = products.Count()
            };
            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ProductDelete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var ctp = _customerTagService.GetCustomerTagProductById(id);
            if (ctp == null)
                throw new ArgumentException("No found the specified id");

            _customerTagService.DeleteCustomerTagProduct(ctp);

            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult ProductUpdate(CustomerRoleProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var ctp = _customerTagService.GetCustomerTagProductById(model.Id);
            if (ctp == null)
                throw new ArgumentException("No customer tag product found with the specified id");

            ctp.DisplayOrder = model.DisplayOrder;
            _customerTagService.UpdateCustomerTagProduct(ctp);

            return new NullJsonResult();
        }

        public IActionResult ProductAddPopup(string customerTagId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var model = new CustomerTagProductModel.AddProductModel();
            model.CustomerTagId = customerTagId;
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
        public IActionResult ProductAddPopupList(DataSourceRequest command, CustomerTagProductModel.AddProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
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
        public IActionResult ProductAddPopup(CustomerTagProductModel.AddProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (model.SelectedProductIds != null)
            {
                foreach (string id in model.SelectedProductIds)
                {
                    var product = _productService.GetProductById(id);
                    if (product != null)
                    {
                        var customerTagProduct = _customerTagService.GetCustomerTagProduct(model.CustomerTagId, id);
                        if (customerTagProduct == null)
                        {
                            customerTagProduct = new CustomerTagProduct();
                            customerTagProduct.CustomerTagId = model.CustomerTagId;
                            customerTagProduct.ProductId = id;
                            customerTagProduct.DisplayOrder = 0;
                            _customerTagService.InsertCustomerTagProduct(customerTagProduct);
                        }
                    }
                }
            }

            //a vendor should have access only to his products
            ViewBag.RefreshPage = true;
            return View(model);
        }


        #endregion
    }
}
