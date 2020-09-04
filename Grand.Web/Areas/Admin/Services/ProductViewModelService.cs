using Grand.Core;
using Grand.Core.Caching;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Directory;
using Grand.Domain.Discounts;
using Grand.Domain.Localization;
using Grand.Domain.Orders;
using Grand.Domain.Seo;
using Grand.Domain.Tax;
using Grand.Framework.Extensions;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Discounts;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Media;
using Grand.Services.Orders;
using Grand.Services.Seo;
using Grand.Services.Shipping;
using Grand.Services.Stores;
using Grand.Services.Tax;
using Grand.Services.Vendors;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Web.Areas.Admin.Models.Orders;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Services
{
    public partial class ProductViewModelService : IProductViewModelService
    {
        private readonly IProductService _productService;
        private readonly IPictureService _pictureService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductTagService _productTagService;
        private readonly ICurrencyService _currencyService;
        private readonly IMeasureService _measureService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IManufacturerService _manufacturerService;
        private readonly ICategoryService _categoryService;
        private readonly IVendorService _vendorService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductTemplateService _productTemplateService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IWorkContext _workContext;
        private readonly IShippingService _shippingService;
        private readonly IShipmentService _shipmentService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly IDiscountService _discountService;
        private readonly ICustomerService _customerService;
        private readonly IStoreService _storeService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IBackInStockSubscriptionService _backInStockSubscriptionService;
        private readonly IDownloadService _downloadService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ILanguageService _languageService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IServiceProvider _serviceProvider;

        private readonly CurrencySettings _currencySettings;
        private readonly MeasureSettings _measureSettings;
        private readonly TaxSettings _taxSettings;

        public ProductViewModelService(
               IProductService productService,
               IPictureService pictureService,
               IProductAttributeService productAttributeService,
               IProductTagService productTagService,
               ICurrencyService currencyService,
               IMeasureService measureService,
               IDateTimeHelper dateTimeHelper,
               IManufacturerService manufacturerService,
               ICategoryService categoryService,
               IVendorService vendorService,
               ILocalizationService localizationService,
               IProductTemplateService productTemplateService,
               ISpecificationAttributeService specificationAttributeService,
               IWorkContext workContext,
               IShippingService shippingService,
               IShipmentService shipmentService,
               ITaxCategoryService taxCategoryService,
               IDiscountService discountService,
               ICustomerService customerService,
               IStoreService storeService,
               IUrlRecordService urlRecordService,
               ICustomerActivityService customerActivityService,
               IBackInStockSubscriptionService backInStockSubscriptionService,
               IDownloadService downloadService,
               IProductAttributeParser productAttributeParser,
               ILanguageService languageService,
               IProductAttributeFormatter productAttributeFormatter,
               IServiceProvider serviceProvider,
               CurrencySettings currencySettings,
               MeasureSettings measureSettings,
               TaxSettings taxSettings)
        {
            _productService = productService;
            _pictureService = pictureService;
            _productAttributeService = productAttributeService;
            _productTagService = productTagService;
            _currencyService = currencyService;
            _measureService = measureService;
            _dateTimeHelper = dateTimeHelper;
            _manufacturerService = manufacturerService;
            _categoryService = categoryService;
            _vendorService = vendorService;
            _localizationService = localizationService;
            _productTemplateService = productTemplateService;
            _specificationAttributeService = specificationAttributeService;
            _workContext = workContext;
            _shippingService = shippingService;
            _shipmentService = shipmentService;
            _taxCategoryService = taxCategoryService;
            _discountService = discountService;
            _customerService = customerService;
            _storeService = storeService;
            _urlRecordService = urlRecordService;
            _customerActivityService = customerActivityService;
            _backInStockSubscriptionService = backInStockSubscriptionService;
            _downloadService = downloadService;
            _productAttributeParser = productAttributeParser;
            _languageService = languageService;
            _productAttributeFormatter = productAttributeFormatter;
            _serviceProvider = serviceProvider;
            _currencySettings = currencySettings;
            _measureSettings = measureSettings;
            _taxSettings = taxSettings;
        }
        protected virtual async Task UpdatePictureSeoNames(Product product)
        {
            foreach (var pp in product.ProductPictures)
                await _pictureService.SetSeoFilename(pp.PictureId, _pictureService.GetPictureSeName(product.Name));
        }
        protected virtual async Task<List<string>> GetChildCategoryIds(string parentCategoryId)
        {
            var categoriesIds = new List<string>();
            var categories = await _categoryService.GetAllCategoriesByParentCategoryId(parentCategoryId, true);
            foreach (var category in categories)
            {
                categoriesIds.Add(category.Id);
                categoriesIds.AddRange(await GetChildCategoryIds(category.Id));
            }
            return categoriesIds;
        }
        protected virtual string[] ParseProductTags(string productTags)
        {
            var result = new List<string>();
            if (!string.IsNullOrWhiteSpace(productTags))
            {
                var values = productTags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var val1 in values)
                    if (!string.IsNullOrEmpty(val1.Trim()))
                        result.Add(val1.Trim());
            }
            return result.ToArray();
        }
        protected virtual async Task SaveProductTags(Product product, string[] productTags)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            //product tags
            var existingProductTags = product.ProductTags.ToList();
            var productTagsToRemove = new List<ProductTag>();
            foreach (var existingProductTag in existingProductTags)
            {
                var existingProductTagText = await _productTagService.GetProductTagByName(existingProductTag.ToLowerInvariant());
                var found = false;
                foreach (var newProductTag in productTags)
                {
                    if (existingProductTagText != null)
                        if (existingProductTagText.Name.Equals(newProductTag, StringComparison.OrdinalIgnoreCase))
                        {
                            found = true;
                            break;
                        }
                }
                if (!found)
                {
                    productTagsToRemove.Add(existingProductTagText);
                }
            }
            foreach (var productTag in productTagsToRemove)
            {
                if (productTag != null)
                {
                    productTag.ProductId = product.Id;
                    await _productTagService.DetachProductTag(productTag);
                }
            }
            foreach (var productTagName in productTags)
            {
                ProductTag productTag;
                var productTag2 = await _productTagService.GetProductTagByName(productTagName);
                if (productTag2 == null)
                {
                    //add new product tag
                    productTag = new ProductTag {
                        Name = productTagName,
                        SeName = SeoExtensions.GetSeName(productTagName, _serviceProvider.GetRequiredService<SeoSettings>()),
                        Count = 0,
                    };
                    await _productTagService.InsertProductTag(productTag);
                }
                else
                {
                    productTag = productTag2;
                }
                if (!product.ProductTagExists(productTag.Name))
                {
                    productTag.ProductId = product.Id;
                    await _productTagService.AttachProductTag(productTag);
                }
            }
        }
        public virtual async Task PrepareAddProductAttributeCombinationModel(ProductAttributeCombinationModel model, Product product)
        {
            if (model == null)
                throw new ArgumentNullException("model");
            if (product == null)
                throw new ArgumentNullException("product");
            if (product.UseMultipleWarehouses)
            {
                model.UseMultipleWarehouses = product.UseMultipleWarehouses;
            }
            if (string.IsNullOrEmpty(model.Id))
            {
                model.ProductId = product.Id;
                var attributes = product.ProductAttributeMappings
                    .Where(x => !x.IsNonCombinable())
                    .ToList();
                foreach (var attribute in attributes)
                {
                    var productAttribute = await _productAttributeService.GetProductAttributeById(attribute.ProductAttributeId);
                    var attributeModel = new ProductAttributeCombinationModel.ProductAttributeModel {
                        Id = attribute.Id,
                        ProductAttributeId = attribute.ProductAttributeId,
                        Name = productAttribute.Name,
                        TextPrompt = attribute.TextPrompt,
                        IsRequired = attribute.IsRequired,
                        AttributeControlType = attribute.AttributeControlType
                    };

                    if (attribute.ShouldHaveValues())
                    {
                        //values
                        var attributeValues = attribute.ProductAttributeValues;
                        foreach (var attributeValue in attributeValues)
                        {
                            var attributeValueModel = new ProductAttributeCombinationModel.ProductAttributeValueModel {
                                Id = attributeValue.Id,
                                Name = attributeValue.Name,
                                IsPreSelected = attributeValue.IsPreSelected,
                            };
                            attributeModel.Values.Add(attributeValueModel);
                        }
                    }

                    model.ProductAttributes.Add(attributeModel);
                }
            }


            if (!string.IsNullOrEmpty(model.PictureId))
            {
                var pictureThumbnailUrl = await _pictureService.GetPictureUrl(model.PictureId, 75, false);
                model.PictureThumbnailUrl = pictureThumbnailUrl;
            }
            foreach (var picture in product.ProductPictures)
            {
                model.ProductPictureModels.Add(new ProductModel.ProductPictureModel {
                    Id = picture.Id,
                    ProductId = product.Id,
                    PictureId = picture.PictureId,
                    PictureUrl = await _pictureService.GetPictureUrl(picture.PictureId),
                    DisplayOrder = picture.DisplayOrder
                });
            }
        }

        public virtual async Task PrepareTierPriceModel(ProductModel.TierPriceModel model)
        {
            var storeId = string.Empty;
            if (_workContext.CurrentCustomer.IsStaff())
                storeId = _workContext.CurrentCustomer.StaffStoreId;

            if (string.IsNullOrEmpty(storeId))
                model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });

            foreach (var store in (await _storeService.GetAllStores()).Where(x => x.Id == storeId || string.IsNullOrWhiteSpace(storeId)))
                model.AvailableStores.Add(new SelectListItem { Text = store.Shortcut, Value = store.Id.ToString() });

            //customer roles
            model.AvailableCustomerRoles.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var role in await _customerService.GetAllCustomerRoles(showHidden: true))
                model.AvailableCustomerRoles.Add(new SelectListItem { Text = role.Name, Value = role.Id.ToString() });
        }
        public virtual async Task PrepareProductAttributeValueModel(Product product, ProductModel.ProductAttributeValueModel model)
        {
            //pictures
            foreach (var x in product.ProductPictures.OrderBy(x => x.DisplayOrder))
            {
                model.ProductPictureModels.Add(new ProductModel.ProductPictureModel {
                    Id = x.Id,
                    ProductId = product.Id,
                    PictureId = x.PictureId,
                    PictureUrl = await _pictureService.GetPictureUrl(x.PictureId),
                    DisplayOrder = x.DisplayOrder
                });
            }

            var associatedProduct = await _productService.GetProductById(model.AssociatedProductId);
            model.AssociatedProductName = associatedProduct != null ? associatedProduct.Name : "";
        }
        public virtual async Task BackInStockNotifications(Product product, ProductModel model, int prevStockQuantity,
            List<ProductWarehouseInventory> prevMultiWarehouseStock
            )
        {
            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                product.BackorderMode == BackorderMode.NoBackorders &&
                product.AllowBackInStockSubscriptions &&
                product.GetTotalStockQuantity() > 0 &&
                prevStockQuantity <= 0 && !product.UseMultipleWarehouses &&
                product.Published)
            {
                await _backInStockSubscriptionService.SendNotificationsToSubscribers(product, "");
            }
            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                product.BackorderMode == BackorderMode.NoBackorders &&
                product.AllowBackInStockSubscriptions &&
                product.UseMultipleWarehouses &&
                product.Published)
            {
                foreach (var prevstock in prevMultiWarehouseStock)
                {
                    if (prevstock.StockQuantity - prevstock.ReservedQuantity <= 0)
                    {
                        var actualStock = product.ProductWarehouseInventory.FirstOrDefault(x => x.WarehouseId == prevstock.WarehouseId);
                        if (actualStock != null)
                        {
                            if (actualStock.StockQuantity - actualStock.ReservedQuantity > 0)
                                await _backInStockSubscriptionService.SendNotificationsToSubscribers(product, prevstock.WarehouseId);
                        }
                    }
                }
                if (product.ProductWarehouseInventory.Sum(x => x.StockQuantity - x.ReservedQuantity) > 0)
                {
                    if (prevMultiWarehouseStock.Sum(x => x.StockQuantity - x.ReservedQuantity) <= 0)
                    {
                        await _backInStockSubscriptionService.SendNotificationsToSubscribers(product, "");
                    }
                }
            }
        }
        public virtual async Task BackInStockNotifications(ProductAttributeCombination combination)
        {
            var product = await _productService.GetProductById(combination.ProductId, true);
            var prevcombination = product.ProductAttributeCombinations.FirstOrDefault(x => x.Id == combination.Id);

            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes &&
                product.BackorderMode == BackorderMode.NoBackorders &&
                product.AllowBackInStockSubscriptions &&
                combination.StockQuantity > 0 &&
                prevcombination.StockQuantity <= 0 && !product.UseMultipleWarehouses &&
                product.Published)
            {
                await _backInStockSubscriptionService.SendNotificationsToSubscribers(product, combination.AttributesXml, "");
            }
            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes &&
                product.BackorderMode == BackorderMode.NoBackorders &&
                product.AllowBackInStockSubscriptions &&
                product.UseMultipleWarehouses &&
                product.Published)
            {
                foreach (var prevstock in prevcombination.WarehouseInventory)
                {
                    if (prevstock.StockQuantity - prevstock.ReservedQuantity <= 0)
                    {
                        var actualStock = combination.WarehouseInventory.FirstOrDefault(x => x.WarehouseId == prevstock.WarehouseId);
                        if (actualStock != null)
                        {
                            if (actualStock.StockQuantity - actualStock.ReservedQuantity > 0)
                                await _backInStockSubscriptionService.SendNotificationsToSubscribers(product, combination.AttributesXml, prevstock.WarehouseId);
                        }
                    }
                }
                if (combination.WarehouseInventory.Sum(x => x.StockQuantity - x.ReservedQuantity) > 0)
                {
                    if (prevcombination.WarehouseInventory.Sum(x => x.StockQuantity - x.ReservedQuantity) <= 0)
                    {
                        await _backInStockSubscriptionService.SendNotificationsToSubscribers(product, combination.AttributesXml, "");
                    }
                }
            }
        }
        public virtual async Task PrepareProductModel(ProductModel model, Product product,
            bool setPredefinedValues, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.PrimaryStoreCurrencyCode = (await _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId))?.CurrencyCode;
            model.BaseWeightIn = (await _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId))?.Name;
            model.BaseDimensionIn = (await _measureService.GetMeasureDimensionById(_measureSettings.BaseDimensionId))?.Name;

            var storeId = string.Empty;
            if (_workContext.CurrentCustomer.IsStaff())
                storeId = _workContext.CurrentCustomer.StaffStoreId;

            if (product != null)
            {
                //date
                model.CreatedOn = _dateTimeHelper.ConvertToUserTime(product.CreatedOnUtc, DateTimeKind.Utc);
                model.UpdatedOn = _dateTimeHelper.ConvertToUserTime(product.UpdatedOnUtc, DateTimeKind.Utc);

                //parent grouped product
                var parentGroupedProduct = await _productService.GetProductById(product.ParentGroupedProductId);
                if (parentGroupedProduct != null)
                {
                    model.AssociatedToProductId = product.ParentGroupedProductId;
                    model.AssociatedToProductName = parentGroupedProduct.Name;
                }

                //reservation
                model.CalendarModel.ProductId = product.Id;
                model.CalendarModel.Interval = product.Interval;
                model.CalendarModel.IntervalUnit = product.IntervalUnitId;
                model.CalendarModel.IncBothDate = product.IncBothDate;

                //product attributes
                foreach (var productAttribute in await _productAttributeService.GetAllProductAttributes())
                {
                    model.AvailableProductAttributes.Add(new SelectListItem {
                        Text = productAttribute.Name,
                        Value = productAttribute.Id.ToString()
                    });
                }

                //manufacturers
                foreach (var manufacturer in await _manufacturerService.GetAllManufacturers(showHidden: true, storeId: storeId))
                {
                    model.AvailableManufacturers.Add(new SelectListItem {
                        Text = manufacturer.Name,
                        Value = manufacturer.Id.ToString()
                    });
                }
                //categories
                var allCategories = await _categoryService.GetAllCategories(showHidden: true, storeId: storeId);
                foreach (var category in allCategories)
                {
                    model.AvailableCategories.Add(new SelectListItem {
                        Text = _categoryService.GetFormattedBreadCrumb(category, allCategories),
                        Value = category.Id.ToString()
                    });
                }

                //specification attributes
                var availableSpecificationAttributes = new List<SelectListItem>();
                foreach (var sa in await _specificationAttributeService.GetSpecificationAttributes())
                {
                    availableSpecificationAttributes.Add(new SelectListItem {
                        Text = sa.Name,
                        Value = sa.Id.ToString()
                    });
                }
                model.AddSpecificationAttributeModel.AvailableAttributes = availableSpecificationAttributes;

                //default specs values
                model.AddSpecificationAttributeModel.ShowOnProductPage = true;

            }

            //copy product
            if (product != null)
            {
                model.CopyProductModel.Id = product.Id;
                model.CopyProductModel.Name = "Copy of " + product.Name;
                model.CopyProductModel.Published = true;
                model.CopyProductModel.CopyImages = true;
            }

            //templates
            var templates = await _productTemplateService.GetAllProductTemplates();
            foreach (var template in templates)
            {
                model.AvailableProductTemplates.Add(new SelectListItem {
                    Text = template.Name,
                    Value = template.Id.ToString()
                });
            }

            //vendors
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            model.AvailableVendors.Add(new SelectListItem {
                Text = _localizationService.GetResource("Admin.Catalog.Products.Fields.Vendor.None"),
                Value = ""
            });
            var vendors = await _vendorService.GetAllVendors(showHidden: true);
            foreach (var vendor in vendors)
            {
                model.AvailableVendors.Add(new SelectListItem {
                    Text = vendor.Name,
                    Value = vendor.Id.ToString()
                });
            }

            //delivery dates
            model.AvailableDeliveryDates.Add(new SelectListItem {
                Text = _localizationService.GetResource("Admin.Catalog.Products.Fields.DeliveryDate.None"),
                Value = ""
            });
            var deliveryDates = await _shippingService.GetAllDeliveryDates();
            foreach (var deliveryDate in deliveryDates)
            {
                model.AvailableDeliveryDates.Add(new SelectListItem {
                    Text = deliveryDate.Name,
                    Value = deliveryDate.Id.ToString()
                });
            }

            //warehouses
            var warehouses = await _shippingService.GetAllWarehouses();
            model.AvailableWarehouses.Add(new SelectListItem {
                Text = _localizationService.GetResource("Admin.Catalog.Products.Fields.Warehouse.None"),
                Value = ""
            });
            foreach (var warehouse in warehouses)
            {
                model.AvailableWarehouses.Add(new SelectListItem {
                    Text = warehouse.Name,
                    Value = warehouse.Id.ToString()
                });
            }

            //multiple warehouses
            foreach (var warehouse in warehouses)
            {
                var pwiModel = new ProductModel.ProductWarehouseInventoryModel {
                    WarehouseId = warehouse.Id,
                    WarehouseName = warehouse.Name
                };
                if (product != null)
                {
                    var pwi = product.ProductWarehouseInventory.FirstOrDefault(x => x.WarehouseId == warehouse.Id);
                    if (pwi != null)
                    {
                        pwiModel.WarehouseUsed = true;
                        pwiModel.StockQuantity = pwi.StockQuantity;
                        pwiModel.ReservedQuantity = pwi.ReservedQuantity;
                        pwiModel.PlannedQuantity = await _shipmentService.GetQuantityInShipments(product, null, pwi.WarehouseId, true, true);
                    }
                }
                model.ProductWarehouseInventoryModels.Add(pwiModel);
            }

            //product tags
            if (product != null)
            {
                var result = new StringBuilder();
                for (var i = 0; i < product.ProductTags.Count; i++)
                {
                    var pt = product.ProductTags.ToList()[i];
                    var productTag = await _productTagService.GetProductTagByName(pt);
                    if (productTag != null)
                    {
                        result.Append(productTag.Name);
                        if (i != product.ProductTags.Count - 1)
                            result.Append(", ");
                    }
                }
                model.ProductTags = result.ToString();
            }

            //tax categories
            var taxCategories = await _taxCategoryService.GetAllTaxCategories();
            model.AvailableTaxCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Configuration.Settings.Tax.TaxCategories.None"), Value = "" });
            foreach (var tc in taxCategories)
                model.AvailableTaxCategories.Add(new SelectListItem { Text = tc.Name, Value = tc.Id.ToString(), Selected = product != null && !setPredefinedValues && tc.Id == product.TaxCategoryId });

            //baseprice units
            var measureWeights = await _measureService.GetAllMeasureWeights();
            foreach (var mw in measureWeights)
                model.AvailableBasepriceUnits.Add(new SelectListItem { Text = mw.Name, Value = mw.Id.ToString(), Selected = product != null && !setPredefinedValues && mw.Id == product.BasepriceUnitId });
            foreach (var mw in measureWeights)
                model.AvailableBasepriceBaseUnits.Add(new SelectListItem { Text = mw.Name, Value = mw.Id.ToString(), Selected = product != null && !setPredefinedValues && mw.Id == product.BasepriceBaseUnitId });

            //units
            var units = await _measureService.GetAllMeasureUnits();
            model.AvailableUnits.Add(new SelectListItem { Text = "---", Value = "" });
            foreach (var un in units)
                model.AvailableUnits.Add(new SelectListItem { Text = un.Name, Value = un.Id.ToString(), Selected = product != null && un.Id == product.UnitId });

            //default specs values
            model.AddSpecificationAttributeModel.ShowOnProductPage = true;

            //discounts
            model.AvailableDiscounts = (await _discountService
                .GetAllDiscounts(DiscountType.AssignedToSkus, storeId: _workContext.CurrentCustomer.StaffStoreId, showHidden: true))
                .Select(d => d.ToModel(_dateTimeHelper))
                .ToList();
            if (!excludeProperties && product != null)
            {
                model.SelectedDiscountIds = product.AppliedDiscounts.ToArray();
            }

            //default values
            if (setPredefinedValues)
            {
                model.MaximumCustomerEnteredPrice = 1000;
                model.MaxNumberOfDownloads = 10;
                model.RecurringCycleLength = 100;
                model.RecurringTotalCycles = 10;
                model.StockQuantity = 0;
                model.NotifyAdminForQuantityBelow = 1;
                model.OrderMinimumQuantity = 1;
                model.OrderMaximumQuantity = 10000;
                model.TaxCategoryId = _taxSettings.DefaultTaxCategoryId;
                model.UnlimitedDownloads = true;
                model.IsShipEnabled = true;
                model.AllowCustomerReviews = true;
                model.Published = true;
                model.VisibleIndividually = true;
            }
            if (_workContext.CurrentVendor != null && !string.IsNullOrEmpty(_workContext.CurrentVendor.StoreId))
            {
                model.LimitedToStores = true;
                model.SelectedStoreIds = new string[] { _workContext.CurrentVendor.StoreId };
            }
        }
        public virtual async Task<(IEnumerable<OrderModel> orderModels, int totalCount)> PrepareOrderModel(string productId, int pageIndex, int pageSize)
        {
            var storeId = string.Empty;
            if (_workContext.CurrentCustomer.IsStaff())
                storeId = _workContext.CurrentCustomer.StaffStoreId;

            var orderService = _serviceProvider.GetRequiredService<IOrderService>();
            var orders = await orderService.SearchOrders(
                            storeId: storeId,
                            productId: productId,
                            pageIndex: pageIndex - 1,
                            pageSize: pageSize);

            var items = new List<OrderModel>();
            foreach (var x in orders)
            {
                var store = await _storeService.GetStoreById(x.StoreId);
                items.Add(new OrderModel {
                    Id = x.Id,
                    OrderNumber = x.OrderNumber,
                    Code = x.Code,
                    StoreName = store != null ? store.Shortcut : "Unknown",
                    OrderStatus = x.OrderStatus.GetLocalizedEnum(_localizationService, _workContext),
                    PaymentStatus = x.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext),
                    ShippingStatus = x.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext),
                    CustomerEmail = x.BillingAddress?.Email,
                    CustomerId = x.CustomerId,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc)
                });
            }
            return (items, orders.TotalCount);
        }
        public virtual async Task SaveProductWarehouseInventory(Product product, IList<ProductModel.ProductWarehouseInventoryModel> model)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (product.ManageInventoryMethodId != (int)ManageInventoryMethod.ManageStock)
                return;

            if (!product.UseMultipleWarehouses)
                return;

            var warehouses = await _shippingService.GetAllWarehouses();

            foreach (var warehouse in warehouses)
            {
                var whim = model.FirstOrDefault(x => x.WarehouseId == warehouse.Id);
                var existingPwI = product.ProductWarehouseInventory.FirstOrDefault(x => x.WarehouseId == warehouse.Id);
                if (existingPwI != null)
                {
                    existingPwI.ProductId = product.Id;
                    if (whim.WarehouseUsed)
                    {
                        //update existing record
                        existingPwI.StockQuantity = whim.StockQuantity;
                        existingPwI.ReservedQuantity = whim.ReservedQuantity;
                        await _productService.UpdateProductWarehouseInventory(existingPwI);
                    }
                    else
                    {
                        //delete. no need to store record for qty 0
                        await _productService.DeleteProductWarehouseInventory(existingPwI);
                    }
                }
                else
                {
                    if (whim.WarehouseUsed)
                    {
                        //no need to insert a record for qty 0
                        existingPwI = new ProductWarehouseInventory {
                            WarehouseId = warehouse.Id,
                            ProductId = product.Id,
                            StockQuantity = whim.StockQuantity,
                            ReservedQuantity = whim.ReservedQuantity
                        };
                        product.ProductWarehouseInventory.Add(existingPwI);
                        await _productService.InsertProductWarehouseInventory(existingPwI);
                    }
                }

            }
            product.StockQuantity = product.ProductWarehouseInventory.Sum(x => x.StockQuantity);
            await _productService.UpdateStockProduct(product, false);

        }
        public virtual async Task PrepareProductReviewModel(ProductReviewModel model,
            ProductReview productReview, bool excludeProperties, bool formatReviewText)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (productReview == null)
                throw new ArgumentNullException("productReview");
            var product = await _productService.GetProductById(productReview.ProductId);
            var customer = await _customerService.GetCustomerById(productReview.CustomerId);
            var store = await _storeService.GetStoreById(productReview.StoreId);
            model.Id = productReview.Id;
            model.StoreName = store != null ? store.Shortcut : "";
            model.ProductId = productReview.ProductId;
            model.ProductName = product.Name;
            model.CustomerId = productReview.CustomerId;
            model.CustomerInfo = customer != null ? customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest") : "";
            model.Rating = productReview.Rating;
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(productReview.CreatedOnUtc, DateTimeKind.Utc);
            model.Signature = productReview.Signature;
            if (!excludeProperties)
            {
                model.Title = productReview.Title;
                if (formatReviewText)
                {
                    model.ReviewText = Core.Html.HtmlHelper.FormatText(productReview.ReviewText, false, true, false, false, false, false);
                    model.ReplyText = Core.Html.HtmlHelper.FormatText(productReview.ReplyText, false, true, false, false, false, false);
                }
                else
                {
                    model.ReviewText = productReview.ReviewText;
                    model.ReplyText = productReview.ReplyText;
                }
                model.IsApproved = productReview.IsApproved;
            }
        }
        public virtual async Task<ProductListModel> PrepareProductListModel()
        {
            var model = new ProductListModel {
                //a vendor should have access only to his products
                IsLoggedInAsVendor = _workContext.CurrentVendor != null
            };

            var storeId = string.Empty;

            if (_workContext.CurrentCustomer.IsStaff())
                storeId = _workContext.CurrentCustomer.StaffStoreId;

            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            var categories = await _categoryService.GetAllCategories(showHidden: true, storeId: storeId);
            foreach (var c in categories)
                model.AvailableCategories.Add(new SelectListItem { Text = _categoryService.GetFormattedBreadCrumb(c, categories), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var m in await _manufacturerService.GetAllManufacturers(showHidden: true, storeId: storeId))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var s in (await _storeService.GetAllStores()).Where(x => x.Id == storeId || string.IsNullOrWhiteSpace(storeId)))
            {
                if (_workContext.CurrentVendor != null && !string.IsNullOrEmpty(_workContext.CurrentVendor.StoreId))
                {
                    if (s.Id == _workContext.CurrentVendor.StoreId)
                        model.AvailableStores.Add(new SelectListItem { Text = s.Shortcut, Value = s.Id.ToString() });
                }
                else
                    model.AvailableStores.Add(new SelectListItem { Text = s.Shortcut, Value = s.Id.ToString() });
            }
            //warehouses
            model.AvailableWarehouses.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var wh in await _shippingService.GetAllWarehouses())
                model.AvailableWarehouses.Add(new SelectListItem { Text = wh.Name, Value = wh.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var v in await _vendorService.GetAllVendors(showHidden: true))
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(_localizationService, _workContext, false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            //"published" property
            //0 - all (according to "ShowHidden" parameter)
            //1 - published only
            //2 - unpublished only
            model.AvailablePublishedOptions.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Products.List.SearchPublished.All"), Value = " " });
            model.AvailablePublishedOptions.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Products.List.SearchPublished.PublishedOnly"), Value = "1" });
            model.AvailablePublishedOptions.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Products.List.SearchPublished.UnpublishedOnly"), Value = "2" });

            return model;
        }
        public virtual async Task<(IEnumerable<ProductModel> productModels, int totalCount)> PrepareProductsModel(ProductListModel model, int pageIndex, int pageSize)
        {
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }
            //limit for store manager
            if (_workContext.CurrentCustomer.IsStaff())
                model.SearchStoreId = _workContext.CurrentCustomer.StaffStoreId;

            var categoryIds = new List<string>();
            if (!string.IsNullOrEmpty(model.SearchCategoryId))
                categoryIds.Add(model.SearchCategoryId);

            //include subcategories
            if (model.SearchIncludeSubCategories && !string.IsNullOrEmpty(model.SearchCategoryId))
                categoryIds.AddRange(await GetChildCategoryIds(model.SearchCategoryId));

            //0 - all (according to "ShowHidden" parameter)
            //1 - published only
            //2 - unpublished only
            bool? overridePublished = null;
            if (model.SearchPublishedId == 1)
                overridePublished = true;
            else if (model.SearchPublishedId == 2)
                overridePublished = false;

            var products = (await _productService.SearchProducts(
                categoryIds: categoryIds,
                manufacturerId: model.SearchManufacturerId,
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                warehouseId: model.SearchWarehouseId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                pageIndex: pageIndex - 1,
                pageSize: pageSize,
                showHidden: true,
                overridePublished: overridePublished
            )).products;

            var items = new List<ProductModel>();
            foreach (var x in products)
            {
                var productModel = x.ToModel(_dateTimeHelper);
                //"Error during serialization or deserialization using the JSON JavaScriptSerializer. The length of the string exceeds the value set on the maxJsonLength property. "
                //also it improves performance
                productModel.FullDescription = "";

                //picture
                var defaultProductPicture = x.ProductPictures.FirstOrDefault();
                if (defaultProductPicture == null)
                    defaultProductPicture = new ProductPicture();
                productModel.PictureThumbnailUrl = await _pictureService.GetPictureUrl(defaultProductPicture.PictureId, 75, true);
                //product type
                productModel.ProductTypeName = x.ProductType.GetLocalizedEnum(_localizationService, _workContext);
                //friendly stock qantity
                //if a simple product AND "manage inventory" is "Track inventory", then display
                if (x.ProductType == ProductType.SimpleProduct && x.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                    productModel.StockQuantityStr = x.GetTotalStockQuantity().ToString();
                items.Add(productModel);
            }
            return (items, products.TotalCount);
        }
        public virtual async Task<IList<Product>> PrepareProducts(ProductListModel model)
        {
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }
            //limit for store manager
            if (_workContext.CurrentCustomer.IsStaff())
                model.SearchStoreId = _workContext.CurrentCustomer.StaffStoreId;

            var categoryIds = new List<string>();
            if (!string.IsNullOrEmpty(model.SearchCategoryId))
                categoryIds.Add(model.SearchCategoryId);

            //include subcategories
            if (model.SearchIncludeSubCategories && !string.IsNullOrEmpty(model.SearchCategoryId))
                categoryIds.AddRange(await GetChildCategoryIds(model.SearchCategoryId));

            //0 - all (according to "ShowHidden" parameter)
            //1 - published only
            //2 - unpublished only
            bool? overridePublished = null;
            if (model.SearchPublishedId == 1)
                overridePublished = true;
            else if (model.SearchPublishedId == 2)
                overridePublished = false;

            var products = (await _productService.SearchProducts(
                categoryIds: categoryIds,
                manufacturerId: model.SearchManufacturerId,
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                warehouseId: model.SearchWarehouseId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                showHidden: true,
                overridePublished: overridePublished
            )).products;

            return products;
        }
        public virtual async Task<Product> InsertProductModel(ProductModel model)
        {
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.VendorId = _workContext.CurrentVendor.Id;
                if (!string.IsNullOrEmpty(_workContext.CurrentVendor.StoreId))
                {
                    model.LimitedToStores = true;
                    model.SelectedStoreIds = new string[] { _workContext.CurrentVendor.StoreId };
                }
            }
            //a staff should have access only to his products
            if (_workContext.CurrentCustomer.IsStaff())
            {
                model.LimitedToStores = true;
                model.SelectedStoreIds = new string[] { _workContext.CurrentCustomer.StaffStoreId };
            }

            //vendors cannot edit "Show on home page" property
            if (_workContext.CurrentVendor != null && model.ShowOnHomePage)
            {
                model.ShowOnHomePage = false;
            }

            //product
            var product = model.ToEntity(_dateTimeHelper);
            product.CreatedOnUtc = DateTime.UtcNow;
            product.UpdatedOnUtc = DateTime.UtcNow;

            await _productService.InsertProduct(product);

            model.SeName = await product.ValidateSeName(model.SeName, product.Name, true, _serviceProvider.GetRequiredService<SeoSettings>(), _urlRecordService, _languageService);
            product.SeName = model.SeName;
            product.Locales = await model.Locales.ToLocalizedProperty(product, x => x.Name, _serviceProvider.GetRequiredService<SeoSettings>(), _urlRecordService, _languageService);

            //search engine name
            await _urlRecordService.SaveSlug(product, model.SeName, "");
            //tags
            await SaveProductTags(product, ParseProductTags(model.ProductTags));
            //warehouses
            await SaveProductWarehouseInventory(product, model.ProductWarehouseInventoryModels);
            //discounts
            var allDiscounts = await _discountService.GetAllDiscounts(DiscountType.AssignedToSkus, storeId: _workContext.CurrentCustomer.StaffStoreId, showHidden: true);
            foreach (var discount in allDiscounts)
            {
                if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                {
                    product.AppliedDiscounts.Add(discount.Id);
                    await _productService.InsertDiscount(discount.Id, product.Id);
                }
            }
            await _productService.UpdateProduct(product);

            //activity log
            await _customerActivityService.InsertActivity("AddNewProduct", product.Id, _localizationService.GetResource("ActivityLog.AddNewProduct"), product.Name);

            return product;
        }
        public virtual async Task<Product> UpdateProductModel(Product product, ProductModel model)
        {
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.VendorId = _workContext.CurrentVendor.Id;
                if (!string.IsNullOrEmpty(_workContext.CurrentVendor.StoreId))
                {
                    model.LimitedToStores = true;
                    model.SelectedStoreIds = new string[] { _workContext.CurrentVendor.StoreId };
                }
            }
            //vendors cannot edit "Show on home page" property
            if (_workContext.CurrentVendor != null && model.ShowOnHomePage != product.ShowOnHomePage)
            {
                model.ShowOnHomePage = product.ShowOnHomePage;
            }

            //a staff should have access only to his products
            if (_workContext.CurrentCustomer.IsStaff())
            {
                model.LimitedToStores = true;
                model.SelectedStoreIds = new string[] { _workContext.CurrentCustomer.StaffStoreId };
            }

            var prevStockQuantity = product.GetTotalStockQuantity();
            var prevMultiWarehouseStock = product.ProductWarehouseInventory.Select(i => new ProductWarehouseInventory() { WarehouseId = i.WarehouseId, StockQuantity = i.StockQuantity, ReservedQuantity = i.ReservedQuantity }).ToList();

            var prevDownloadId = product.DownloadId;
            var prevSampleDownloadId = product.SampleDownloadId;

            //product
            product = model.ToEntity(product, _dateTimeHelper);
            product.UpdatedOnUtc = DateTime.UtcNow;

            model.SeName = await product.ValidateSeName(model.SeName, product.Name, true, _serviceProvider.GetRequiredService<SeoSettings>(), _urlRecordService, _languageService);
            product.SeName = model.SeName;
            product.Locales = await model.Locales.ToLocalizedProperty(product, x => x.Name, _serviceProvider.GetRequiredService<SeoSettings>(), _urlRecordService, _languageService);

            //search engine name
            await _urlRecordService.SaveSlug(product, model.SeName, "");
            //tags
            await SaveProductTags(product, ParseProductTags(model.ProductTags));
            //warehouses
            await SaveProductWarehouseInventory(product, model.ProductWarehouseInventoryModels);
            //picture seo names
            await UpdatePictureSeoNames(product);
            //discounts
            var allDiscounts = await _discountService.GetAllDiscounts(DiscountType.AssignedToSkus, storeId: _workContext.CurrentCustomer.StaffStoreId, showHidden: true);
            foreach (var discount in allDiscounts)
            {
                if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                {
                    //new discount
                    if (product.AppliedDiscounts.Count(d => d == discount.Id) == 0)
                    {
                        product.AppliedDiscounts.Add(discount.Id);
                        await _productService.InsertDiscount(discount.Id, product.Id);
                    }
                }
                else
                {
                    //remove discount
                    if (product.AppliedDiscounts.Count(d => d == discount.Id) > 0)
                    {
                        product.AppliedDiscounts.Remove(discount.Id);
                        await _productService.DeleteDiscount(discount.Id, product.Id);
                    }
                }
            }
            await _productService.UpdateProduct(product);

            //back in stock notifications
            await BackInStockNotifications(product, model, prevStockQuantity, prevMultiWarehouseStock);

            //delete an old "download" file (if deleted or updated)
            if (!string.IsNullOrEmpty(prevDownloadId) && prevDownloadId != product.DownloadId)
            {
                var prevDownload = await _downloadService.GetDownloadById(prevDownloadId);
                if (prevDownload != null)
                    await _downloadService.DeleteDownload(prevDownload);
            }
            //delete an old "sample download" file (if deleted or updated)
            if (!string.IsNullOrEmpty(prevSampleDownloadId) && prevSampleDownloadId != product.SampleDownloadId)
            {
                var prevSampleDownload = await _downloadService.GetDownloadById(prevSampleDownloadId);
                if (prevSampleDownload != null)
                    await _downloadService.DeleteDownload(prevSampleDownload);
            }
            //activity log
            await _customerActivityService.InsertActivity("EditProduct", product.Id, _localizationService.GetResource("ActivityLog.EditProduct"), product.Name);
            return product;
        }
        public virtual async Task DeleteProduct(Product product)
        {
            await _productService.DeleteProduct(product);
            //activity log
            await _customerActivityService.InsertActivity("DeleteProduct", product.Id, _localizationService.GetResource("ActivityLog.DeleteProduct"), product.Name);
        }
        public virtual async Task DeleteSelected(IList<string> selectedIds)
        {
            var products = new List<Product>();
            products.AddRange(await _productService.GetProductsByIds(selectedIds.ToArray(), true));
            for (var i = 0; i < products.Count; i++)
            {
                var product = products[i];
                //a vendor should have access only to his products
                if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                    continue;

                if (_workContext.CurrentCustomer.IsStaff())
                {
                    if (!(product.LimitedToStores && product.Stores.Contains(_workContext.CurrentCustomer.StaffStoreId) && product.Stores.Count == 1))
                        continue;
                }

                await _productService.DeleteProduct(product);
            }
        }
        public virtual async Task<ProductModel.AddRequiredProductModel> PrepareAddRequiredProductModel()
        {
            var model = new ProductModel.AddRequiredProductModel {
                //a vendor should have access only to his products
                IsLoggedInAsVendor = _workContext.CurrentVendor != null
            };

            var storeId = string.Empty;
            if (_workContext.CurrentCustomer.IsStaff())
                storeId = _workContext.CurrentCustomer.StaffStoreId;

            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            var categories = await _categoryService.GetAllCategories(showHidden: true, storeId: storeId);
            foreach (var c in categories)
                model.AvailableCategories.Add(new SelectListItem { Text = _categoryService.GetFormattedBreadCrumb(c, categories), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var m in await _manufacturerService.GetAllManufacturers(showHidden: true, storeId: storeId))
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
        public virtual async Task<(IList<ProductModel> products, int totalCount)> PrepareProductModel(ProductModel.AddRequiredProductModel model, int pageIndex, int pageSize)
        {
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }
            //limit for store manager
            if (_workContext.CurrentCustomer.IsStaff())
                model.SearchStoreId = _workContext.CurrentCustomer.StaffStoreId;

            var products = await _productService.PrepareProductList(model.SearchCategoryId, model.SearchManufacturerId, model.SearchStoreId, model.SearchVendorId, model.SearchProductTypeId, model.SearchProductName, pageIndex, pageSize);
            return (products.Select(x => x.ToModel(_dateTimeHelper)).ToList(), products.TotalCount);
        }
        public virtual async Task<(IList<ProductModel> products, int totalCount)> PrepareProductModel(ProductModel.AddRelatedProductModel model, int pageIndex, int pageSize)
        {
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }
            //limit for store manager
            if (_workContext.CurrentCustomer.IsStaff())
                model.SearchStoreId = _workContext.CurrentCustomer.StaffStoreId;

            var products = await _productService.PrepareProductList(model.SearchCategoryId, model.SearchManufacturerId, model.SearchStoreId, model.SearchVendorId, model.SearchProductTypeId, model.SearchProductName, pageIndex, pageSize);
            return (products.Select(x => x.ToModel(_dateTimeHelper)).ToList(), products.TotalCount);
        }
        public virtual async Task<(IList<ProductModel> products, int totalCount)> PrepareProductModel(ProductModel.AddSimilarProductModel model, int pageIndex, int pageSize)
        {
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }
            //limit for store manager
            if (_workContext.CurrentCustomer.IsStaff())
                model.SearchStoreId = _workContext.CurrentCustomer.StaffStoreId;

            var products = await _productService.PrepareProductList(model.SearchCategoryId, model.SearchManufacturerId, model.SearchStoreId, model.SearchVendorId, model.SearchProductTypeId, model.SearchProductName, pageIndex, pageSize);
            return (products.Select(x => x.ToModel(_dateTimeHelper)).ToList(), products.TotalCount);
        }
        public virtual async Task<(IList<ProductModel> products, int totalCount)> PrepareProductModel(ProductModel.AddBundleProductModel model, int pageIndex, int pageSize)
        {
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }
            //limit for store manager
            if (_workContext.CurrentCustomer.IsStaff())
                model.SearchStoreId = _workContext.CurrentCustomer.StaffStoreId;

            var products = await _productService.PrepareProductList(model.SearchCategoryId, model.SearchManufacturerId, model.SearchStoreId, model.SearchVendorId, (int)ProductType.SimpleProduct, model.SearchProductName, pageIndex, pageSize);
            return (products.Select(x => x.ToModel(_dateTimeHelper)).ToList(), products.TotalCount);
        }
        public virtual async Task<(IList<ProductModel> products, int totalCount)> PrepareProductModel(ProductModel.AddCrossSellProductModel model, int pageIndex, int pageSize)
        {
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }

            //limit for store manager
            if (_workContext.CurrentCustomer.IsStaff())
                model.SearchStoreId = _workContext.CurrentCustomer.StaffStoreId;

            var products = await _productService.PrepareProductList(model.SearchCategoryId, model.SearchManufacturerId, model.SearchStoreId, model.SearchVendorId, model.SearchProductTypeId, model.SearchProductName, pageIndex, pageSize);
            return (products.Select(x => x.ToModel(_dateTimeHelper)).ToList(), products.TotalCount);
        }
        public virtual async Task<(IList<ProductModel> products, int totalCount)> PrepareProductModel(ProductModel.AddAssociatedProductModel model, int pageIndex, int pageSize)
        {
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }
            //limit for store manager
            if (_workContext.CurrentCustomer.IsStaff())
                model.SearchStoreId = _workContext.CurrentCustomer.StaffStoreId;

            var products = await _productService.PrepareProductList(model.SearchCategoryId, model.SearchManufacturerId, model.SearchStoreId, model.SearchVendorId, model.SearchProductTypeId, model.SearchProductName, pageIndex, pageSize);
            return (products.Select(x => x.ToModel(_dateTimeHelper)).ToList(), products.TotalCount);
        }
        public virtual async Task<(IList<ProductModel> products, int totalCount)> PrepareProductModel(ProductModel.ProductAttributeValueModel.AssociateProductToAttributeValueModel model, int pageIndex, int pageSize)
        {
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }
            //limit for store manager
            if (_workContext.CurrentCustomer.IsStaff())
                model.SearchStoreId = _workContext.CurrentCustomer.StaffStoreId;

            var products = await _productService.PrepareProductList(model.SearchCategoryId, model.SearchManufacturerId, model.SearchStoreId, model.SearchVendorId, (int)ProductType.SimpleProduct, model.SearchProductName, pageIndex, pageSize);
            return (products.Select(x => x.ToModel(_dateTimeHelper)).ToList(), products.TotalCount);
        }
        public virtual async Task<IList<ProductModel.ProductCategoryModel>> PrepareProductCategoryModel(Product product)
        {
            var productCategories = product.ProductCategories.OrderBy(x => x.DisplayOrder);
            var items = new List<ProductModel.ProductCategoryModel>();
            foreach (var x in productCategories)
            {
                var category = await _categoryService.GetCategoryById(x.CategoryId);
                items.Add(new ProductModel.ProductCategoryModel {
                    Id = x.Id,
                    Category = await _categoryService.GetFormattedBreadCrumb(category),
                    ProductId = product.Id,
                    CategoryId = x.CategoryId,
                    IsFeaturedProduct = x.IsFeaturedProduct,
                    DisplayOrder = x.DisplayOrder
                });
            }
            return items;
        }
        public virtual async Task InsertProductCategoryModel(ProductModel.ProductCategoryModel model)
        {
            var product = await _productService.GetProductById(model.ProductId, true);
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    throw new ArgumentException("This is not your product");
                }
            }

            if (product.ProductCategories.Where(x => x.CategoryId == model.CategoryId).Count() == 0)
            {
                var productCategory = new ProductCategory {
                    ProductId = model.ProductId,
                    CategoryId = model.CategoryId,
                    DisplayOrder = model.DisplayOrder,
                };
                //a vendor cannot edit "IsFeaturedProduct" property
                if (_workContext.CurrentVendor == null)
                {
                    productCategory.IsFeaturedProduct = model.IsFeaturedProduct;
                }
                await _categoryService.InsertProductCategory(productCategory);
            }
        }
        public virtual async Task UpdateProductCategoryModel(ProductModel.ProductCategoryModel model)
        {
            var product = await _productService.GetProductById(model.ProductId, true);
            var productCategory = product.ProductCategories.Where(x => x.Id == model.Id).FirstOrDefault();
            if (productCategory == null)
                throw new ArgumentException("No product category mapping found with the specified id");

            if (product.ProductCategories.Where(x => x.Id != model.Id && x.CategoryId == model.CategoryId).Any())
                throw new ArgumentException("This category is already mapped with this product");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    throw new ArgumentException("This is not your product");
                }
            }
            productCategory.CategoryId = model.CategoryId;
            productCategory.DisplayOrder = model.DisplayOrder;
            productCategory.ProductId = model.ProductId;
            //a vendor cannot edit "IsFeaturedProduct" property
            if (_workContext.CurrentVendor == null)
            {
                productCategory.IsFeaturedProduct = model.IsFeaturedProduct;
            }
            await _categoryService.UpdateProductCategory(productCategory);
        }
        public virtual async Task DeleteProductCategory(string id, string productId)
        {
            var product = await _productService.GetProductById(productId, true);
            var productCategory = product.ProductCategories.Where(x => x.Id == id).FirstOrDefault();
            if (productCategory == null)
                throw new ArgumentException("No product category mapping found with the specified id");
            productCategory.ProductId = productId;

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    throw new ArgumentException("This is not your product");
                }
            }
            await _categoryService.DeleteProductCategory(productCategory);
        }
        public virtual async Task<IList<ProductModel.ProductManufacturerModel>> PrepareProductManufacturerModel(Product product)
        {
            var items = new List<ProductModel.ProductManufacturerModel>();
            foreach (var x in product.ProductManufacturers.OrderBy(x => x.DisplayOrder))
            {
                items.Add(new ProductModel.ProductManufacturerModel {
                    Id = x.Id,
                    Manufacturer = (await _manufacturerService.GetManufacturerById(x.ManufacturerId)).Name,
                    ProductId = product.Id,
                    ManufacturerId = x.ManufacturerId,
                    IsFeaturedProduct = x.IsFeaturedProduct,
                    DisplayOrder = x.DisplayOrder
                });
            }
            return items;
        }
        public virtual async Task InsertProductManufacturer(ProductModel.ProductManufacturerModel model)
        {
            var manufacturerId = model.ManufacturerId;
            var product = await _productService.GetProductById(model.ProductId, true);
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    throw new ArgumentException("This is not your product");
                }
            }

            var existingProductmanufacturers = product.ProductManufacturers;
            if (product.ProductManufacturers.Where(x => x.ManufacturerId == manufacturerId).Count() == 0)
            {
                var productManufacturer = new ProductManufacturer {
                    ProductId = model.ProductId,
                    ManufacturerId = manufacturerId,
                    DisplayOrder = model.DisplayOrder,
                };
                //a vendor cannot edit "IsFeaturedProduct" property
                if (_workContext.CurrentVendor == null)
                {
                    productManufacturer.IsFeaturedProduct = model.IsFeaturedProduct;
                }
                await _manufacturerService.InsertProductManufacturer(productManufacturer);
            }
        }
        public virtual async Task UpdateProductManufacturer(ProductModel.ProductManufacturerModel model)
        {
            var product = await _productService.GetProductById(model.ProductId, true);
            var productManufacturer = product.ProductManufacturers.Where(x => x.Id == model.Id).FirstOrDefault();
            if (productManufacturer == null)
                throw new ArgumentException("No product manufacturer mapping found with the specified id");

            if (product.ProductManufacturers.Where(x => x.Id != model.Id && x.ManufacturerId == model.ManufacturerId).Any())
                throw new ArgumentException("This manufacturer is already mapped with this product");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    throw new ArgumentException("This is not your product");
                }
            }
            productManufacturer.ProductId = product.Id;
            productManufacturer.ManufacturerId = model.ManufacturerId;
            productManufacturer.DisplayOrder = model.DisplayOrder;
            //a vendor cannot edit "IsFeaturedProduct" property
            if (_workContext.CurrentVendor == null)
            {
                productManufacturer.IsFeaturedProduct = model.IsFeaturedProduct;
            }
            await _manufacturerService.UpdateProductManufacturer(productManufacturer);
        }
        public virtual async Task DeleteProductManufacturer(string id, string productId)
        {
            var product = await _productService.GetProductById(productId, true);
            var productManufacturer = product.ProductManufacturers.Where(x => x.Id == id).FirstOrDefault();
            if (productManufacturer == null)
                throw new ArgumentException("No product manufacturer mapping found with the specified id");
            productManufacturer.ProductId = productId;
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    throw new ArgumentException("This is not your product");
                }
            }
            await _manufacturerService.DeleteProductManufacturer(productManufacturer);
        }
        public virtual async Task InsertRelatedProductModel(ProductModel.AddRelatedProductModel model)
        {
            var productId1 = await _productService.GetProductById(model.ProductId, true);

            foreach (var id in model.SelectedProductIds)
            {
                var product = await _productService.GetProductById(id);
                if (product != null)
                {
                    //a vendor should have access only to his products
                    if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                        continue;

                    var existingRelatedProducts = productId1.RelatedProducts;
                    if (model.ProductId != id)
                        if (existingRelatedProducts.Where(x => x.ProductId2 == id).Count() == 0)
                        {
                            var related = new RelatedProduct {
                                ProductId1 = model.ProductId,
                                ProductId2 = id,
                                DisplayOrder = 1,
                            };
                            productId1.RelatedProducts.Add(related);
                            await _productService.InsertRelatedProduct(related);
                        }
                }
            }
        }
        public virtual async Task UpdateRelatedProductModel(ProductModel.RelatedProductModel model)
        {
            var product1 = await _productService.GetProductById(model.ProductId1, true);
            var relatedProduct = product1.RelatedProducts.Where(x => x.Id == model.Id).FirstOrDefault();
            if (relatedProduct == null)
                throw new ArgumentException("No related product found with the specified id");

            var product2 = await _productService.GetProductById(relatedProduct.ProductId2);
            if (product2 == null)
                throw new ArgumentException("No product found with the specified id");
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                if (product2 != null && product2.VendorId != _workContext.CurrentVendor.Id)
                {
                    throw new ArgumentException("This is not your product");
                }
            }
            relatedProduct.ProductId1 = model.ProductId1;
            relatedProduct.DisplayOrder = model.DisplayOrder;
            await _productService.UpdateRelatedProduct(relatedProduct);
        }
        public virtual async Task DeleteRelatedProductModel(ProductModel.RelatedProductModel model)
        {
            var product = await _productService.GetProductById(model.ProductId1, true);
            var relatedProduct = product.RelatedProducts.Where(x => x.Id == model.Id).FirstOrDefault();
            if (relatedProduct == null)
                throw new ArgumentException("No related product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    throw new ArgumentException("This is not your product");
                }
            }
            relatedProduct.ProductId1 = model.ProductId1;
            await _productService.DeleteRelatedProduct(relatedProduct);
        }
        public virtual async Task InsertSimilarProductModel(ProductModel.AddSimilarProductModel model)
        {
            var productId1 = await _productService.GetProductById(model.ProductId, true);

            foreach (string id in model.SelectedProductIds)
            {
                var product = await _productService.GetProductById(id);
                if (product != null)
                {
                    //a vendor should have access only to his products
                    if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                        continue;

                    var existingSimilarProducts = productId1.SimilarProducts;
                    if (model.ProductId != id)
                        if (existingSimilarProducts.Where(x => x.ProductId2 == id).Count() == 0)
                        {
                            var similar = new SimilarProduct {
                                ProductId1 = model.ProductId,
                                ProductId2 = id,
                                DisplayOrder = 1,
                            };
                            productId1.SimilarProducts.Add(similar);
                            await _productService.InsertSimilarProduct(similar);
                        }
                }
            }
        }
        public virtual async Task UpdateSimilarProductModel(ProductModel.SimilarProductModel model)
        {
            var product1 = await _productService.GetProductById(model.ProductId1, true);
            var similarProduct = product1.SimilarProducts.Where(x => x.Id == model.Id).FirstOrDefault();
            if (similarProduct == null)
                throw new ArgumentException("No similar product found with the specified id");

            var product2 = await _productService.GetProductById(similarProduct.ProductId2);
            if (product2 == null)
                throw new ArgumentException("No product found with the specified id");
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                if (product2 != null && product2.VendorId != _workContext.CurrentVendor.Id)
                {
                    throw new ArgumentException("This is not your product");
                }
            }
            similarProduct.ProductId1 = model.ProductId1;
            similarProduct.DisplayOrder = model.DisplayOrder;
            await _productService.UpdateSimilarProduct(similarProduct);
        }
        public virtual async Task DeleteSimilarProductModel(ProductModel.SimilarProductModel model)
        {
            var product = await _productService.GetProductById(model.ProductId1, true);
            var similarProduct = product.SimilarProducts.Where(x => x.Id == model.Id).FirstOrDefault();
            if (similarProduct == null)
                throw new ArgumentException("No similar product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    throw new ArgumentException("This is not your product");
                }
            }
            similarProduct.ProductId1 = model.ProductId1;
            await _productService.DeleteSimilarProduct(similarProduct);
        }
        public virtual async Task InsertBundleProductModel(ProductModel.AddBundleProductModel model)
        {
            var productId1 = await _productService.GetProductById(model.ProductId, true);

            foreach (var id in model.SelectedProductIds)
            {
                var product = await _productService.GetProductById(id);
                if (product != null)
                {
                    //a vendor should have access only to his products
                    if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                        continue;

                    var existingBundleProducts = productId1.BundleProducts;
                    if (model.ProductId != id)
                        if (existingBundleProducts.Where(x => x.ProductId == id).Count() == 0)
                        {
                            var bundle = new BundleProduct {
                                ProductBundleId = model.ProductId,
                                ProductId = id,
                                DisplayOrder = 1,
                                Quantity = 1,
                            };
                            productId1.BundleProducts.Add(bundle);
                            await _productService.InsertBundleProduct(bundle);
                        }
                }
            }
        }
        public virtual async Task UpdateBundleProductModel(ProductModel.BundleProductModel model)
        {
            var product = await _productService.GetProductById(model.ProductBundleId, true);
            var bundleProduct = product.BundleProducts.Where(x => x.Id == model.Id).FirstOrDefault();
            if (bundleProduct == null)
                throw new ArgumentException("No bundle product found with the specified id");

            var product2 = await _productService.GetProductById(bundleProduct.ProductId);
            if (product2 == null)
                throw new ArgumentException("No product found with the specified id");
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                if (product2 != null && product2.VendorId != _workContext.CurrentVendor.Id)
                {
                    throw new ArgumentException("This is not your product");
                }
            }
            bundleProduct.ProductBundleId = model.ProductBundleId;
            bundleProduct.ProductId = model.ProductId;
            bundleProduct.Quantity = model.Quantity > 0 ? model.Quantity : 1;
            bundleProduct.DisplayOrder = model.DisplayOrder;
            await _productService.UpdateBundleProduct(bundleProduct);
        }
        public virtual async Task DeleteBundleProductModel(ProductModel.BundleProductModel model)
        {
            var product = await _productService.GetProductById(model.ProductBundleId, true);
            var bundleProduct = product.BundleProducts.Where(x => x.Id == model.Id).FirstOrDefault();
            if (bundleProduct == null)
                throw new ArgumentException("No bundle product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    throw new ArgumentException("This is not your product");
                }
            }
            bundleProduct.ProductBundleId = model.ProductBundleId;
            await _productService.DeleteBundleProduct(bundleProduct);
        }
        public virtual async Task InsertCrossSellProductModel(ProductModel.AddCrossSellProductModel model)
        {
            var crossSellProduct = await _productService.GetProductById(model.ProductId, true);
            foreach (var id in model.SelectedProductIds)
            {
                var product = await _productService.GetProductById(id);
                if (product != null)
                {
                    //a vendor should have access only to his products
                    if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                        continue;

                    if (crossSellProduct.CrossSellProduct.Where(x => x == id).Count() == 0)
                    {
                        if (model.ProductId != id)
                            await _productService.InsertCrossSellProduct(
                                new CrossSellProduct {
                                    ProductId1 = model.ProductId,
                                    ProductId2 = id,
                                });
                    }
                }
            }
        }
        public virtual async Task DeleteCrossSellProduct(string productId, string crossSellProductId)
        {
            var crosssell = new CrossSellProduct() {
                ProductId1 = productId,
                ProductId2 = crossSellProductId
            };
            await _productService.DeleteCrossSellProduct(crosssell);
        }
        public virtual async Task InsertAssociatedProductModel(ProductModel.AddAssociatedProductModel model)
        {
            foreach (var id in model.SelectedProductIds)
            {
                var product = await _productService.GetProductById(id);
                if (product != null)
                {
                    //a vendor should have access only to his products
                    if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                        continue;

                    product.ParentGroupedProductId = model.ProductId;
                    await _productService.UpdateAssociatedProduct(product);
                }
            }
        }
        public virtual async Task DeleteAssociatedProduct(Product product)
        {
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                throw new ArgumentException("This is not your product");

            product.ParentGroupedProductId = "";
            await _productService.UpdateAssociatedProduct(product);
        }
        public virtual async Task<ProductModel.AddRelatedProductModel> PrepareRelatedProductModel()
        {
            var model = new ProductModel.AddRelatedProductModel {
                //a vendor should have access only to his products
                IsLoggedInAsVendor = _workContext.CurrentVendor != null
            };
            var storeId = string.Empty;

            if (_workContext.CurrentCustomer.IsStaff())
                storeId = _workContext.CurrentCustomer.StaffStoreId;

            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            var categories = await _categoryService.GetAllCategories(showHidden: true, storeId: storeId);
            foreach (var c in categories)
                model.AvailableCategories.Add(new SelectListItem { Text = _categoryService.GetFormattedBreadCrumb(c, categories), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var m in await _manufacturerService.GetAllManufacturers(showHidden: true, storeId: storeId))
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
        public virtual async Task<ProductModel.AddSimilarProductModel> PrepareSimilarProductModel()
        {
            var model = new ProductModel.AddSimilarProductModel {
                //a vendor should have access only to his products
                IsLoggedInAsVendor = _workContext.CurrentVendor != null
            };

            var storeId = string.Empty;
            if (_workContext.CurrentCustomer.IsStaff())
                storeId = _workContext.CurrentCustomer.StaffStoreId;

            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            var categories = await _categoryService.GetAllCategories(showHidden: true, storeId: storeId);
            foreach (var c in categories)
                model.AvailableCategories.Add(new SelectListItem { Text = _categoryService.GetFormattedBreadCrumb(c, categories), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var m in await _manufacturerService.GetAllManufacturers(showHidden: true, storeId: storeId))
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
        public virtual async Task<ProductModel.AddBundleProductModel> PrepareBundleProductModel()
        {
            var model = new ProductModel.AddBundleProductModel {
                //a vendor should have access only to his products
                IsLoggedInAsVendor = _workContext.CurrentVendor != null
            };
            var storeId = string.Empty;

            if (_workContext.CurrentCustomer.IsStaff())
                storeId = _workContext.CurrentCustomer.StaffStoreId;

            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            var categories = await _categoryService.GetAllCategories(showHidden: true, storeId: storeId);
            foreach (var c in categories)
                model.AvailableCategories.Add(new SelectListItem { Text = _categoryService.GetFormattedBreadCrumb(c, categories), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var m in await _manufacturerService.GetAllManufacturers(showHidden: true, storeId: storeId))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var s in (await _storeService.GetAllStores()).Where(x => x.Id == storeId || string.IsNullOrWhiteSpace(storeId)))
                model.AvailableStores.Add(new SelectListItem { Text = s.Shortcut, Value = s.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var v in await _vendorService.GetAllVendors(showHidden: true))
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            return model;
        }
        public virtual async Task<ProductModel.AddCrossSellProductModel> PrepareCrossSellProductModel()
        {
            var model = new ProductModel.AddCrossSellProductModel {
                //a vendor should have access only to his products
                IsLoggedInAsVendor = _workContext.CurrentVendor != null
            };

            var storeId = string.Empty;
            if (_workContext.CurrentCustomer.IsStaff())
                storeId = _workContext.CurrentCustomer.StaffStoreId;

            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            var categories = await _categoryService.GetAllCategories(showHidden: true, storeId: storeId);
            foreach (var c in categories)
                model.AvailableCategories.Add(new SelectListItem { Text = _categoryService.GetFormattedBreadCrumb(c, categories), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var m in await _manufacturerService.GetAllManufacturers(showHidden: true, storeId: storeId))
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
        public virtual async Task<ProductModel.AddAssociatedProductModel> PrepareAssociatedProductModel()
        {
            var model = new ProductModel.AddAssociatedProductModel {
                //a vendor should have access only to his products
                IsLoggedInAsVendor = _workContext.CurrentVendor != null
            };

            var storeId = string.Empty;
            if (_workContext.CurrentCustomer.IsStaff())
                storeId = _workContext.CurrentCustomer.StaffStoreId;

            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            var categories = await _categoryService.GetAllCategories(showHidden: true, storeId: storeId);
            foreach (var c in categories)
                model.AvailableCategories.Add(new SelectListItem { Text = _categoryService.GetFormattedBreadCrumb(c, categories), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var m in await _manufacturerService.GetAllManufacturers(showHidden: true, storeId: storeId))
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
        public virtual async Task<BulkEditListModel> PrepareBulkEditListModel()
        {
            var model = new BulkEditListModel();

            var storeId = string.Empty;

            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            var categories = await _categoryService.GetAllCategories(showHidden: true, storeId: storeId);
            foreach (var c in categories)
                model.AvailableCategories.Add(new SelectListItem { Text = _categoryService.GetFormattedBreadCrumb(c, categories), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var m in await _manufacturerService.GetAllManufacturers(showHidden: true, storeId: storeId))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(_localizationService, _workContext, false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            // avaible stores
            if (_workContext.CurrentCustomer.IsStaff())
            {
                storeId = _workContext.CurrentCustomer.StaffStoreId;
                var store = (await _storeService.GetAllStores()).Where(x => x.Id == storeId).FirstOrDefault();
                if (store != null)
                    model.AvailableStores.Add(new SelectListItem { Text = store.Shortcut, Value = store.Id.ToString() });
            }
            else
            {
                model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });

                foreach (var s in (await _storeService.GetAllStores()))
                {
                    model.AvailableStores.Add(new SelectListItem { Text = s.Shortcut, Value = s.Id.ToString() });
                }
            }
            return model;
        }
        public virtual async Task<(IEnumerable<BulkEditProductModel> bulkEditProductModels, int totalCount)> PrepareBulkEditProductModel(BulkEditListModel model, int pageIndex, int pageSize)
        {
            var storeId = model.SearchStoreId;
            if (_workContext.CurrentCustomer.IsStaff())
                storeId = _workContext.CurrentCustomer.StaffStoreId;

            var vendorId = "";
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
                vendorId = _workContext.CurrentVendor.Id;

            var searchCategoryIds = new List<string>();
            if (!string.IsNullOrEmpty(model.SearchCategoryId))
                searchCategoryIds.Add(model.SearchCategoryId);

            var products = (await _productService.SearchProducts(categoryIds: searchCategoryIds,
                manufacturerId: model.SearchManufacturerId,
                vendorId: vendorId,
                storeId: storeId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                pageIndex: pageIndex - 1,
                pageSize: pageSize,
                showHidden: true)).products;

            return (products.Select(x =>
            {
                var productModel = new BulkEditProductModel {
                    Id = x.Id,
                    Name = x.Name,
                    Sku = x.Sku,
                    OldPrice = x.OldPrice,
                    Price = x.Price,
                    ManageInventoryMethod = x.ManageInventoryMethod.GetLocalizedEnum(_localizationService, _workContext.WorkingLanguage.Id),
                    StockQuantity = x.StockQuantity,
                    Published = x.Published
                };
                if (x.ManageInventoryMethod == ManageInventoryMethod.ManageStock && x.UseMultipleWarehouses)
                {
                    //multi-warehouse supported
                    //TODO localize
                    productModel.ManageInventoryMethod += " (multi-warehouse)";
                }
                return productModel;
            }), products.TotalCount);
        }
        public virtual async Task UpdateBulkEdit(IList<BulkEditProductModel> products)
        {
            foreach (var pModel in products)
            {
                //update
                var product = await _productService.GetProductById(pModel.Id, true);
                if (product != null)
                {
                    //a vendor should have access only to his products
                    if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                        continue;

                    //a staff can have access only to his products
                    if (_workContext.CurrentCustomer.IsStaff())
                    {
                        if (!product.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                            continue;
                    }

                    var prevStockQuantity = product.GetTotalStockQuantity();

                    product.Sku = pModel.Sku;
                    product.Price = pModel.Price;
                    product.OldPrice = pModel.OldPrice;
                    product.StockQuantity = pModel.StockQuantity;
                    product.Published = pModel.Published;
                    product.Name = pModel.Name;
                    await _productService.UpdateProduct(product);

                    //back in stock notifications
                    if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                        product.BackorderMode == BackorderMode.NoBackorders &&
                        product.AllowBackInStockSubscriptions &&
                        product.GetTotalStockQuantity() > 0 &&
                        prevStockQuantity <= 0 && !product.UseMultipleWarehouses &&
                        product.Published)
                    {
                        await _backInStockSubscriptionService.SendNotificationsToSubscribers(product, "");
                    }
                }
            }

        }
        public virtual async Task DeleteBulkEdit(IList<BulkEditProductModel> products)
        {
            foreach (var pModel in products)
            {
                //delete
                var product = await _productService.GetProductById(pModel.Id, true);
                if (product != null)
                {
                    //a vendor should have access only to his products
                    if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                        continue;

                    if (_workContext.CurrentCustomer.IsStaff())
                    {
                        if (!product.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                            continue;
                    }

                    await _productService.DeleteProduct(product);
                }
            }
        }
        public virtual async Task<IList<ProductModel.TierPriceModel>> PrepareTierPriceModel(Product product)
        {
            var storeId = string.Empty;
            if (_workContext.CurrentCustomer.IsStaff())
                storeId = _workContext.CurrentCustomer.StaffStoreId;

            var items = new List<ProductModel.TierPriceModel>();
            foreach (var x in product.TierPrices.Where(x => x.StoreId == storeId || string.IsNullOrWhiteSpace(storeId) || string.IsNullOrWhiteSpace(x.StoreId)).OrderBy(x => x.StoreId).ThenBy(x => x.Quantity).ThenBy(x => x.CustomerRoleId))
            {
                string storeName;
                if (!string.IsNullOrEmpty(x.StoreId))
                {
                    var store = await _storeService.GetStoreById(x.StoreId);
                    storeName = store != null ? store.Shortcut : "Deleted";
                }
                else
                    storeName = _localizationService.GetResource("Admin.Catalog.Products.TierPrices.Fields.Store.All");

                items.Add(new ProductModel.TierPriceModel {
                    Id = x.Id,
                    StoreId = x.StoreId,
                    Store = storeName,
                    CustomerRole = !string.IsNullOrEmpty(x.CustomerRoleId) ? (await _customerService.GetCustomerRoleById(x.CustomerRoleId)).Name : _localizationService.GetResource("Admin.Catalog.Products.TierPrices.Fields.CustomerRole.All"),
                    ProductId = product.Id,
                    CustomerRoleId = !string.IsNullOrEmpty(x.CustomerRoleId) ? x.CustomerRoleId : "",
                    Quantity = x.Quantity,
                    Price = x.Price,
                    StartDateTime = x.StartDateTimeUtc.HasValue ? _dateTimeHelper.ConvertToUserTime(x.StartDateTimeUtc.Value, DateTimeKind.Utc) : new DateTime?(),
                    EndDateTime = x.EndDateTimeUtc.HasValue ? _dateTimeHelper.ConvertToUserTime(x.EndDateTimeUtc.Value, DateTimeKind.Utc) : new DateTime?()
                });
            };
            return items;
        }
        public virtual async Task<(IEnumerable<ProductModel.BidModel> bidModels, int totalCount)> PrepareBidMode(string productId, int pageIndex, int pageSize)
        {
            var auctionService = _serviceProvider.GetRequiredService<IAuctionService>();
            var priceFormatter = _serviceProvider.GetRequiredService<IPriceFormatter>();
            var bids = await auctionService.GetBidsByProductId(productId, pageIndex - 1, pageSize);
            var bidsModel = new List<ProductModel.BidModel>();
            foreach (var x in bids)
            {
                bidsModel.Add(new ProductModel.BidModel {
                    BidId = x.Id,
                    ProductId = x.ProductId,
                    Amount = priceFormatter.FormatPrice(x.Amount),
                    Date = _dateTimeHelper.ConvertToUserTime(x.Date, DateTimeKind.Utc),
                    CustomerId = x.CustomerId,
                    Email = (await _customerService.GetCustomerById(x.CustomerId))?.Email,
                    OrderId = x.OrderId
                });
            }
            return (bidsModel, bids.TotalCount);
        }
        public virtual async Task<(IEnumerable<ProductModel.ActivityLogModel> activityLogModels, int totalCount)> PrepareActivityLogModel(string productId, int pageIndex, int pageSize)
        {
            var activityLog = await _customerActivityService.GetProductActivities(null, null, productId, pageIndex - 1, pageSize);
            var items = new List<ProductModel.ActivityLogModel>();
            foreach (var x in activityLog)
            {
                var customer = await _customerService.GetCustomerById(x.CustomerId);
                items.Add(
                new ProductModel.ActivityLogModel {
                    Id = x.Id,
                    ActivityLogTypeName = (await _customerActivityService.GetActivityTypeById(x.ActivityLogTypeId))?.Name,
                    Comment = x.Comment,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc),
                    CustomerId = x.CustomerId,
                    CustomerEmail = customer != null ? customer.Email : "null"
                });
            }
            return (items, activityLog.TotalCount);
        }
        public virtual async Task<ProductModel.ProductAttributeMappingModel> PrepareProductAttributeMappingModel(Product product)
        {
            var model = new ProductModel.ProductAttributeMappingModel {
                ProductId = product.Id
            };
            foreach (var attribute in await _productAttributeService.GetAllProductAttributes())
            {
                model.AvailableProductAttribute.Add(new SelectListItem() {
                    Value = attribute.Id,
                    Text = attribute.Name
                });
            }
            return model;
        }
        public virtual async Task<ProductModel.ProductAttributeMappingModel> PrepareProductAttributeMappingModel(Product product, ProductAttributeMapping productAttributeMapping)
        {
            var model = productAttributeMapping.ToModel();
            foreach (var attribute in await _productAttributeService.GetAllProductAttributes())
            {
                model.AvailableProductAttribute.Add(new SelectListItem() {
                    Value = attribute.Id,
                    Text = attribute.Name,
                    Selected = attribute.Id == model.ProductAttributeId
                });
            }
            return model;
        }
        public virtual async Task<ProductModel.ProductAttributeMappingModel> PrepareProductAttributeMappingModel(ProductModel.ProductAttributeMappingModel model)
        {
            foreach (var attribute in await _productAttributeService.GetAllProductAttributes())
            {
                model.AvailableProductAttribute.Add(new SelectListItem() {
                    Value = attribute.Id,
                    Text = attribute.Name
                });
            }
            return model;
        }
        public virtual async Task<IList<ProductModel.ProductAttributeMappingModel>> PrepareProductAttributeMappingModels(Product product)
        {
            var items = new List<ProductModel.ProductAttributeMappingModel>();
            foreach (var x in product.ProductAttributeMappings.OrderBy(x => x.DisplayOrder))
            {
                var attributeModel = new ProductModel.ProductAttributeMappingModel {
                    Id = x.Id,
                    ProductId = product.Id,
                    ProductAttribute = (await _productAttributeService.GetProductAttributeById(x.ProductAttributeId))?.Name,
                    ProductAttributeId = x.ProductAttributeId,
                    TextPrompt = x.TextPrompt,
                    IsRequired = x.IsRequired,
                    ShowOnCatalogPage = x.ShowOnCatalogPage,
                    AttributeControlType = x.AttributeControlType.GetLocalizedEnum(_localizationService, _workContext),
                    AttributeControlTypeId = x.AttributeControlTypeId,
                    DisplayOrder = x.DisplayOrder
                };


                if (x.ShouldHaveValues())
                {
                    attributeModel.ShouldHaveValues = true;
                    attributeModel.TotalValues = x.ProductAttributeValues.Count;
                }

                if (x.ValidationRulesAllowed())
                {
                    var validationRules = new StringBuilder(string.Empty);
                    attributeModel.ValidationRulesAllowed = true;
                    if (x.ValidationMinLength != null)
                        validationRules.AppendFormat("{0}: {1}<br />",
                            _localizationService.GetResource("Admin.Catalog.Products.ProductAttributes.Attributes.ValidationRules.MinLength"),
                            x.ValidationMinLength);
                    if (x.ValidationMaxLength != null)
                        validationRules.AppendFormat("{0}: {1}<br />",
                            _localizationService.GetResource("Admin.Catalog.Products.ProductAttributes.Attributes.ValidationRules.MaxLength"),
                            x.ValidationMaxLength);
                    if (!string.IsNullOrEmpty(x.ValidationFileAllowedExtensions))
                        validationRules.AppendFormat("{0}: {1}<br />",
                            _localizationService.GetResource("Admin.Catalog.Products.ProductAttributes.Attributes.ValidationRules.FileAllowedExtensions"),
                            System.Net.WebUtility.HtmlEncode(x.ValidationFileAllowedExtensions));
                    if (x.ValidationFileMaximumSize != null)
                        validationRules.AppendFormat("{0}: {1}<br />",
                            _localizationService.GetResource("Admin.Catalog.Products.ProductAttributes.Attributes.ValidationRules.FileMaximumSize"),
                            x.ValidationFileMaximumSize);
                    if (!string.IsNullOrEmpty(x.DefaultValue))
                        validationRules.AppendFormat("{0}: {1}<br />",
                            _localizationService.GetResource("Admin.Catalog.Products.ProductAttributes.Attributes.ValidationRules.DefaultValue"),
                            System.Net.WebUtility.HtmlEncode(x.DefaultValue));
                    attributeModel.ValidationRulesString = validationRules.ToString();
                }

                //currenty any attribute can have condition. why not?
                attributeModel.ConditionAllowed = true;
                var conditionAttribute = _productAttributeParser.ParseProductAttributeMappings(product, x.ConditionAttributeXml).FirstOrDefault();
                var conditionValue = _productAttributeParser.ParseProductAttributeValues(product, x.ConditionAttributeXml).FirstOrDefault();
                if (conditionAttribute != null && conditionValue != null)
                {
                    var productAttribute = await _productAttributeService.GetProductAttributeById(conditionAttribute.ProductAttributeId);
                    var _paname = productAttribute != null ? productAttribute.Name : "";
                    attributeModel.ConditionString = string.Format("{0}: {1}",
                        System.Net.WebUtility.HtmlEncode(_paname),
                        System.Net.WebUtility.HtmlEncode(conditionValue.Name));
                }
                else
                    attributeModel.ConditionString = string.Empty;
                items.Add(attributeModel);
            }
            return items;
        }

        public virtual async Task InsertProductAttributeMappingModel(ProductModel.ProductAttributeMappingModel model)
        {
            //insert mapping
            var productAttributeMapping = model.ToEntity();
            //predefined values
            var predefinedValues = await _productAttributeService.GetPredefinedProductAttributeValues(model.ProductAttributeId);
            foreach (var predefinedValue in predefinedValues)
            {
                var pav = predefinedValue.ToEntity();
                //locales
                pav.Locales.Clear();
                var languages = await _languageService.GetAllLanguages(true);
                //localization
                foreach (var lang in languages)
                {
                    var name = predefinedValue.GetLocalized(x => x.Name, lang.Id, false, false);
                    if (!string.IsNullOrEmpty(name))
                        pav.Locales.Add(new LocalizedProperty() { LanguageId = lang.Id, LocaleKey = "Name", LocaleValue = name });
                }

                productAttributeMapping.ProductAttributeValues.Add(pav);
            }
            await _productAttributeService.InsertProductAttributeMapping(productAttributeMapping);
        }
        public virtual async Task UpdateProductAttributeMappingModel(ProductModel.ProductAttributeMappingModel model)
        {
            var product = await _productService.GetProductById(model.ProductId, true);
            if (product != null)
            {
                var productAttributeMapping = product.ProductAttributeMappings.Where(x => x.Id == model.Id).FirstOrDefault();
                if (productAttributeMapping != null)
                {
                    productAttributeMapping = model.ToEntity(productAttributeMapping);
                    await _productAttributeService.UpdateProductAttributeMapping(productAttributeMapping);
                }
            }
        }
        public virtual async Task<ProductModel.ProductAttributeMappingModel> PrepareProductAttributeMappingModel(ProductAttributeMapping productAttributeMapping)
        {
            var model = new ProductModel.ProductAttributeMappingModel {
                //prepare only used properties
                Id = productAttributeMapping.Id,
                ValidationRulesAllowed = productAttributeMapping.ValidationRulesAllowed(),
                AttributeControlTypeId = productAttributeMapping.AttributeControlTypeId,
                ValidationMinLength = productAttributeMapping.ValidationMinLength,
                ValidationMaxLength = productAttributeMapping.ValidationMaxLength,
                ValidationFileAllowedExtensions = productAttributeMapping.ValidationFileAllowedExtensions,
                ValidationFileMaximumSize = productAttributeMapping.ValidationFileMaximumSize,
                DefaultValue = productAttributeMapping.DefaultValue,
            };
            return await Task.FromResult(model);
        }
        public virtual async Task UpdateProductAttributeValidationRulesModel(ProductAttributeMapping productAttributeMapping, ProductModel.ProductAttributeMappingModel model)
        {
            productAttributeMapping.ProductId = model.ProductId;
            productAttributeMapping.ValidationMinLength = model.ValidationMinLength;
            productAttributeMapping.ValidationMaxLength = model.ValidationMaxLength;
            productAttributeMapping.ValidationFileAllowedExtensions = model.ValidationFileAllowedExtensions;
            productAttributeMapping.ValidationFileMaximumSize = model.ValidationFileMaximumSize;
            productAttributeMapping.DefaultValue = model.DefaultValue;
            await _productAttributeService.UpdateProductAttributeMapping(productAttributeMapping);
        }
        public virtual async Task<ProductAttributeConditionModel> PrepareProductAttributeConditionModel(Product product, ProductAttributeMapping productAttributeMapping)
        {
            var model = new ProductAttributeConditionModel {
                ProductAttributeMappingId = productAttributeMapping.Id,
                EnableCondition = !string.IsNullOrEmpty(productAttributeMapping.ConditionAttributeXml),
                ProductId = product.Id
            };
            //pre-select attribute and values
            var selectedPva = _productAttributeParser
                .ParseProductAttributeMappings(product, productAttributeMapping.ConditionAttributeXml)
                .FirstOrDefault();

            var attributes = product.ProductAttributeMappings
                //ignore non-combinable attributes (should have selectable values)
                .Where(x => x.CanBeUsedAsCondition())
                //ignore this attribute (it cannot depend on itself)
                .Where(x => x.Id != productAttributeMapping.Id)
                .ToList();
            foreach (var attribute in attributes)
            {
                var pam = await _productAttributeService.GetProductAttributeById(attribute.ProductAttributeId);
                var attributeModel = new ProductAttributeConditionModel.ProductAttributeModel {
                    Id = attribute.Id,
                    ProductAttributeId = attribute.ProductAttributeId,
                    Name = pam.Name,
                    TextPrompt = attribute.TextPrompt,
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType
                };

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = product.ProductAttributeMappings.FirstOrDefault(x => x.Id == attribute.Id).ProductAttributeValues;
                    foreach (var attributeValue in attributeValues)
                    {
                        var attributeValueModel = new ProductAttributeConditionModel.ProductAttributeValueModel {
                            Id = attributeValue.Id,
                            Name = attributeValue.Name,
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(attributeValueModel);
                    }

                    //pre-select attribute and value
                    if (selectedPva != null && attribute.Id == selectedPva.Id)
                    {
                        //attribute
                        model.SelectedProductAttributeId = selectedPva.Id;

                        //values
                        switch (attribute.AttributeControlType)
                        {
                            case AttributeControlType.DropdownList:
                            case AttributeControlType.RadioList:
                            case AttributeControlType.Checkboxes:
                            case AttributeControlType.ColorSquares:
                            case AttributeControlType.ImageSquares:
                                {
                                    if (!string.IsNullOrEmpty(productAttributeMapping.ConditionAttributeXml))
                                    {
                                        //clear default selection
                                        foreach (var item in attributeModel.Values)
                                            item.IsPreSelected = false;

                                        //select new values
                                        var selectedValues = _productAttributeParser.ParseProductAttributeValues(product, productAttributeMapping.ConditionAttributeXml);
                                        foreach (var attributeValue in selectedValues)
                                            foreach (var item in attributeModel.Values)
                                                if (attributeValue.Id == item.Id)
                                                    item.IsPreSelected = true;
                                    }
                                }
                                break;
                            case AttributeControlType.ReadonlyCheckboxes:
                            case AttributeControlType.TextBox:
                            case AttributeControlType.MultilineTextbox:
                            case AttributeControlType.Datepicker:
                            case AttributeControlType.FileUpload:
                            default:
                                //these attribute types are supported as conditions
                                break;
                        }
                    }
                }

                model.ProductAttributes.Add(attributeModel);
            }
            return model;
        }
        public virtual async Task UpdateProductAttributeConditionModel(Product product, ProductAttributeMapping productAttributeMapping, ProductAttributeConditionModel model, Dictionary<string, string> form)
        {
            string attributesXml = null;
            if (model.EnableCondition)
            {
                var attribute = product.ProductAttributeMappings.FirstOrDefault(x => x.Id == model.SelectedProductAttributeId);
                if (attribute != null)
                {
                    var controlId = string.Format("product_attribute_{0}", attribute.Id);
                    switch (attribute.AttributeControlType)
                    {
                        case AttributeControlType.DropdownList:
                        case AttributeControlType.RadioList:
                        case AttributeControlType.ColorSquares:
                        case AttributeControlType.ImageSquares:
                            {
                                var ctrlAttributes = form[controlId];
                                if (!string.IsNullOrEmpty(ctrlAttributes))
                                {
                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, ctrlAttributes);
                                }
                                else
                                {
                                    //for conditions we should empty values save even when nothing is selected
                                    //otherwise "attributesXml" will be empty
                                    //hence we won't be able to find a selected attribute
                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, "");
                                }
                            }
                            break;
                        case AttributeControlType.Checkboxes:
                            {
                                var cblAttributes = form[controlId];
                                if (!string.IsNullOrEmpty(cblAttributes))
                                {
                                    var anyValueSelected = false;
                                    foreach (var item in cblAttributes.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                    {
                                        if (!string.IsNullOrEmpty(item))
                                        {
                                            attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                                attribute, item);
                                            anyValueSelected = true;
                                        }
                                    }
                                    if (!anyValueSelected)
                                    {
                                        //for conditions we should save empty values even when nothing is selected
                                        //otherwise "attributesXml" will be empty
                                        //hence we won't be able to find a selected attribute
                                        attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                            attribute, "");
                                    }
                                }
                                else
                                {
                                    //for conditions we should save empty values even when nothing is selected
                                    //otherwise "attributesXml" will be empty
                                    //hence we won't be able to find a selected attribute
                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                            attribute, "");
                                }
                            }
                            break;
                        case AttributeControlType.ReadonlyCheckboxes:
                        case AttributeControlType.TextBox:
                        case AttributeControlType.MultilineTextbox:
                        case AttributeControlType.Datepicker:
                        case AttributeControlType.FileUpload:
                        default:
                            //these attribute types are supported as conditions
                            break;
                    }
                }
            }
            productAttributeMapping.ProductId = model.ProductId;
            productAttributeMapping.ConditionAttributeXml = attributesXml;
            await _productAttributeService.UpdateProductAttributeMapping(productAttributeMapping);
        }
        public virtual async Task<IList<ProductModel.ProductAttributeValueModel>> PrepareProductAttributeValueModels(Product product, ProductAttributeMapping productAttributeMapping)
        {
            var items = new List<ProductModel.ProductAttributeValueModel>();
            foreach (var x in productAttributeMapping.ProductAttributeValues.OrderBy(x => x.DisplayOrder))
            {
                Product associatedProduct = null;
                if (x.AttributeValueType == AttributeValueType.AssociatedToProduct)
                {
                    associatedProduct = await _productService.GetProductById(x.AssociatedProductId);
                }

                var pictureThumbnailUrl = await _pictureService.GetPictureUrl(string.IsNullOrEmpty(x.PictureId) ? x.ImageSquaresPictureId : x.PictureId, 75, false);

                //little hack here. Grid is rendered wrong way with <inmg> without "src" attribute
                if (string.IsNullOrEmpty(pictureThumbnailUrl))
                    pictureThumbnailUrl = await _pictureService.GetPictureUrl("", 1, true);
                items.Add(new ProductModel.ProductAttributeValueModel {
                    Id = x.Id,
                    ProductAttributeMappingId = x.ProductAttributeMappingId,
                    AttributeValueTypeId = x.AttributeValueTypeId,
                    AttributeValueTypeName = x.AttributeValueType.GetLocalizedEnum(_localizationService, _workContext),
                    AssociatedProductId = x.AssociatedProductId,
                    AssociatedProductName = associatedProduct != null ? associatedProduct.Name : "",
                    Name = productAttributeMapping.AttributeControlType != AttributeControlType.ColorSquares ? x.Name : string.Format("{0} - {1}", x.Name, x.ColorSquaresRgb),
                    ColorSquaresRgb = x.ColorSquaresRgb,
                    ImageSquaresPictureId = x.ImageSquaresPictureId,
                    PriceAdjustment = x.PriceAdjustment,
                    PriceAdjustmentStr = x.PriceAdjustment.ToString("G29"),
                    WeightAdjustment = x.WeightAdjustment,
                    WeightAdjustmentStr = x.AttributeValueType == AttributeValueType.Simple ? x.WeightAdjustment.ToString("G29") : "",
                    Cost = x.Cost,
                    Quantity = x.Quantity,
                    IsPreSelected = x.IsPreSelected,
                    DisplayOrder = x.DisplayOrder,
                    PictureId = x.PictureId,
                    PictureThumbnailUrl = pictureThumbnailUrl,
                    ProductId = product.Id,
                });
            }
            return items;
        }
        public virtual async Task<ProductModel.ProductAttributeValueModel> PrepareProductAttributeValueModel(ProductAttributeMapping pa, ProductAttributeValue pav)
        {
            var associatedProduct = await _productService.GetProductById(pav.AssociatedProductId);

            var model = new ProductModel.ProductAttributeValueModel {
                ProductAttributeMappingId = pav.ProductAttributeMappingId,
                AttributeValueTypeId = pav.AttributeValueTypeId,
                AttributeValueTypeName = pav.AttributeValueType.GetLocalizedEnum(_localizationService, _workContext),
                AssociatedProductId = pav.AssociatedProductId,
                AssociatedProductName = associatedProduct != null ? associatedProduct.Name : "",
                Name = pav.Name,
                ColorSquaresRgb = pav.ColorSquaresRgb,
                DisplayColorSquaresRgb = pa.AttributeControlType == AttributeControlType.ColorSquares,
                ImageSquaresPictureId = pav.ImageSquaresPictureId,
                DisplayImageSquaresPicture = pa.AttributeControlType == AttributeControlType.ImageSquares,
                PriceAdjustment = pav.PriceAdjustment,
                WeightAdjustment = pav.WeightAdjustment,
                Cost = pav.Cost,
                Quantity = pav.Quantity,
                IsPreSelected = pav.IsPreSelected,
                DisplayOrder = pav.DisplayOrder,
                PictureId = pav.PictureId
            };
            if (model.DisplayColorSquaresRgb && string.IsNullOrEmpty(model.ColorSquaresRgb))
            {
                model.ColorSquaresRgb = "#000000";
            }
            return model;
        }
        public virtual async Task InsertProductAttributeValueModel(ProductModel.ProductAttributeValueModel model)
        {
            var pav = new ProductAttributeValue {
                ProductAttributeMappingId = model.ProductAttributeMappingId,
                AttributeValueTypeId = model.AttributeValueTypeId,
                AssociatedProductId = model.AssociatedProductId,
                ProductId = model.ProductId,
                Name = model.Name,
                ColorSquaresRgb = model.ColorSquaresRgb,
                ImageSquaresPictureId = model.ImageSquaresPictureId,
                PriceAdjustment = model.PriceAdjustment,
                WeightAdjustment = model.WeightAdjustment,
                Cost = model.Cost,
                Quantity = model.Quantity,
                IsPreSelected = model.IsPreSelected,
                DisplayOrder = model.DisplayOrder,
                PictureId = model.PictureId,
            };
            pav.Locales = model.Locales.ToLocalizedProperty();
            await _productAttributeService.InsertProductAttributeValue(pav);
        }
        public virtual async Task UpdateProductAttributeValueModel(ProductAttributeValue pav, ProductModel.ProductAttributeValueModel model)
        {
            pav.AttributeValueTypeId = model.AttributeValueTypeId;
            pav.AssociatedProductId = model.AssociatedProductId;
            pav.Name = model.Name;
            pav.ProductId = model.ProductId;
            pav.ProductAttributeMappingId = model.ProductAttributeMappingId;
            pav.ColorSquaresRgb = model.ColorSquaresRgb;
            pav.ImageSquaresPictureId = model.ImageSquaresPictureId;
            pav.PriceAdjustment = model.PriceAdjustment;
            pav.WeightAdjustment = model.WeightAdjustment;
            pav.Cost = model.Cost;
            pav.Quantity = model.Quantity;
            pav.IsPreSelected = model.IsPreSelected;
            pav.DisplayOrder = model.DisplayOrder;
            pav.PictureId = model.PictureId;
            pav.Locales = model.Locales.ToLocalizedProperty();

            await _productAttributeService.UpdateProductAttributeValue(pav);
        }
        public virtual async Task<ProductModel.ProductAttributeValueModel.AssociateProductToAttributeValueModel> PrepareAssociateProductToAttributeValueModel()
        {
            var model = new ProductModel.ProductAttributeValueModel.AssociateProductToAttributeValueModel {
                //a vendor should have access only to his products
                IsLoggedInAsVendor = _workContext.CurrentVendor != null
            };
            var storeId = string.Empty;

            if (_workContext.CurrentCustomer.IsStaff())
                storeId = _workContext.CurrentCustomer.StaffStoreId;

            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            var categories = await _categoryService.GetAllCategories(showHidden: true, storeId: storeId);
            foreach (var c in categories)
                model.AvailableCategories.Add(new SelectListItem { Text = _categoryService.GetFormattedBreadCrumb(c, categories), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var m in await _manufacturerService.GetAllManufacturers(showHidden: true, storeId: storeId))
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
        public virtual async Task<IList<ProductModel.ProductAttributeCombinationModel>> PrepareProductAttributeCombinationModel(Product product)
        {
            var shoppingCartService = _serviceProvider.GetRequiredService<IShoppingCartService>();
            var items = new List<ProductModel.ProductAttributeCombinationModel>();

            foreach (var x in product.ProductAttributeCombinations)
            {
                var attributesXml = await _productAttributeFormatter.FormatAttributes((await _productService.GetProductById(product.Id)), x.AttributesXml, _workContext.CurrentCustomer, "<br />", true, true, true, false, true, true);
                var pacModel = new ProductModel.ProductAttributeCombinationModel {
                    Id = x.Id,
                    ProductId = product.Id,
                    AttributesXml = string.IsNullOrEmpty(attributesXml) ? "(null)" : attributesXml,
                    StockQuantity = product.UseMultipleWarehouses ? x.WarehouseInventory.Sum(y => y.StockQuantity - y.ReservedQuantity) : x.StockQuantity,
                    AllowOutOfStockOrders = x.AllowOutOfStockOrders,
                    Sku = x.Sku,
                    ManufacturerPartNumber = x.ManufacturerPartNumber,
                    Gtin = x.Gtin,
                    OverriddenPrice = x.OverriddenPrice,
                    NotifyAdminForQuantityBelow = x.NotifyAdminForQuantityBelow
                };
                //warnings
                var warnings = await shoppingCartService.GetShoppingCartItemAttributeWarnings(_workContext.CurrentCustomer,
                    ShoppingCartType.ShoppingCart, await _productService.GetProductById(product.Id), 1, x.AttributesXml, true);
                for (var i = 0; i < warnings.Count; i++)
                {
                    pacModel.Warnings += warnings[i];
                    if (i != warnings.Count - 1)
                        pacModel.Warnings += "<br />";
                }
                items.Add(pacModel);
            }

            return items;
        }

        public virtual async Task<ProductAttributeCombinationModel> PrepareProductAttributeCombinationModel(Product product, string combinationId)
        {
            var model = new ProductAttributeCombinationModel();
            var wim = new List<ProductAttributeCombinationModel.WarehouseInventoryModel>();
            foreach (var warehouse in await _shippingService.GetAllWarehouses())
            {
                var pwiModel = new ProductAttributeCombinationModel.WarehouseInventoryModel {
                    WarehouseId = warehouse.Id,
                    WarehouseName = warehouse.Name
                };
                wim.Add(pwiModel);
            }
            if (product.UseMultipleWarehouses)
            {
                model.UseMultipleWarehouses = product.UseMultipleWarehouses;
                model.WarehouseInventoryModels = wim;
            }

            if (!string.IsNullOrEmpty(combinationId))
            {
                var combination = product.ProductAttributeCombinations.FirstOrDefault(x => x.Id == combinationId);
                if (combination != null)
                {
                    model = combination.ToModel();
                    model.UseMultipleWarehouses = product.UseMultipleWarehouses;
                    model.WarehouseInventoryModels = wim;
                    model.ProductId = product.Id;
                    model.AttributesXML = await _productAttributeFormatter.FormatAttributes(product, combination.AttributesXml, _workContext.CurrentCustomer, "<br />", true, true, true, false);
                    if (model.UseMultipleWarehouses)
                    {
                        foreach (var _winv in combination.WarehouseInventory)
                        {
                            var warehouseInventoryModel = model.WarehouseInventoryModels.FirstOrDefault(x => x.WarehouseId == _winv.WarehouseId);
                            if (warehouseInventoryModel != null)
                            {
                                warehouseInventoryModel.WarehouseUsed = true;
                                warehouseInventoryModel.Id = _winv.Id;
                                warehouseInventoryModel.StockQuantity = _winv.StockQuantity;
                                warehouseInventoryModel.ReservedQuantity = _winv.ReservedQuantity;
                                warehouseInventoryModel.PlannedQuantity = await _shipmentService.GetQuantityInShipments(product, combination.AttributesXml, _winv.WarehouseId, true, true);
                            }
                        }
                    }
                }
            }
            return model;
        }
        public virtual async Task<IList<string>> InsertOrUpdateProductAttributeCombinationPopup(Product product, ProductAttributeCombinationModel model, Dictionary<string, string> form)
        {
            var attributesXml = "";
            var warnings = new List<string>();
            var shoppingCartService = _serviceProvider.GetRequiredService<IShoppingCartService>();
            async Task PrepareCombinationWarehouseInventory(ProductAttributeCombination combination)
            {
                var warehouses = await _shippingService.GetAllWarehouses();

                foreach (var warehouse in warehouses)
                {
                    var whim = model.WarehouseInventoryModels.FirstOrDefault(x => x.WarehouseId == warehouse.Id);
                    var existingPwI = combination.WarehouseInventory.FirstOrDefault(x => x.WarehouseId == warehouse.Id);
                    if (existingPwI != null)
                    {
                        if (whim.WarehouseUsed)
                        {
                            //update 
                            existingPwI.StockQuantity = whim.StockQuantity;
                            existingPwI.ReservedQuantity = whim.ReservedQuantity;
                        }
                        else
                        {
                            //delete 
                            combination.WarehouseInventory.Remove(existingPwI);
                        }
                    }
                    else
                    {
                        if (whim.WarehouseUsed)
                        {
                            //no need to insert a record for qty 0
                            existingPwI = new ProductCombinationWarehouseInventory {
                                WarehouseId = whim.WarehouseId,
                                StockQuantity = whim.StockQuantity,
                                ReservedQuantity = whim.ReservedQuantity
                            };
                            combination.WarehouseInventory.Add(existingPwI);
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(model.Id))
            {
                #region Product attributes

                var attributes = product.ProductAttributeMappings
                    .Where(x => !x.IsNonCombinable())
                    .ToList();
                foreach (var attribute in attributes)
                {
                    attribute.ProductId = product.Id;
                    var controlId = string.Format("product_attribute_{0}", attribute.Id);
                    switch (attribute.AttributeControlType)
                    {
                        case AttributeControlType.DropdownList:
                        case AttributeControlType.RadioList:
                        case AttributeControlType.ColorSquares:
                        case AttributeControlType.ImageSquares:
                            {
                                var ctrlAttributes = form[controlId];
                                if (!string.IsNullOrEmpty(ctrlAttributes))
                                {
                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, ctrlAttributes);
                                }
                            }
                            break;
                        case AttributeControlType.Checkboxes:
                            {
                                var cblAttributes = form[controlId];
                                if (!string.IsNullOrEmpty(cblAttributes))
                                {
                                    foreach (var item in cblAttributes.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                    {
                                        if (!string.IsNullOrEmpty(item))
                                            attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                                attribute, item);
                                    }
                                }
                            }
                            break;
                        case AttributeControlType.ReadonlyCheckboxes:
                            {
                                //load read-only (already server-side selected) values
                                var attributeValues = attribute.ProductAttributeValues;
                                foreach (var selectedAttributeId in attributeValues
                                    .Where(v => v.IsPreSelected)
                                    .Select(v => v.Id)
                                    .ToList())
                                {
                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, selectedAttributeId);
                                }
                            }
                            break;
                        case AttributeControlType.TextBox:
                        case AttributeControlType.MultilineTextbox:
                            {
                                var ctrlAttributes = form[controlId];
                                if (!string.IsNullOrEmpty(ctrlAttributes))
                                {
                                    var enteredText = ctrlAttributes.ToString().Trim();
                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, enteredText);
                                }
                            }
                            break;
                        case AttributeControlType.Datepicker:
                            {
                                var date = form[controlId + "_day"];
                                var month = form[controlId + "_month"];
                                var year = form[controlId + "_year"];
                                DateTime? selectedDate = null;
                                try
                                {
                                    selectedDate = new DateTime(int.Parse(year), int.Parse(month), int.Parse(date));
                                }
                                catch { }
                                if (selectedDate.HasValue)
                                {
                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, selectedDate.Value.ToString("D"));
                                }
                            }
                            break;
                        case AttributeControlType.FileUpload:
                            {
                                Guid.TryParse(form[controlId], out Guid downloadGuid);
                                var download = await _downloadService.GetDownloadByGuid(downloadGuid);
                                if (download != null)
                                {
                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                            attribute, download.DownloadGuid.ToString());
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }

                //validate conditional attributes (if specified)
                foreach (var attribute in attributes)
                {
                    attribute.ProductId = product.Id;
                    var conditionMet = _productAttributeParser.IsConditionMet(product, attribute, attributesXml);
                    if (conditionMet.HasValue && !conditionMet.Value)
                    {
                        attributesXml = _productAttributeParser.RemoveProductAttribute(attributesXml, attribute);
                    }
                }


                #endregion

                warnings.AddRange(await shoppingCartService.GetShoppingCartItemAttributeWarnings(_workContext.CurrentCustomer,
                    ShoppingCartType.ShoppingCart, product, 1, attributesXml, true));
                if (product.ProductAttributeCombinations.Where(x => x.AttributesXml == attributesXml).Count() > 0)
                {
                    warnings.Add("This combination attributes exists!");
                }
                if (warnings.Count == 0)
                {
                    var combination = new ProductAttributeCombination {
                        ProductId = product.Id,
                        AttributesXml = attributesXml,
                        StockQuantity = model.StockQuantity,
                        AllowOutOfStockOrders = model.AllowOutOfStockOrders,
                        Sku = model.Sku,
                        Text = model.Text,
                        ManufacturerPartNumber = model.ManufacturerPartNumber,
                        Gtin = model.Gtin,
                        OverriddenPrice = model.OverriddenPrice,
                        NotifyAdminForQuantityBelow = model.NotifyAdminForQuantityBelow,
                        PictureId = model.PictureId
                    };

                    if (product.UseMultipleWarehouses)
                    {
                        await PrepareCombinationWarehouseInventory(combination);
                        combination.StockQuantity = combination.WarehouseInventory.Sum(x => x.StockQuantity);
                    }
                    await _productAttributeService.InsertProductAttributeCombination(combination);

                    if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
                    {
                        product.StockQuantity = product.ProductAttributeCombinations.Sum(x => x.StockQuantity);
                        await _productService.UpdateStockProduct(product, false);
                    }
                }
            }
            else
            {
                var combination = product.ProductAttributeCombinations.FirstOrDefault(x => x.Id == model.Id);
                combination.StockQuantity = model.StockQuantity;
                combination.AllowOutOfStockOrders = model.AllowOutOfStockOrders;
                combination.Sku = model.Sku;
                combination.Text = model.Text;
                combination.ManufacturerPartNumber = model.ManufacturerPartNumber;
                combination.Gtin = model.Gtin;
                combination.OverriddenPrice = model.OverriddenPrice;
                combination.NotifyAdminForQuantityBelow = model.NotifyAdminForQuantityBelow;
                combination.ProductId = product.Id;
                combination.PictureId = model.PictureId;

                if (product.UseMultipleWarehouses)
                {
                    await PrepareCombinationWarehouseInventory(combination);
                    combination.StockQuantity = combination.WarehouseInventory.Sum(x => x.StockQuantity);
                }

                //notification - back in stock
                await BackInStockNotifications(combination);

                //update combination
                await _productAttributeService.UpdateProductAttributeCombination(combination);

                if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
                {
                    var pr = await _productService.GetProductById(model.ProductId);
                    pr.StockQuantity = pr.ProductAttributeCombinations.Sum(x => x.StockQuantity);
                    await _productService.UpdateStockProduct(pr, false);
                }
            }
            return warnings;
        }
        public virtual async Task GenerateAllAttributeCombinations(Product product)
        {
            var shoppingCartService = _serviceProvider.GetRequiredService<IShoppingCartService>();
            var allAttributesXml = _productAttributeParser.GenerateAllCombinations(product, true);
            var id = 1;
            foreach (var attributesXml in allAttributesXml)
            {
                var existingCombination = _productAttributeParser.FindProductAttributeCombination(product, attributesXml);

                //already exists?
                if (existingCombination != null)
                    continue;

                //new one
                var warnings = new List<string>();
                warnings.AddRange(await shoppingCartService.GetShoppingCartItemAttributeWarnings(_workContext.CurrentCustomer,
                    ShoppingCartType.ShoppingCart, product, 1, attributesXml, true));
                if (warnings.Count != 0)
                    continue;

                //save combination
                var combination = new ProductAttributeCombination {
                    ProductId = product.Id,
                    AttributesXml = attributesXml,
                    StockQuantity = 10000,
                    AllowOutOfStockOrders = false,
                    Sku = null,
                    ManufacturerPartNumber = null,
                    Gtin = null,
                    OverriddenPrice = null,
                    NotifyAdminForQuantityBelow = 1,
                };
                await _productAttributeService.InsertProductAttributeCombination(combination);
                id++;
            }

            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
            {
                product.StockQuantity = product.ProductAttributeCombinations.Sum(x => x.StockQuantity);
                await _productService.UpdateStockProduct(product, false);
            }
        }
        public virtual async Task<IList<ProductModel.ProductAttributeCombinationTierPricesModel>> PrepareProductAttributeCombinationTierPricesModel(Product product, string productAttributeCombinationId)
        {
            var items = new List<ProductModel.ProductAttributeCombinationTierPricesModel>();
            foreach (var x in product.ProductAttributeCombinations.Where(x => x.Id == productAttributeCombinationId).SelectMany(x => x.TierPrices))
            {
                string storeName;
                if (!string.IsNullOrEmpty(x.StoreId))
                {
                    var store = await _storeService.GetStoreById(x.StoreId);
                    storeName = store != null ? store.Shortcut : "Deleted";
                }
                else
                {
                    storeName = _localizationService.GetResource("Admin.Catalog.Products.TierPrices.Fields.Store.All");
                }

                var priceModel = new ProductModel.ProductAttributeCombinationTierPricesModel {
                    Id = x.Id,
                    CustomerRoleId = x.CustomerRoleId,
                    CustomerRole = !string.IsNullOrEmpty(x.CustomerRoleId) ? (await _customerService.GetCustomerRoleById(x.CustomerRoleId)).Name : _localizationService.GetResource("Admin.Catalog.Products.TierPrices.Fields.CustomerRole.All"),
                    StoreId = x.StoreId,
                    Store = storeName,
                    Price = x.Price,
                    Quantity = x.Quantity
                };
                items.Add(priceModel);
            }
            return items;
        }
        public virtual async Task InsertProductAttributeCombinationTierPricesModel(Product product, ProductAttributeCombination productAttributeCombination, ProductModel.ProductAttributeCombinationTierPricesModel model)
        {
            if (!string.IsNullOrEmpty(model.CustomerRoleId))
                model.CustomerRoleId = model.CustomerRoleId.Trim();
            else
                model.CustomerRoleId = "";

            if (!string.IsNullOrEmpty(model.StoreId))
                model.StoreId = model.StoreId.Trim();
            else
                model.StoreId = "";

            if (productAttributeCombination != null)
            {
                var pctp = new ProductCombinationTierPrices {
                    Price = model.Price,
                    Quantity = model.Quantity,
                    StoreId = model.StoreId,
                    CustomerRoleId = model.CustomerRoleId
                };
                productAttributeCombination.TierPrices.Add(pctp);
                productAttributeCombination.ProductId = product.Id;
                await _productAttributeService.UpdateProductAttributeCombination(productAttributeCombination);
            }
        }
        public virtual async Task UpdateProductAttributeCombinationTierPricesModel(Product product, ProductAttributeCombination productAttributeCombination, ProductModel.ProductAttributeCombinationTierPricesModel model)
        {
            if (!string.IsNullOrEmpty(model.CustomerRoleId))
                model.CustomerRoleId = model.CustomerRoleId.Trim();
            else
                model.CustomerRoleId = "";

            if (!string.IsNullOrEmpty(model.StoreId))
                model.StoreId = model.StoreId.Trim();
            else
                model.StoreId = "";

            if (productAttributeCombination != null)
            {
                var tierPrice = productAttributeCombination.TierPrices.FirstOrDefault(x => x.Id == model.Id);
                if (tierPrice != null)
                {
                    tierPrice.Price = model.Price;
                    tierPrice.Quantity = model.Quantity;
                    tierPrice.StoreId = model.StoreId;
                    tierPrice.CustomerRoleId = model.CustomerRoleId;
                    productAttributeCombination.ProductId = product.Id;
                    await _productAttributeService.UpdateProductAttributeCombination(productAttributeCombination);
                }
            }
        }
        public virtual async Task DeleteProductAttributeCombinationTierPrices(Product product, ProductAttributeCombination productAttributeCombination, ProductCombinationTierPrices tierPrice)
        {
            productAttributeCombination.TierPrices.Remove(tierPrice);
            productAttributeCombination.ProductId = product.Id;
            await _productAttributeService.UpdateProductAttributeCombination(productAttributeCombination);
        }

        //Pictures
        public virtual async Task<IList<ProductModel.ProductPictureModel>> PrepareProductPictureModel(Product product)
        {
            var items = new List<ProductModel.ProductPictureModel>();
            foreach (var x in product.ProductPictures.OrderBy(x => x.DisplayOrder))
            {

                var picture = await _pictureService.GetPictureById(x.PictureId);
                var m = new ProductModel.ProductPictureModel {
                    Id = x.Id,
                    ProductId = product.Id,
                    PictureId = x.PictureId,
                    PictureUrl = picture != null ? await _pictureService.GetPictureUrl(picture) : null,
                    OverrideAltAttribute = picture?.AltAttribute,
                    OverrideTitleAttribute = picture?.TitleAttribute,
                    DisplayOrder = x.DisplayOrder
                };
                items.Add(m);
            }
            return items;
        }
        public virtual async Task InsertProductPicture(Product product, string pictureId, int displayOrder, string overrideAltAttribute, string overrideTitleAttribute)
        {
            var picture = await _pictureService.GetPictureById(pictureId);
            if (picture == null)
                throw new ArgumentException("No picture found with the specified id");

            if (product.ProductPictures.Where(x => x.PictureId == pictureId).Count() > 0)
                return;

            await _pictureService.UpdatePicture(picture.Id,
                await _pictureService.LoadPictureBinary(picture),
                picture.MimeType,
                picture.SeoFilename,
                overrideAltAttribute,
                overrideTitleAttribute);

            await _productService.InsertProductPicture(new ProductPicture {
                PictureId = pictureId,
                ProductId = product.Id,
                DisplayOrder = displayOrder,
                AltAttribute = overrideAltAttribute,
                MimeType = picture.MimeType,
                SeoFilename = picture.SeoFilename,
                TitleAttribute = overrideTitleAttribute
            });

            await _pictureService.SetSeoFilename(pictureId, _pictureService.GetPictureSeName(product.Name));
        }
        public virtual async Task UpdateProductPicture(ProductModel.ProductPictureModel model)
        {
            var product = await _productService.GetProductById(model.ProductId, true);

            var productPicture = product.ProductPictures.Where(x => x.Id == model.Id).FirstOrDefault();
            if (productPicture == null)
                throw new ArgumentException("No product picture found with the specified id");
            productPicture.ProductId = product.Id;
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {

                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    throw new ArgumentException("This is not your product");
                }
            }

            var picture = await _pictureService.GetPictureById(productPicture.PictureId);
            if (picture == null)
                throw new ArgumentException("No picture found with the specified id");


            productPicture.DisplayOrder = model.DisplayOrder;
            productPicture.MimeType = picture.MimeType;
            productPicture.SeoFilename = picture.SeoFilename;
            productPicture.AltAttribute = model.OverrideAltAttribute;
            productPicture.TitleAttribute = model.OverrideTitleAttribute;

            await _productService.UpdateProductPicture(productPicture);

            await _pictureService.UpdatePicture(picture.Id,
                await _pictureService.LoadPictureBinary(picture),
                picture.MimeType,
                picture.SeoFilename,
                model.OverrideAltAttribute,
                model.OverrideTitleAttribute);
        }
        public virtual async Task DeleteProductPicture(ProductModel.ProductPictureModel model)
        {
            var product = await _productService.GetProductById(model.ProductId, true);

            var productPicture = product.ProductPictures.Where(x => x.Id == model.Id).FirstOrDefault();
            if (productPicture == null)
                throw new ArgumentException("No product picture found with the specified id");
            productPicture.ProductId = product.Id;

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    throw new ArgumentException("This is not your product");
                }
            }
            var pictureId = productPicture.PictureId;
            await _productService.DeleteProductPicture(productPicture);

            var picture = await _pictureService.GetPictureById(pictureId);
            if (picture != null)
                await _pictureService.DeletePicture(picture);
        }
        //Product specification
        public virtual async Task<IList<ProductSpecificationAttributeModel>> PrepareProductSpecificationAttributeModel(Product product)
        {
            var items = new List<ProductSpecificationAttributeModel>();
            foreach (var x in product.ProductSpecificationAttributes.OrderBy(x => x.DisplayOrder))
            {
                var specificationAttribute = await _specificationAttributeService.GetSpecificationAttributeById(x.SpecificationAttributeId);
                var psaModel = new ProductSpecificationAttributeModel {
                    Id = x.Id,
                    AttributeTypeId = (int)x.AttributeType,
                    ProductSpecificationId = specificationAttribute.Id,
                    AttributeId = x.SpecificationAttributeId,
                    ProductId = product.Id,
                    AttributeTypeName = x.AttributeType.GetLocalizedEnum(_localizationService, _workContext),
                    AttributeName = specificationAttribute.Name,
                    AllowFiltering = x.AllowFiltering,
                    ShowOnProductPage = x.ShowOnProductPage,
                    DisplayOrder = x.DisplayOrder
                };
                switch (x.AttributeType)
                {
                    case SpecificationAttributeType.Option:
                        psaModel.ValueRaw = System.Net.WebUtility.HtmlEncode(specificationAttribute.SpecificationAttributeOptions.Where(y => y.Id == x.SpecificationAttributeOptionId).FirstOrDefault()?.Name);
                        psaModel.SpecificationAttributeOptionId = x.SpecificationAttributeOptionId;
                        break;
                    case SpecificationAttributeType.CustomText:
                        psaModel.ValueRaw = System.Net.WebUtility.HtmlEncode(x.CustomValue);
                        break;
                    case SpecificationAttributeType.CustomHtmlText:
                        //do not encode?
                        psaModel.ValueRaw = System.Net.WebUtility.HtmlEncode(x.CustomValue);
                        break;
                    case SpecificationAttributeType.Hyperlink:
                        psaModel.ValueRaw = x.CustomValue;
                        break;
                    default:
                        break;
                }
                items.Add(psaModel);
            }
            return items;
        }
        public virtual async Task InsertProductSpecificationAttributeModel(ProductModel.AddProductSpecificationAttributeModel model, Product product)
        {
            //we allow filtering only for "Option" attribute type
            if (model.AttributeTypeId != (int)SpecificationAttributeType.Option)
            {
                model.AllowFiltering = false;
                model.SpecificationAttributeOptionId = null;
            }

            var psa = new ProductSpecificationAttribute {
                AttributeTypeId = model.AttributeTypeId,
                SpecificationAttributeOptionId = model.SpecificationAttributeOptionId,
                SpecificationAttributeId = model.SpecificationAttributeId,
                ProductId = product.Id,
                CustomValue = model.CustomValue,
                AllowFiltering = model.AllowFiltering,
                ShowOnProductPage = model.ShowOnProductPage,
                DisplayOrder = model.DisplayOrder,
            };

            await _specificationAttributeService.InsertProductSpecificationAttribute(psa);
            product.ProductSpecificationAttributes.Add(psa);
        }
        public virtual async Task UpdateProductSpecificationAttributeModel(Product product, ProductSpecificationAttribute psa, ProductSpecificationAttributeModel model)
        {
            if (model.AttributeTypeId == (int)SpecificationAttributeType.Option)
            {
                psa.AllowFiltering = model.AllowFiltering;
                psa.SpecificationAttributeOptionId = model.SpecificationAttributeOptionId;
            }
            else
            {
                psa.CustomValue = model.ValueRaw;
            }
            psa.ShowOnProductPage = model.ShowOnProductPage;
            psa.DisplayOrder = model.DisplayOrder;
            psa.ProductId = model.ProductId;
            await _specificationAttributeService.UpdateProductSpecificationAttribute(psa);
        }
        public virtual async Task DeleteProductSpecificationAttribute(Product product, ProductSpecificationAttribute psa)
        {
            psa.ProductId = product.Id;
            product.ProductSpecificationAttributes.Remove(psa);
            await _specificationAttributeService.DeleteProductSpecificationAttribute(psa);
        }
    }
}
