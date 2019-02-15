using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Discounts;
using Grand.Framework.Extensions;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Discounts;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Media;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Services.Vendors;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Catalog;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Services
{
    public partial class ManufacturerViewModelService : IManufacturerViewModelService
    {
        #region Fields

        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IManufacturerTemplateService _manufacturerTemplateService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IStoreService _storeService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly IDiscountService _discountService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IVendorService _vendorService;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion

        #region Constructors

        public ManufacturerViewModelService(ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IManufacturerTemplateService manufacturerTemplateService,
            IProductService productService,
            ICustomerService customerService,
            IStoreService storeService,
            IUrlRecordService urlRecordService,
            IPictureService pictureService,
            ILocalizationService localizationService,
            IDiscountService discountService,
            ICustomerActivityService customerActivityService,
            IVendorService vendorService,
            IDateTimeHelper dateTimeHelper)
        {
            this._categoryService = categoryService;
            this._manufacturerTemplateService = manufacturerTemplateService;
            this._manufacturerService = manufacturerService;
            this._productService = productService;
            this._customerService = customerService;
            this._storeService = storeService;
            this._urlRecordService = urlRecordService;
            this._pictureService = pictureService;
            this._localizationService = localizationService;
            this._discountService = discountService;
            this._customerActivityService = customerActivityService;
            this._vendorService = vendorService;
            this._dateTimeHelper = dateTimeHelper;
        }

        #endregion

        public virtual void PrepareTemplatesModel(ManufacturerModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            var templates = _manufacturerTemplateService.GetAllManufacturerTemplates();
            foreach (var template in templates)
            {
                model.AvailableManufacturerTemplates.Add(new SelectListItem
                {
                    Text = template.Name,
                    Value = template.Id.ToString()
                });
            }
        }


        public virtual void PrepareDiscountModel(ManufacturerModel model, Manufacturer manufacturer, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableDiscounts = _discountService
                .GetAllDiscounts(DiscountType.AssignedToManufacturers, showHidden: true)
                .Select(d => d.ToModel())
                .ToList();

            if (!excludeProperties && manufacturer != null)
            {
                model.SelectedDiscountIds = manufacturer.AppliedDiscounts.ToArray();
            }
        }

        public virtual Manufacturer InsertManufacturerModel(ManufacturerModel model)
        {
            var manufacturer = model.ToEntity();
            manufacturer.CreatedOnUtc = DateTime.UtcNow;
            manufacturer.UpdatedOnUtc = DateTime.UtcNow;
            //discounts
            var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToManufacturers, showHidden: true);
            foreach (var discount in allDiscounts)
            {
                if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                    manufacturer.AppliedDiscounts.Add(discount.Id);
            }

            _manufacturerService.InsertManufacturer(manufacturer);
            //search engine name
            manufacturer.Locales = model.Locales.ToLocalizedProperty(manufacturer, x => x.Name, _urlRecordService);
            model.SeName = manufacturer.ValidateSeName(model.SeName, manufacturer.Name, true);
            manufacturer.SeName = model.SeName;
            _manufacturerService.UpdateManufacturer(manufacturer);

            _urlRecordService.SaveSlug(manufacturer, model.SeName, "");

            //update picture seo file name
            _pictureService.UpdatePictureSeoNames(manufacturer.PictureId, manufacturer.Name);

            //activity log
            _customerActivityService.InsertActivity("AddNewManufacturer", manufacturer.Id, _localizationService.GetResource("ActivityLog.AddNewManufacturer"), manufacturer.Name);
            return manufacturer;
        }

        public virtual Manufacturer UpdateManufacturerModel(Manufacturer manufacturer, ManufacturerModel model)
        {
            string prevPictureId = manufacturer.PictureId;
            manufacturer = model.ToEntity(manufacturer);
            manufacturer.UpdatedOnUtc = DateTime.UtcNow;
            manufacturer.Locales = model.Locales.ToLocalizedProperty(manufacturer, x => x.Name, _urlRecordService);
            //discounts
            var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToManufacturers, showHidden: true);
            foreach (var discount in allDiscounts)
            {
                if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                {
                    //new discount
                    if (manufacturer.AppliedDiscounts.Count(d => d == discount.Id) == 0)
                        manufacturer.AppliedDiscounts.Add(discount.Id);
                }
                else
                {
                    //remove discount
                    if (manufacturer.AppliedDiscounts.Count(d => d == discount.Id) > 0)
                        manufacturer.AppliedDiscounts.Remove(discount.Id);
                }
            }
            model.SeName = manufacturer.ValidateSeName(model.SeName, manufacturer.Name, true);
            manufacturer.SeName = model.SeName;

            _manufacturerService.UpdateManufacturer(manufacturer);
            //search engine name
            _urlRecordService.SaveSlug(manufacturer, model.SeName, "");

            //delete an old picture (if deleted or updated)
            if (!String.IsNullOrEmpty(prevPictureId) && prevPictureId != manufacturer.PictureId)
            {
                var prevPicture = _pictureService.GetPictureById(prevPictureId);
                if (prevPicture != null)
                    _pictureService.DeletePicture(prevPicture);
            }
            //update picture seo file name
            _pictureService.UpdatePictureSeoNames(manufacturer.PictureId, manufacturer.Name);

            //activity log
            _customerActivityService.InsertActivity("EditManufacturer", manufacturer.Id, _localizationService.GetResource("ActivityLog.EditManufacturer"), manufacturer.Name);
            return manufacturer;
        }
        public virtual void DeleteManufacturer(Manufacturer manufacturer)
        {
            _manufacturerService.DeleteManufacturer(manufacturer);
            //activity log
            _customerActivityService.InsertActivity("DeleteManufacturer", manufacturer.Id, _localizationService.GetResource("ActivityLog.DeleteManufacturer"), manufacturer.Name);

        }

        public virtual ManufacturerModel.AddManufacturerProductModel PrepareAddManufacturerProductModel()
        {
            var model = new ManufacturerModel.AddManufacturerProductModel();
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

        public virtual (IList<ProductModel> products, int totalCount) PrepareProductModel(ManufacturerModel.AddManufacturerProductModel model, int pageIndex, int pageSize)
        {
            var products = _productService.PrepareProductList(model.SearchCategoryId, model.SearchManufacturerId, model.SearchStoreId, model.SearchVendorId, model.SearchProductTypeId, model.SearchProductName, pageIndex, pageSize);
            return (products.Select(x => x.ToModel()).ToList(), products.TotalCount);
        }

        public virtual (IEnumerable<ManufacturerModel.ManufacturerProductModel> manufacturerProductModels, int totalCount) PrepareManufacturerProductModel(string manufacturerId, int pageIndex, int pageSize)
        {
            var productManufacturers = _manufacturerService.GetProductManufacturersByManufacturerId(manufacturerId,
                pageIndex - 1, pageSize, true);

            return (productManufacturers.Select(x => new ManufacturerModel.ManufacturerProductModel
            {
                Id = x.Id,
                ManufacturerId = x.ManufacturerId,
                ProductId = x.ProductId,
                ProductName = _productService.GetProductById(x.ProductId).Name,
                IsFeaturedProduct = x.IsFeaturedProduct,
                DisplayOrder = x.DisplayOrder
            }), productManufacturers.TotalCount);
        }
        public virtual void ProductUpdate(ManufacturerModel.ManufacturerProductModel model)
        {
            var product = _productService.GetProductById(model.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var productManufacturer = product.ProductManufacturers.Where(x => x.Id == model.Id).FirstOrDefault();
            if (productManufacturer == null)
                throw new ArgumentException("No product manufacturer mapping found with the specified id");

            productManufacturer.IsFeaturedProduct = model.IsFeaturedProduct;
            productManufacturer.DisplayOrder = model.DisplayOrder;
            productManufacturer.ProductId = model.ProductId;
            _manufacturerService.UpdateProductManufacturer(productManufacturer);
        }
        public virtual void ProductDelete(string id, string productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var productManufacturer = product.ProductManufacturers.Where(x => x.Id == id).FirstOrDefault();
            if (productManufacturer == null)
                throw new ArgumentException("No product manufacturer mapping found with the specified id");

            productManufacturer.ProductId = productId;

            _manufacturerService.DeleteProductManufacturer(productManufacturer);
        }
        public virtual void InsertManufacturerProductModel(ManufacturerModel.AddManufacturerProductModel model)
        {
            foreach (string id in model.SelectedProductIds)
            {
                var product = _productService.GetProductById(id);
                if (product != null)
                {
                    var existingProductmanufacturers = product.ProductManufacturers;
                    if (product.ProductManufacturers.Where(x => x.ManufacturerId == model.ManufacturerId).Count() == 0)
                    {
                        _manufacturerService.InsertProductManufacturer(
                            new ProductManufacturer
                            {
                                ManufacturerId = model.ManufacturerId,
                                ProductId = id,
                                IsFeaturedProduct = false,
                                DisplayOrder = 1,
                            });
                    }
                }
            }
        }
        public virtual (IEnumerable<ManufacturerModel.ActivityLogModel> activityLogModels, int totalCount) PrepareActivityLogModel(string manufacturerId, int pageIndex, int pageSize)
        {
            var activityLog = _customerActivityService.GetManufacturerActivities(null, null, manufacturerId, pageIndex - 1, pageSize);
            return (activityLog.Select(x =>
            {
                var customer = _customerService.GetCustomerById(x.CustomerId);
                var m = new ManufacturerModel.ActivityLogModel
                {
                    Id = x.Id,
                    ActivityLogTypeName = _customerActivityService.GetActivityTypeById(x.ActivityLogTypeId)?.Name,
                    Comment = x.Comment,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc),
                    CustomerId = x.CustomerId,
                    CustomerEmail = customer != null ? customer.Email : "null"
                };
                return m;

            }), activityLog.TotalCount);
        }
    }
}
