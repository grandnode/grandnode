using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Framework.Extensions;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Services.Vendors;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Web.Areas.Admin.Models.Customers;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Services
{
    public partial class CustomerRoleViewModelService : ICustomerRoleViewModelService
    {
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IStoreService _storeService;
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;

        #region Constructors

        public CustomerRoleViewModelService(ICustomerService customerService,
            ILocalizationService localizationService,
            ICustomerActivityService customerActivityService,
            IPermissionService permissionService,
            IProductService productService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IStoreService storeService,
            IVendorService vendorService,
            IWorkContext workContext)
        {
            _customerService = customerService;
            _localizationService = localizationService;
            _customerActivityService = customerActivityService;
            _productService = productService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _storeService = storeService;
            _vendorService = vendorService;
            _workContext = workContext;
        }

        #endregion

        public virtual CustomerRoleModel PrepareCustomerRoleModel(CustomerRole customerRole)
        {
            var model = customerRole.ToModel();
            var product = _productService.GetProductById(customerRole.PurchasedWithProductId);
            if (product != null)
            {
                model.PurchasedWithProductName = product.Name;
            }
            return model;
        }

        public virtual CustomerRoleModel PrepareCustomerRoleModel()
        {
            var model = new CustomerRoleModel();
            //default values
            model.Active = true;
            return model;
        }

        public virtual CustomerRole InsertCustomerRoleModel(CustomerRoleModel model)
        {
            var customerRole = model.ToEntity();
            _customerService.InsertCustomerRole(customerRole);
            //activity log
            _customerActivityService.InsertActivity("AddNewCustomerRole", customerRole.Id, _localizationService.GetResource("ActivityLog.AddNewCustomerRole"), customerRole.Name);
            return customerRole;
        }
        public virtual CustomerRole UpdateCustomerRoleModel(CustomerRole customerRole, CustomerRoleModel model)
        {
            customerRole = model.ToEntity(customerRole);
            _customerService.UpdateCustomerRole(customerRole);

            //activity log
            _customerActivityService.InsertActivity("EditCustomerRole", customerRole.Id, _localizationService.GetResource("ActivityLog.EditCustomerRole"), customerRole.Name);
            return customerRole;
        }
        public virtual void DeleteCustomerRole(CustomerRole customerRole)
        {
            _customerService.DeleteCustomerRole(customerRole);

            //activity log
            _customerActivityService.InsertActivity("DeleteCustomerRole", customerRole.Id, _localizationService.GetResource("ActivityLog.DeleteCustomerRole"), customerRole.Name);
        }
        public virtual CustomerRoleModel.AssociateProductToCustomerRoleModel PrepareAssociateProductToCustomerRoleModel()
        {
            var model = new CustomerRoleModel.AssociateProductToCustomerRoleModel();
            //a vendor should have access only to his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;

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
            return model;
        }
        public virtual (IList<ProductModel> products, int totalCount) PrepareProductModel(CustomerRoleModel.AssociateProductToCustomerRoleModel model, int pageIndex, int pageSize)
        {
            var products = _productService.PrepareProductList(model.SearchCategoryId, model.SearchManufacturerId, model.SearchStoreId, model.SearchVendorId, model.SearchProductTypeId, model.SearchProductName, pageIndex, pageSize);
            return (products.Select(x => x.ToModel()).ToList(), products.TotalCount);
        }
        public virtual IList<CustomerRoleProductModel> PrepareCustomerRoleProductModel(string customerRoleId)
        {
            var products = _customerService.GetCustomerRoleProducts(customerRoleId);
            return products.Select(x => new CustomerRoleProductModel
            {
                Id = x.Id,
                Name = _productService.GetProductById(x.ProductId)?.Name,
                ProductId = x.ProductId,
                DisplayOrder = x.DisplayOrder
            }).ToList();
        }
        public virtual CustomerRoleProductModel.AddProductModel PrepareProductModel(string customerRoleId)
        {
            var model = new CustomerRoleProductModel.AddProductModel();
            model.CustomerRoleId = customerRoleId;
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
            return model;
        }

        public virtual (IList<ProductModel> products, int totalCount) PrepareProductModel(CustomerRoleProductModel.AddProductModel model, int pageIndex, int pageSize)
        {
            var products = _productService.PrepareProductList(model.SearchCategoryId, model.SearchManufacturerId, model.SearchStoreId, model.SearchVendorId, model.SearchProductTypeId, model.SearchProductName, pageIndex, pageSize);
            return (products.Select(x => x.ToModel()).ToList(), products.TotalCount);
        }
        public virtual void InsertProductModel(CustomerRoleProductModel.AddProductModel model)
        {
            foreach (string id in model.SelectedProductIds)
            {
                var product = _productService.GetProductById(id);
                if (product != null)
                {
                    var customerRoleProduct = _customerService.GetCustomerRoleProduct(model.CustomerRoleId, id);
                    if (customerRoleProduct == null)
                    {
                        customerRoleProduct = new CustomerRoleProduct();
                        customerRoleProduct.CustomerRoleId = model.CustomerRoleId;
                        customerRoleProduct.ProductId = id;
                        customerRoleProduct.DisplayOrder = 0;
                        _customerService.InsertCustomerRoleProduct(customerRoleProduct);
                    }
                }
            }
        }
    }
}
