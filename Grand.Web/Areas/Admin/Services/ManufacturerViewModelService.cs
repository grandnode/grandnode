using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Discounts;
using Grand.Domain.Seo;
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
using System.Threading.Tasks;

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
        private readonly ILanguageService _languageService;
        private readonly IWorkContext _workContext;
        private readonly SeoSettings _seoSettings;

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
            IDateTimeHelper dateTimeHelper,
            ILanguageService languageService,
            IWorkContext workContext,
            SeoSettings seoSettings)
        {
            _categoryService = categoryService;
            _manufacturerTemplateService = manufacturerTemplateService;
            _manufacturerService = manufacturerService;
            _productService = productService;
            _customerService = customerService;
            _storeService = storeService;
            _urlRecordService = urlRecordService;
            _pictureService = pictureService;
            _localizationService = localizationService;
            _discountService = discountService;
            _customerActivityService = customerActivityService;
            _vendorService = vendorService;
            _dateTimeHelper = dateTimeHelper;
            _languageService = languageService;
            _workContext = workContext;
            _seoSettings = seoSettings;
        }

        #endregion

        public virtual void PrepareSortOptionsModel(ManufacturerModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableSortOptions = ProductSortingEnum.Position.ToSelectList().ToList();
            model.AvailableSortOptions.Insert(0, new SelectListItem { Text = "None", Value = "-1" });
        }

        public virtual async Task PrepareTemplatesModel(ManufacturerModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            var templates = await _manufacturerTemplateService.GetAllManufacturerTemplates();
            foreach (var template in templates)
            {
                model.AvailableManufacturerTemplates.Add(new SelectListItem
                {
                    Text = template.Name,
                    Value = template.Id.ToString()
                });
            }
        }


        public virtual async Task PrepareDiscountModel(ManufacturerModel model, Manufacturer manufacturer, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableDiscounts = (await _discountService
                .GetAllDiscounts(DiscountType.AssignedToManufacturers, storeId: _workContext.CurrentCustomer.Id, showHidden: true))
                .Select(d => d.ToModel(_dateTimeHelper))
                .ToList();

            if (!excludeProperties && manufacturer != null)
            {
                model.SelectedDiscountIds = manufacturer.AppliedDiscounts.ToArray();
            }
        }

        public virtual async Task<Manufacturer> InsertManufacturerModel(ManufacturerModel model)
        {
            var manufacturer = model.ToEntity();
            manufacturer.CreatedOnUtc = DateTime.UtcNow;
            manufacturer.UpdatedOnUtc = DateTime.UtcNow;
            //discounts
            var allDiscounts = await _discountService.GetAllDiscounts(DiscountType.AssignedToManufacturers, showHidden: true);
            foreach (var discount in allDiscounts)
            {
                if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                    manufacturer.AppliedDiscounts.Add(discount.Id);
            }

            await _manufacturerService.InsertManufacturer(manufacturer);
            //search engine name
            manufacturer.Locales = await model.Locales.ToLocalizedProperty(manufacturer, x => x.Name, _seoSettings, _urlRecordService, _languageService);
            model.SeName = await manufacturer.ValidateSeName(model.SeName, manufacturer.Name, true, _seoSettings, _urlRecordService, _languageService);
            manufacturer.SeName = model.SeName;
            await _manufacturerService.UpdateManufacturer(manufacturer);

            await _urlRecordService.SaveSlug(manufacturer, model.SeName, "");

            //update picture seo file name
            await _pictureService.UpdatePictureSeoNames(manufacturer.PictureId, manufacturer.Name);

            //activity log
            await _customerActivityService.InsertActivity("AddNewManufacturer", manufacturer.Id, _localizationService.GetResource("ActivityLog.AddNewManufacturer"), manufacturer.Name);
            return manufacturer;
        }

        public virtual async Task<Manufacturer> UpdateManufacturerModel(Manufacturer manufacturer, ManufacturerModel model)
        {
            string prevPictureId = manufacturer.PictureId;
            manufacturer = model.ToEntity(manufacturer);
            manufacturer.UpdatedOnUtc = DateTime.UtcNow;
            manufacturer.Locales = await model.Locales.ToLocalizedProperty(manufacturer, x => x.Name, _seoSettings, _urlRecordService, _languageService);
            //discounts
            var allDiscounts = await _discountService.GetAllDiscounts(DiscountType.AssignedToManufacturers, showHidden: true);
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
            model.SeName = await manufacturer.ValidateSeName(model.SeName, manufacturer.Name, true, _seoSettings, _urlRecordService, _languageService);
            manufacturer.SeName = model.SeName;

            await _manufacturerService.UpdateManufacturer(manufacturer);
            //search engine name
            await _urlRecordService.SaveSlug(manufacturer, model.SeName, "");

            //delete an old picture (if deleted or updated)
            if (!String.IsNullOrEmpty(prevPictureId) && prevPictureId != manufacturer.PictureId)
            {
                var prevPicture = await _pictureService.GetPictureById(prevPictureId);
                if (prevPicture != null)
                    await _pictureService.DeletePicture(prevPicture);
            }
            //update picture seo file name
            await _pictureService.UpdatePictureSeoNames(manufacturer.PictureId, manufacturer.Name);

            //activity log
            await _customerActivityService.InsertActivity("EditManufacturer", manufacturer.Id, _localizationService.GetResource("ActivityLog.EditManufacturer"), manufacturer.Name);
            return manufacturer;
        }

        public virtual async Task DeleteManufacturer(Manufacturer manufacturer)
        {
            await _manufacturerService.DeleteManufacturer(manufacturer);
            //activity log
            await _customerActivityService.InsertActivity("DeleteManufacturer", manufacturer.Id, _localizationService.GetResource("ActivityLog.DeleteManufacturer"), manufacturer.Name);
        }

        public virtual async Task<ManufacturerModel.AddManufacturerProductModel> PrepareAddManufacturerProductModel(string storeId)
        {
            var model = new ManufacturerModel.AddManufacturerProductModel();
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
            foreach (var s in (await _storeService.GetAllStores()).Where(x => x.Id == storeId || string.IsNullOrWhiteSpace(storeId)))
                model.AvailableStores.Add(new SelectListItem { Text = s.Shortcut, Value = s.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var v in await _vendorService.GetAllVendors(showHidden: true))
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(_localizationService, _workContext, false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            return model;
        }

        public virtual async Task<(IList<ProductModel> products, int totalCount)> PrepareProductModel(ManufacturerModel.AddManufacturerProductModel model, int pageIndex, int pageSize)
        {
            var products = await _productService.PrepareProductList(model.SearchCategoryId, model.SearchManufacturerId, model.SearchStoreId, model.SearchVendorId, model.SearchProductTypeId, model.SearchProductName, pageIndex, pageSize);
            return (products.Select(x => x.ToModel(_dateTimeHelper)).ToList(), products.TotalCount);
        }

        public virtual async Task<(IEnumerable<ManufacturerModel.ManufacturerProductModel> manufacturerProductModels, int totalCount)> PrepareManufacturerProductModel(string manufacturerId, string storeId, int pageIndex, int pageSize)
        {
            var productManufacturers = await _manufacturerService.GetProductManufacturersByManufacturerId(manufacturerId, storeId, 
                pageIndex - 1, pageSize, true);
            var items = new List<ManufacturerModel.ManufacturerProductModel>();
            foreach (var x in productManufacturers)
            {
                items.Add(new ManufacturerModel.ManufacturerProductModel
                {
                    Id = x.Id,
                    ManufacturerId = x.ManufacturerId,
                    ProductId = x.ProductId,
                    ProductName = (await _productService.GetProductById(x.ProductId)).Name,
                    IsFeaturedProduct = x.IsFeaturedProduct,
                    DisplayOrder = x.DisplayOrder
                });
            }
            return (items, productManufacturers.TotalCount);
        }
        public virtual async Task ProductUpdate(ManufacturerModel.ManufacturerProductModel model)
        {
            var product = await _productService.GetProductById(model.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var productManufacturer = product.ProductManufacturers.Where(x => x.Id == model.Id).FirstOrDefault();
            if (productManufacturer == null)
                throw new ArgumentException("No product manufacturer mapping found with the specified id");

            productManufacturer.IsFeaturedProduct = model.IsFeaturedProduct;
            productManufacturer.DisplayOrder = model.DisplayOrder;
            productManufacturer.ProductId = model.ProductId;
            await _manufacturerService.UpdateProductManufacturer(productManufacturer);
        }
        public virtual async Task ProductDelete(string id, string productId)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var productManufacturer = product.ProductManufacturers.Where(x => x.Id == id).FirstOrDefault();
            if (productManufacturer == null)
                throw new ArgumentException("No product manufacturer mapping found with the specified id");

            productManufacturer.ProductId = productId;

            await _manufacturerService.DeleteProductManufacturer(productManufacturer);
        }
        public virtual async Task InsertManufacturerProductModel(ManufacturerModel.AddManufacturerProductModel model)
        {
            foreach (string id in model.SelectedProductIds)
            {
                var product = await _productService.GetProductById(id);
                if (product != null)
                {
                    var existingProductmanufacturers = product.ProductManufacturers;
                    if (product.ProductManufacturers.Where(x => x.ManufacturerId == model.ManufacturerId).Count() == 0)
                    {
                        await _manufacturerService.InsertProductManufacturer(
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
        public virtual async Task<(IEnumerable<ManufacturerModel.ActivityLogModel> activityLogModels, int totalCount)> PrepareActivityLogModel(string manufacturerId, int pageIndex, int pageSize)
        {
            var activityLog = await _customerActivityService.GetManufacturerActivities(null, null, manufacturerId, pageIndex - 1, pageSize);
            var items = new List<ManufacturerModel.ActivityLogModel>();
            foreach (var x in activityLog)
            {
                var customer = await _customerService.GetCustomerById(x.CustomerId);
                var m = new ManufacturerModel.ActivityLogModel
                {
                    Id = x.Id,
                    ActivityLogTypeName = (await _customerActivityService.GetActivityTypeById(x.ActivityLogTypeId))?.Name,
                    Comment = x.Comment,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc),
                    CustomerId = x.CustomerId,
                    CustomerEmail = customer != null ? customer.Email : "null"
                };
                items.Add(m);
            }
            return (items, activityLog.TotalCount);
        }
    }
}
