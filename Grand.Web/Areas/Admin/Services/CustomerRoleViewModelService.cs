using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Framework.Extensions;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Stores;
using Grand.Services.Vendors;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Web.Areas.Admin.Models.Customers;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        private readonly IDateTimeHelper _dateTimeHelper;

        #region Constructors

        public CustomerRoleViewModelService(ICustomerService customerService,
            ILocalizationService localizationService,
            ICustomerActivityService customerActivityService,
            IProductService productService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IStoreService storeService,
            IVendorService vendorService,
            IDateTimeHelper dateTimeHelper)
        {
            _customerService = customerService;
            _localizationService = localizationService;
            _customerActivityService = customerActivityService;
            _productService = productService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _storeService = storeService;
            _vendorService = vendorService;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion

        public virtual CustomerRoleModel PrepareCustomerRoleModel(CustomerRole customerRole)
        {
            var model = customerRole.ToModel();
            return model;
        }

        public virtual CustomerRoleModel PrepareCustomerRoleModel()
        {
            var model = new CustomerRoleModel();
            //default values
            model.Active = true;
            return model;
        }

        public virtual async Task<CustomerRole> InsertCustomerRoleModel(CustomerRoleModel model)
        {
            var customerRole = model.ToEntity();
            await _customerService.InsertCustomerRole(customerRole);
            //activity log
            await _customerActivityService.InsertActivity("AddNewCustomerRole", customerRole.Id, _localizationService.GetResource("ActivityLog.AddNewCustomerRole"), customerRole.Name);
            return customerRole;
        }
        public virtual async Task<CustomerRole> UpdateCustomerRoleModel(CustomerRole customerRole, CustomerRoleModel model)
        {
            customerRole = model.ToEntity(customerRole);
            await _customerService.UpdateCustomerRole(customerRole);

            //activity log
            await _customerActivityService.InsertActivity("EditCustomerRole", customerRole.Id, _localizationService.GetResource("ActivityLog.EditCustomerRole"), customerRole.Name);
            return customerRole;
        }
        public virtual async Task DeleteCustomerRole(CustomerRole customerRole)
        {
            await _customerService.DeleteCustomerRole(customerRole);

            //activity log
            await _customerActivityService.InsertActivity("DeleteCustomerRole", customerRole.Id, _localizationService.GetResource("ActivityLog.DeleteCustomerRole"), customerRole.Name);
        }
        public virtual async Task<IList<CustomerRoleProductModel>> PrepareCustomerRoleProductModel(string customerRoleId)
        {
            var products = await _customerService.GetCustomerRoleProducts(customerRoleId);
            var model = new List<CustomerRoleProductModel>();
            foreach (var item in products)
            {
                var cr = new CustomerRoleProductModel {
                    Id = item.Id,
                    Name = (await _productService.GetProductById(item.ProductId))?.Name,
                    ProductId = item.ProductId,
                    DisplayOrder = item.DisplayOrder
                };
                model.Add(cr);
            }
            return model;
        }
        public virtual async Task<CustomerRoleProductModel.AddProductModel> PrepareProductModel(string customerRoleId)
        {
            var model = new CustomerRoleProductModel.AddProductModel();
            model.CustomerRoleId = customerRoleId;
            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            var categories = await _categoryService.GetAllCategories(showHidden: true);
            foreach (var c in categories)
                model.AvailableCategories.Add(new SelectListItem { Text = _categoryService.GetFormattedBreadCrumb(c, categories), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var m in await _manufacturerService.GetAllManufacturers(showHidden: true))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var s in await _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Shortcut, Value = s.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var v in await _vendorService.GetAllVendors(showHidden: true))
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList().ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            return model;
        }

        public virtual async Task<(IList<ProductModel> products, int totalCount)> PrepareProductModel(CustomerRoleProductModel.AddProductModel model, int pageIndex, int pageSize)
        {
            var products = await _productService.PrepareProductList(model.SearchCategoryId, model.SearchManufacturerId, model.SearchStoreId, model.SearchVendorId, model.SearchProductTypeId, model.SearchProductName, pageIndex, pageSize);
            return (products.Select(x => x.ToModel(_dateTimeHelper)).ToList(), products.TotalCount);
        }
        public virtual async Task InsertProductModel(CustomerRoleProductModel.AddProductModel model)
        {
            foreach (string id in model.SelectedProductIds)
            {
                var product = await _productService.GetProductById(id);
                if (product != null)
                {
                    var customerRoleProduct = await _customerService.GetCustomerRoleProduct(model.CustomerRoleId, id);
                    if (customerRoleProduct == null)
                    {
                        customerRoleProduct = new CustomerRoleProduct();
                        customerRoleProduct.CustomerRoleId = model.CustomerRoleId;
                        customerRoleProduct.ProductId = id;
                        customerRoleProduct.DisplayOrder = 0;
                        await _customerService.InsertCustomerRoleProduct(customerRoleProduct);
                    }
                }
            }
        }
    }
}
