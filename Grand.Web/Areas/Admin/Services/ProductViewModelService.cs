using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Discounts;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Tax;
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
using Grand.Web.Areas.Admin.Infrastructure.Cache;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Web.Areas.Admin.Models.Orders;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        private readonly ICacheManager _cacheManager;
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
               ICacheManager cacheManager,
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
            _cacheManager = cacheManager;
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
            _currencySettings = currencySettings;
            _measureSettings = measureSettings;
            _taxSettings = taxSettings;
        }
        protected virtual void UpdatePictureSeoNames(Product product)
        {
            foreach (var pp in product.ProductPictures)
                _pictureService.SetSeoFilename(pp.PictureId, _pictureService.GetPictureSeName(product.Name));
        }
        protected virtual List<string> GetChildCategoryIds(string parentCategoryId)
        {
            var categoriesIds = new List<string>();
            var categories = _categoryService.GetAllCategoriesByParentCategoryId(parentCategoryId, true);
            foreach (var category in categories)
            {
                categoriesIds.Add(category.Id);
                categoriesIds.AddRange(GetChildCategoryIds(category.Id));
            }
            return categoriesIds;
        }
        protected virtual string[] ParseProductTags(string productTags)
        {
            var result = new List<string>();
            if (!String.IsNullOrWhiteSpace(productTags))
            {
                string[] values = productTags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string val1 in values)
                    if (!String.IsNullOrEmpty(val1.Trim()))
                        result.Add(val1.Trim());
            }
            return result.ToArray();
        }
        protected virtual void SaveProductTags(Product product, string[] productTags)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            //product tags
            var existingProductTags = product.ProductTags.ToList();
            var productTagsToRemove = new List<ProductTag>();
            foreach (var existingProductTag in existingProductTags)
            {
                var existingProductTagText = _productTagService.GetProductTagByName(existingProductTag.ToLowerInvariant());
                bool found = false;
                foreach (string newProductTag in productTags)
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
                productTag.ProductId = product.Id;
                _productService.DeleteProductTag(productTag);
            }
            foreach (string productTagName in productTags)
            {
                ProductTag productTag;
                var productTag2 = _productTagService.GetProductTagByName(productTagName);
                if (productTag2 == null)
                {
                    //add new product tag
                    productTag = new ProductTag
                    {
                        Name = productTagName,
                        SeName = SeoExtensions.GetSeName(productTagName),
                        Count = 0,
                    };
                    _productTagService.InsertProductTag(productTag);
                }
                else
                {
                    productTag = productTag2;
                }
                if (!product.ProductTagExists(productTag.Name))
                {
                    productTag.ProductId = product.Id;
                    _productService.InsertProductTag(productTag);
                }
            }
        }
        public virtual void PrepareAddProductAttributeCombinationModel(ProductAttributeCombinationModel model, Product product)
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
                    var productAttribute = _productAttributeService.GetProductAttributeById(attribute.ProductAttributeId);
                    var attributeModel = new ProductAttributeCombinationModel.ProductAttributeModel
                    {
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
                            var attributeValueModel = new ProductAttributeCombinationModel.ProductAttributeValueModel
                            {
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
                var pictureThumbnailUrl = _pictureService.GetPictureUrl(model.PictureId, 75, false);
                model.PictureThumbnailUrl = pictureThumbnailUrl;
            }

            //pictures
            model.ProductPictureModels = product.ProductPictures.Select(picture => new ProductModel.ProductPictureModel
            {
                Id = picture.Id,
                ProductId = product.Id,
                PictureId = picture.PictureId,
                PictureUrl = _pictureService.GetPictureUrl(picture.PictureId),
                DisplayOrder = picture.DisplayOrder
            }).ToList();
        }
        public virtual void PrepareTierPriceModel(ProductModel.TierPriceModel model)
        {
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var store in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = store.Name, Value = store.Id.ToString() });

            //customer roles
            model.AvailableCustomerRoles.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var role in _customerService.GetAllCustomerRoles(true))
                model.AvailableCustomerRoles.Add(new SelectListItem { Text = role.Name, Value = role.Id.ToString() });
        }
        public virtual void PrepareProductAttributeValueModel(Product product, ProductModel.ProductAttributeValueModel model)
        {
            //pictures
            model.ProductPictureModels = product.ProductPictures.OrderBy(x => x.DisplayOrder)
                .Select(x => new ProductModel.ProductPictureModel
                {
                    Id = x.Id,
                    ProductId = product.Id,
                    PictureId = x.PictureId,
                    PictureUrl = _pictureService.GetPictureUrl(x.PictureId),
                    DisplayOrder = x.DisplayOrder
                })
                .ToList();

            var associatedProduct = _productService.GetProductById(model.AssociatedProductId);
            model.AssociatedProductName = associatedProduct != null ? associatedProduct.Name : "";
        }
        public virtual void BackInStockNotifications(Product product, ProductModel model, int prevStockQuantity,
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
                _backInStockSubscriptionService.SendNotificationsToSubscribers(product, "");
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
                                _backInStockSubscriptionService.SendNotificationsToSubscribers(product, prevstock.WarehouseId);
                        }
                    }
                }
                if (product.ProductWarehouseInventory.Sum(x => x.StockQuantity - x.ReservedQuantity) > 0)
                {
                    if (prevMultiWarehouseStock.Sum(x => x.StockQuantity - x.ReservedQuantity) <= 0)
                    {
                        _backInStockSubscriptionService.SendNotificationsToSubscribers(product, "");
                    }
                }
            }
        }
        public virtual void PrepareProductModel(ProductModel model, Product product,
            bool setPredefinedValues, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
            model.BaseWeightIn = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).Name;
            model.BaseDimensionIn = _measureService.GetMeasureDimensionById(_measureSettings.BaseDimensionId).Name;

            if (product != null)
            {
                //date
                model.CreatedOn = _dateTimeHelper.ConvertToUserTime(product.CreatedOnUtc, DateTimeKind.Utc);
                model.UpdatedOn = _dateTimeHelper.ConvertToUserTime(product.UpdatedOnUtc, DateTimeKind.Utc);

                //parent grouped product
                var parentGroupedProduct = _productService.GetProductById(product.ParentGroupedProductId);
                if (parentGroupedProduct != null)
                {
                    model.AssociatedToProductId = product.ParentGroupedProductId;
                    model.AssociatedToProductName = parentGroupedProduct.Name;
                }

                //reservation
                model.CalendarModel.Interval = product.Interval;
                model.CalendarModel.IntervalUnit = product.IntervalUnitId;
                model.CalendarModel.IncBothDate = product.IncBothDate;

                //product attributes
                foreach (var productAttribute in _productAttributeService.GetAllProductAttributes())
                {
                    model.AvailableProductAttributes.Add(new SelectListItem
                    {
                        Text = productAttribute.Name,
                        Value = productAttribute.Id.ToString()
                    });
                }

                //manufacturers
                foreach (var manufacturer in _manufacturerService.GetAllManufacturers(showHidden: true))
                {
                    model.AvailableManufacturers.Add(new SelectListItem
                    {
                        Text = manufacturer.Name,
                        Value = manufacturer.Id.ToString()
                    });
                }
                //categories
                var allCategories = _categoryService.GetAllCategories(showHidden: true);
                foreach (var category in allCategories)
                {
                    model.AvailableCategories.Add(new SelectListItem
                    {
                        Text = category.GetFormattedBreadCrumb(allCategories),
                        Value = category.Id.ToString()
                    });
                }


                //specification attributes
                model.AddSpecificationAttributeModel.AvailableAttributes = _cacheManager
                    .Get(ModelCacheEventConsumer.SPEC_ATTRIBUTES_MODEL_KEY, () =>
                    {
                        var availableSpecificationAttributes = new List<SelectListItem>();
                        foreach (var sa in _specificationAttributeService.GetSpecificationAttributes())
                        {
                            availableSpecificationAttributes.Add(new SelectListItem
                            {
                                Text = sa.Name,
                                Value = sa.Id.ToString()
                            });
                        }
                        return availableSpecificationAttributes;
                    });

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
            var templates = _productTemplateService.GetAllProductTemplates();
            foreach (var template in templates)
            {
                model.AvailableProductTemplates.Add(new SelectListItem
                {
                    Text = template.Name,
                    Value = template.Id.ToString()
                });
            }

            //vendors
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            model.AvailableVendors.Add(new SelectListItem
            {
                Text = _localizationService.GetResource("Admin.Catalog.Products.Fields.Vendor.None"),
                Value = ""
            });
            var vendors = _vendorService.GetAllVendors(showHidden: true);
            foreach (var vendor in vendors)
            {
                model.AvailableVendors.Add(new SelectListItem
                {
                    Text = vendor.Name,
                    Value = vendor.Id.ToString()
                });
            }

            //delivery dates
            model.AvailableDeliveryDates.Add(new SelectListItem
            {
                Text = _localizationService.GetResource("Admin.Catalog.Products.Fields.DeliveryDate.None"),
                Value = ""
            });
            var deliveryDates = _shippingService.GetAllDeliveryDates();
            foreach (var deliveryDate in deliveryDates)
            {
                model.AvailableDeliveryDates.Add(new SelectListItem
                {
                    Text = deliveryDate.Name,
                    Value = deliveryDate.Id.ToString()
                });
            }

            //warehouses
            var warehouses = _shippingService.GetAllWarehouses();
            model.AvailableWarehouses.Add(new SelectListItem
            {
                Text = _localizationService.GetResource("Admin.Catalog.Products.Fields.Warehouse.None"),
                Value = ""
            });
            foreach (var warehouse in warehouses)
            {
                model.AvailableWarehouses.Add(new SelectListItem
                {
                    Text = warehouse.Name,
                    Value = warehouse.Id.ToString()
                });
            }

            //multiple warehouses
            foreach (var warehouse in warehouses)
            {
                var pwiModel = new ProductModel.ProductWarehouseInventoryModel
                {
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
                        pwiModel.PlannedQuantity = _shipmentService.GetQuantityInShipments(product, null, pwi.WarehouseId, true, true);
                    }
                }
                model.ProductWarehouseInventoryModels.Add(pwiModel);
            }

            //product tags
            if (product != null)
            {
                var result = new StringBuilder();
                for (int i = 0; i < product.ProductTags.Count; i++)
                {
                    var pt = product.ProductTags.ToList()[i];
                    var productTag = _productTagService.GetProductTagByName(pt);
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
            var taxCategories = _taxCategoryService.GetAllTaxCategories();
            model.AvailableTaxCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Configuration.Settings.Tax.TaxCategories.None"), Value = "" });
            foreach (var tc in taxCategories)
                model.AvailableTaxCategories.Add(new SelectListItem { Text = tc.Name, Value = tc.Id.ToString(), Selected = product != null && !setPredefinedValues && tc.Id == product.TaxCategoryId });

            //baseprice units
            var measureWeights = _measureService.GetAllMeasureWeights();
            foreach (var mw in measureWeights)
                model.AvailableBasepriceUnits.Add(new SelectListItem { Text = mw.Name, Value = mw.Id.ToString(), Selected = product != null && !setPredefinedValues && mw.Id == product.BasepriceUnitId });
            foreach (var mw in measureWeights)
                model.AvailableBasepriceBaseUnits.Add(new SelectListItem { Text = mw.Name, Value = mw.Id.ToString(), Selected = product != null && !setPredefinedValues && mw.Id == product.BasepriceBaseUnitId });

            //units
            var units = _measureService.GetAllMeasureUnits();
            model.AvailableUnits.Add(new SelectListItem { Text = "---", Value = "" });
            foreach (var un in units)
                model.AvailableUnits.Add(new SelectListItem { Text = un.Name, Value = un.Id.ToString(), Selected = product != null && un.Id == product.UnitId });

            //default specs values
            model.AddSpecificationAttributeModel.ShowOnProductPage = true;

            //discounts
            model.AvailableDiscounts = _discountService
                .GetAllDiscounts(DiscountType.AssignedToSkus, showHidden: true)
                .Select(d => d.ToModel())
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
        public virtual (IEnumerable<OrderModel> orderModels, int totalCount) PrepareOrderModel(string productId, int pageIndex, int pageSize)
        {
            var orderService = Grand.Core.Infrastructure.EngineContext.Current.Resolve<IOrderService>();
            var orders = orderService.SearchOrders(
                            productId: productId,
                            pageIndex: pageIndex - 1,
                            pageSize: pageSize);
            return (orders.Select(x =>
            {
                var store = _storeService.GetStoreById(x.StoreId);
                return new OrderModel
                {
                    Id = x.Id,
                    OrderNumber = x.OrderNumber,
                    StoreName = store != null ? store.Name : "Unknown",
                    OrderStatus = x.OrderStatus.GetLocalizedEnum(_localizationService, _workContext),
                    PaymentStatus = x.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext),
                    ShippingStatus = x.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext),
                    CustomerEmail = x.BillingAddress?.Email,
                    CustomerId = x.CustomerId,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc)
                };
            }), orders.TotalCount);
        }
        public virtual void SaveProductWarehouseInventory(Product product, IList<ProductModel.ProductWarehouseInventoryModel> model)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (product.ManageInventoryMethodId != (int)ManageInventoryMethod.ManageStock)
                return;

            if (!product.UseMultipleWarehouses)
                return;

            var warehouses = _shippingService.GetAllWarehouses();

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
                        _productService.UpdateProductWarehouseInventory(existingPwI);
                    }
                    else
                    {
                        //delete. no need to store record for qty 0
                        _productService.DeleteProductWarehouseInventory(existingPwI);
                    }
                }
                else
                {
                    if (whim.WarehouseUsed)
                    {
                        //no need to insert a record for qty 0
                        existingPwI = new ProductWarehouseInventory
                        {
                            WarehouseId = warehouse.Id,
                            ProductId = product.Id,
                            StockQuantity = whim.StockQuantity,
                            ReservedQuantity = whim.ReservedQuantity
                        };
                        product.ProductWarehouseInventory.Add(existingPwI);
                        _productService.InsertProductWarehouseInventory(existingPwI);
                    }
                }

            }
        }
        public virtual void PrepareProductReviewModel(ProductReviewModel model,
            ProductReview productReview, bool excludeProperties, bool formatReviewText)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (productReview == null)
                throw new ArgumentNullException("productReview");
            var product = _productService.GetProductById(productReview.ProductId);
            var customer = _customerService.GetCustomerById(productReview.CustomerId);
            var store = _storeService.GetStoreById(productReview.StoreId);
            model.Id = productReview.Id;
            model.StoreName = store != null ? store.Name : "";
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
        public virtual ProductListModel PrepareProductListModel()
        {
            var model = new ProductListModel();
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
            {
                if (_workContext.CurrentVendor != null && !string.IsNullOrEmpty(_workContext.CurrentVendor.StoreId))
                {
                    if (s.Id == _workContext.CurrentVendor.StoreId)
                        model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
                }
                else
                    model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
            }
            //warehouses
            model.AvailableWarehouses.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var wh in _shippingService.GetAllWarehouses())
                model.AvailableWarehouses.Add(new SelectListItem { Text = wh.Name, Value = wh.Id.ToString() });

            //vendors
            model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var v in _vendorService.GetAllVendors(showHidden: true))
                model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });

            //"published" property
            //0 - all (according to "ShowHidden" parameter)
            //1 - published only
            //2 - unpublished only
            model.AvailablePublishedOptions.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Products.List.SearchPublished.All"), Value = " " });
            model.AvailablePublishedOptions.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Products.List.SearchPublished.PublishedOnly"), Value = "1" });
            model.AvailablePublishedOptions.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Catalog.Products.List.SearchPublished.UnpublishedOnly"), Value = "2" });

            return model;
        }
        public virtual (IEnumerable<ProductModel> productModels, int totalCount) PrepareProductsModel(ProductListModel model, int pageIndex, int pageSize)
        {
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }

            var categoryIds = new List<string>();
            if (!String.IsNullOrEmpty(model.SearchCategoryId))
                categoryIds.Add(model.SearchCategoryId);

            //include subcategories
            if (model.SearchIncludeSubCategories && !String.IsNullOrEmpty(model.SearchCategoryId))
                categoryIds.AddRange(GetChildCategoryIds(model.SearchCategoryId));

            //0 - all (according to "ShowHidden" parameter)
            //1 - published only
            //2 - unpublished only
            bool? overridePublished = null;
            if (model.SearchPublishedId == 1)
                overridePublished = true;
            else if (model.SearchPublishedId == 2)
                overridePublished = false;

            var products = _productService.SearchProducts(
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
            );
            return (products.Select(x =>
            {
                var productModel = x.ToModel();
                //"Error during serialization or deserialization using the JSON JavaScriptSerializer. The length of the string exceeds the value set on the maxJsonLength property. "
                //also it improves performance
                productModel.FullDescription = "";

                //picture
                var defaultProductPicture = x.ProductPictures.FirstOrDefault();
                if (defaultProductPicture == null)
                    defaultProductPicture = new ProductPicture();
                productModel.PictureThumbnailUrl = _pictureService.GetPictureUrl(defaultProductPicture.PictureId, 75, true);
                //product type
                productModel.ProductTypeName = x.ProductType.GetLocalizedEnum(_localizationService, _workContext);
                //friendly stock qantity
                //if a simple product AND "manage inventory" is "Track inventory", then display
                if (x.ProductType == ProductType.SimpleProduct && x.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                    productModel.StockQuantityStr = x.GetTotalStockQuantity().ToString();
                return productModel;
            }), products.TotalCount);
        }
        public virtual IList<Product> PrepareProducts(ProductListModel model)
        {
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }

            var categoryIds = new List<string>();
            if (!String.IsNullOrEmpty(model.SearchCategoryId))
                categoryIds.Add(model.SearchCategoryId);

            //include subcategories
            if (model.SearchIncludeSubCategories && !String.IsNullOrEmpty(model.SearchCategoryId))
                categoryIds.AddRange(GetChildCategoryIds(model.SearchCategoryId));

            //0 - all (according to "ShowHidden" parameter)
            //1 - published only
            //2 - unpublished only
            bool? overridePublished = null;
            if (model.SearchPublishedId == 1)
                overridePublished = true;
            else if (model.SearchPublishedId == 2)
                overridePublished = false;

            var products = _productService.SearchProducts(
                categoryIds: categoryIds,
                manufacturerId: model.SearchManufacturerId,
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                warehouseId: model.SearchWarehouseId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                showHidden: true,
                overridePublished: overridePublished
            );

            return products;
        }
        public virtual Product InsertProductModel(ProductModel model)
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
            if (_workContext.CurrentVendor != null && model.ShowOnHomePage)
            {
                model.ShowOnHomePage = false;
            }

            //product
            var product = model.ToEntity();
            product.CreatedOnUtc = DateTime.UtcNow;
            product.UpdatedOnUtc = DateTime.UtcNow;

            _productService.InsertProduct(product);

            model.SeName = product.ValidateSeName(model.SeName, product.Name, true);
            product.SeName = model.SeName;
            product.Locales = model.Locales.ToLocalizedProperty(product, x => x.Name, _urlRecordService);

            //search engine name
            _urlRecordService.SaveSlug(product, model.SeName, "");
            //tags
            SaveProductTags(product, ParseProductTags(model.ProductTags));
            //warehouses
            SaveProductWarehouseInventory(product, model.ProductWarehouseInventoryModels);
            //discounts
            var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToSkus, showHidden: true);
            foreach (var discount in allDiscounts)
            {
                if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                {
                    product.AppliedDiscounts.Add(discount.Id);
                    _productService.InsertDiscount(discount.Id, product.Id);
                }
            }
            _productService.UpdateProduct(product);

            //activity log
            _customerActivityService.InsertActivity("AddNewProduct", product.Id, _localizationService.GetResource("ActivityLog.AddNewProduct"), product.Name);

            return product;
        }
        public virtual Product UpdateProductModel(Product product, ProductModel model)
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
            var prevStockQuantity = product.GetTotalStockQuantity();
            var prevMultiWarehouseStock = product.ProductWarehouseInventory.Select(i => new ProductWarehouseInventory() { WarehouseId = i.WarehouseId, StockQuantity = i.StockQuantity, ReservedQuantity = i.ReservedQuantity }).ToList();

            string prevDownloadId = product.DownloadId;
            string prevSampleDownloadId = product.SampleDownloadId;

            //product
            product = model.ToEntity(product);
            product.UpdatedOnUtc = DateTime.UtcNow;

            model.SeName = product.ValidateSeName(model.SeName, product.Name, true);
            product.SeName = model.SeName;
            product.Locales = model.Locales.ToLocalizedProperty(product, x => x.Name, _urlRecordService);

            //search engine name
            _urlRecordService.SaveSlug(product, model.SeName, "");
            //tags
            SaveProductTags(product, ParseProductTags(model.ProductTags));
            //warehouses
            SaveProductWarehouseInventory(product, model.ProductWarehouseInventoryModels);
            //picture seo names
            UpdatePictureSeoNames(product);
            //discounts
            var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToSkus, showHidden: true);
            foreach (var discount in allDiscounts)
            {
                if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                {
                    //new discount
                    if (product.AppliedDiscounts.Count(d => d == discount.Id) == 0)
                    {
                        product.AppliedDiscounts.Add(discount.Id);
                        _productService.InsertDiscount(discount.Id, product.Id);
                    }
                }
                else
                {
                    //remove discount
                    if (product.AppliedDiscounts.Count(d => d == discount.Id) > 0)
                    {
                        product.AppliedDiscounts.Remove(discount.Id);
                        _productService.DeleteDiscount(discount.Id, product.Id);
                    }
                }
            }
            _productService.UpdateProduct(product);

            //back in stock notifications
            BackInStockNotifications(product, model, prevStockQuantity, prevMultiWarehouseStock);

            //delete an old "download" file (if deleted or updated)
            if (!String.IsNullOrEmpty(prevDownloadId) && prevDownloadId != product.DownloadId)
            {
                var prevDownload = _downloadService.GetDownloadById(prevDownloadId);
                if (prevDownload != null)
                    _downloadService.DeleteDownload(prevDownload);
            }
            //delete an old "sample download" file (if deleted or updated)
            if (!String.IsNullOrEmpty(prevSampleDownloadId) && prevSampleDownloadId != product.SampleDownloadId)
            {
                var prevSampleDownload = _downloadService.GetDownloadById(prevSampleDownloadId);
                if (prevSampleDownload != null)
                    _downloadService.DeleteDownload(prevSampleDownload);
            }
            //activity log
            _customerActivityService.InsertActivity("EditProduct", product.Id, _localizationService.GetResource("ActivityLog.EditProduct"), product.Name);
            return product;
        }
        public virtual void DeleteProduct(Product product)
        {
            _productService.DeleteProduct(product);
            //activity log
            _customerActivityService.InsertActivity("DeleteProduct", product.Id, _localizationService.GetResource("ActivityLog.DeleteProduct"), product.Name);
        }
        public virtual void DeleteSelected(IList<string> selectedIds)
        {
            var products = new List<Product>();
            products.AddRange(_productService.GetProductsByIds(selectedIds.ToArray()));
            for (int i = 0; i < products.Count; i++)
            {
                var product = products[i];
                //a vendor should have access only to his products
                if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                    continue;
                _productService.DeleteProduct(product);
            }
        }
        public virtual ProductModel.AddRequiredProductModel PrepareAddRequiredProductModel()
        {
            var model = new ProductModel.AddRequiredProductModel();
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
        public virtual (IList<ProductModel> products, int totalCount) PrepareProductModel(ProductModel.AddRequiredProductModel model, int pageIndex, int pageSize)
        {
            var products = _productService.PrepareProductList(model.SearchCategoryId, model.SearchManufacturerId, model.SearchStoreId, model.SearchVendorId, model.SearchProductTypeId, model.SearchProductName, pageIndex, pageSize);
            return (products.Select(x => x.ToModel()).ToList(), products.TotalCount);
        }
        public virtual (IList<ProductModel> products, int totalCount) PrepareProductModel(ProductModel.AddRelatedProductModel model, int pageIndex, int pageSize)
        {
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }
            var products = _productService.PrepareProductList(model.SearchCategoryId, model.SearchManufacturerId, model.SearchStoreId, model.SearchVendorId, model.SearchProductTypeId, model.SearchProductName, pageIndex, pageSize);
            return (products.Select(x => x.ToModel()).ToList(), products.TotalCount);
        }
        public virtual (IList<ProductModel> products, int totalCount) PrepareProductModel(ProductModel.AddBundleProductModel model, int pageIndex, int pageSize)
        {
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }
            var products = _productService.PrepareProductList(model.SearchCategoryId, model.SearchManufacturerId, model.SearchStoreId, model.SearchVendorId, (int)ProductType.SimpleProduct, model.SearchProductName, pageIndex, pageSize);
            return (products.Select(x => x.ToModel()).ToList(), products.TotalCount);
        }
        public virtual (IList<ProductModel> products, int totalCount) PrepareProductModel(ProductModel.AddCrossSellProductModel model, int pageIndex, int pageSize)
        {
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }
            var products = _productService.PrepareProductList(model.SearchCategoryId, model.SearchManufacturerId, model.SearchStoreId, model.SearchVendorId, model.SearchProductTypeId, model.SearchProductName, pageIndex, pageSize);
            return (products.Select(x => x.ToModel()).ToList(), products.TotalCount);
        }
        public virtual (IList<ProductModel> products, int totalCount) PrepareProductModel(ProductModel.AddAssociatedProductModel model, int pageIndex, int pageSize)
        {
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }
            var products = _productService.PrepareProductList(model.SearchCategoryId, model.SearchManufacturerId, model.SearchStoreId, model.SearchVendorId, model.SearchProductTypeId, model.SearchProductName, pageIndex, pageSize);
            return (products.Select(x => x.ToModel()).ToList(), products.TotalCount);
        }
        public virtual (IList<ProductModel> products, int totalCount) PrepareProductModel(ProductModel.ProductAttributeValueModel.AssociateProductToAttributeValueModel model, int pageIndex, int pageSize)
        {
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }
            var products = _productService.PrepareProductList(model.SearchCategoryId, model.SearchManufacturerId, model.SearchStoreId, model.SearchVendorId, (int)ProductType.SimpleProduct, model.SearchProductName, pageIndex, pageSize);
            return (products.Select(x => x.ToModel()).ToList(), products.TotalCount);
        }
        public virtual IList<ProductModel.ProductCategoryModel> PrepareProductCategoryModel(Product product)
        {
            var productCategories = product.ProductCategories.OrderBy(x => x.DisplayOrder);
            return productCategories
                .Select(x => new ProductModel.ProductCategoryModel
                {
                    Id = x.Id,
                    Category = _categoryService.GetCategoryById(x.CategoryId)?.GetFormattedBreadCrumb(_categoryService),
                    ProductId = product.Id,
                    CategoryId = x.CategoryId,
                    IsFeaturedProduct = x.IsFeaturedProduct,
                    DisplayOrder = x.DisplayOrder
                })
                .ToList();
        }
        public virtual void InsertProductCategoryModel(ProductModel.ProductCategoryModel model)
        {
            var product = _productService.GetProductById(model.ProductId);
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
                var productCategory = new ProductCategory
                {
                    ProductId = model.ProductId,
                    CategoryId = model.CategoryId,
                    DisplayOrder = model.DisplayOrder,
                };
                //a vendor cannot edit "IsFeaturedProduct" property
                if (_workContext.CurrentVendor == null)
                {
                    productCategory.IsFeaturedProduct = model.IsFeaturedProduct;
                }
                _categoryService.InsertProductCategory(productCategory);
            }
        }
        public virtual void UpdateProductCategoryModel(ProductModel.ProductCategoryModel model)
        {
            var product = _productService.GetProductById(model.ProductId);
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
            _categoryService.UpdateProductCategory(productCategory);
        }
        public virtual void DeleteProductCategory(string id, string productId)
        {
            var product = _productService.GetProductById(productId);
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
            _categoryService.DeleteProductCategory(productCategory);
        }
        public virtual IList<ProductModel.ProductManufacturerModel> PrepareProductManufacturerModel(Product product)
        {
            return product.ProductManufacturers.OrderBy(x => x.DisplayOrder).Select(
                x => new ProductModel.ProductManufacturerModel
                {
                    Id = x.Id,
                    Manufacturer = _manufacturerService.GetManufacturerById(x.ManufacturerId).Name,
                    ProductId = product.Id,
                    ManufacturerId = x.ManufacturerId,
                    IsFeaturedProduct = x.IsFeaturedProduct,
                    DisplayOrder = x.DisplayOrder
                })
            .ToList();
        }
        public virtual void InsertProductManufacturer(ProductModel.ProductManufacturerModel model)
        {
            var manufacturerId = model.ManufacturerId;
            var product = _productService.GetProductById(model.ProductId);
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
                var productManufacturer = new ProductManufacturer
                {
                    ProductId = model.ProductId,
                    ManufacturerId = manufacturerId,
                    DisplayOrder = model.DisplayOrder,
                };
                //a vendor cannot edit "IsFeaturedProduct" property
                if (_workContext.CurrentVendor == null)
                {
                    productManufacturer.IsFeaturedProduct = model.IsFeaturedProduct;
                }
                _manufacturerService.InsertProductManufacturer(productManufacturer);
            }
        }
        public virtual void UpdateProductManufacturer(ProductModel.ProductManufacturerModel model)
        {
            var product = _productService.GetProductById(model.ProductId);
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
            _manufacturerService.UpdateProductManufacturer(productManufacturer);

        }
        public virtual void DeleteProductManufacturer(string id, string productId)
        {
            var product = _productService.GetProductById(productId);
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
            _manufacturerService.DeleteProductManufacturer(productManufacturer);
        }
        public virtual void InsertRelatedProductModel(ProductModel.AddRelatedProductModel model)
        {
            var productId1 = _productService.GetProductById(model.ProductId);

            foreach (string id in model.SelectedProductIds)
            {
                var product = _productService.GetProductById(id);
                if (product != null)
                {
                    //a vendor should have access only to his products
                    if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                        continue;

                    var existingRelatedProducts = productId1.RelatedProducts;
                    if (model.ProductId != id)
                        if (existingRelatedProducts.Where(x => x.ProductId2 == id).Count() == 0)
                        {
                            var related = new RelatedProduct
                            {
                                ProductId1 = model.ProductId,
                                ProductId2 = id,
                                DisplayOrder = 1,
                            };
                            productId1.RelatedProducts.Add(related);
                            _productService.InsertRelatedProduct(related);
                        }
                }
            }
        }
        public virtual void UpdateRelatedProductModel(ProductModel.RelatedProductModel model)
        {
            var product1 = _productService.GetProductById(model.ProductId1);
            var relatedProduct = product1.RelatedProducts.Where(x => x.Id == model.Id).FirstOrDefault();
            if (relatedProduct == null)
                throw new ArgumentException("No related product found with the specified id");

            var product2 = _productService.GetProductById(relatedProduct.ProductId2);
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
            _productService.UpdateRelatedProduct(relatedProduct);

        }
        public virtual void DeleteRelatedProductModel(ProductModel.RelatedProductModel model)
        {
            var product = _productService.GetProductById(model.ProductId1);
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
            _productService.DeleteRelatedProduct(relatedProduct);
        }
        public virtual void InsertBundleProductModel(ProductModel.AddBundleProductModel model)
        {
            var productId1 = _productService.GetProductById(model.ProductId);

            foreach (string id in model.SelectedProductIds)
            {
                var product = _productService.GetProductById(id);
                if (product != null)
                {
                    //a vendor should have access only to his products
                    if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                        continue;

                    var existingBundleProducts = productId1.BundleProducts;
                    if (model.ProductId != id)
                        if (existingBundleProducts.Where(x => x.ProductId == id).Count() == 0)
                        {
                            var bundle = new BundleProduct
                            {
                                ProductBundleId = model.ProductId,
                                ProductId = id,
                                DisplayOrder = 1,
                                Quantity = 1,
                            };
                            productId1.BundleProducts.Add(bundle);
                            _productService.InsertBundleProduct(bundle);
                        }
                }
            }
        }
        public virtual void UpdateBundleProductModel(ProductModel.BundleProductModel model)
        {
            var product = _productService.GetProductById(model.ProductBundleId);
            var bundleProduct = product.BundleProducts.Where(x => x.Id == model.Id).FirstOrDefault();
            if (bundleProduct == null)
                throw new ArgumentException("No bundle product found with the specified id");

            var product2 = _productService.GetProductById(bundleProduct.ProductId);
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
            _productService.UpdateBundleProduct(bundleProduct);
        }
        public virtual void DeleteBundleProductModel(ProductModel.BundleProductModel model)
        {
            var product = _productService.GetProductById(model.ProductBundleId);
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
            _productService.DeleteBundleProduct(bundleProduct);
        }
        public virtual void InsertCrossSellProductModel(ProductModel.AddCrossSellProductModel model)
        {
            foreach (string id in model.SelectedProductIds)
            {
                var product = _productService.GetProductById(id);
                if (product != null)
                {
                    //a vendor should have access only to his products
                    if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                        continue;

                    if (product.CrossSellProduct.Where(x => x == model.ProductId).Count() == 0)
                    {
                        if (model.ProductId != id)
                            _productService.InsertCrossSellProduct(
                                new CrossSellProduct
                                {
                                    ProductId1 = model.ProductId,
                                    ProductId2 = id,
                                });
                    }
                }
            }
        }
        public virtual void DeleteCrossSellProduct(string productId, string crossSellProductId)
        {
            CrossSellProduct crosssell = new CrossSellProduct()
            {
                ProductId1 = productId,
                ProductId2 = crossSellProductId
            };
            _productService.DeleteCrossSellProduct(crosssell);
        }
        public virtual void InsertAssociatedProductModel(ProductModel.AddAssociatedProductModel model)
        {
            foreach (string id in model.SelectedProductIds)
            {
                var product = _productService.GetProductById(id);
                if (product != null)
                {
                    //a vendor should have access only to his products
                    if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                        continue;

                    product.ParentGroupedProductId = model.ProductId;
                    _productService.UpdateAssociatedProduct(product);
                }
            }
        }
        public virtual void DeleteAssociatedProduct(Product product)
        {
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                throw new ArgumentException("This is not your product");

            product.ParentGroupedProductId = "";
            _productService.UpdateAssociatedProduct(product);
        }
        public virtual ProductModel.AddRelatedProductModel PrepareRelatedProductModel()
        {
            var model = new ProductModel.AddRelatedProductModel();
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
        public virtual ProductModel.AddBundleProductModel PrepareBundleProductModel()
        {
            var model = new ProductModel.AddBundleProductModel();
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

            return model;
        }
        public virtual ProductModel.AddCrossSellProductModel PrepareCrossSellProductModel()
        {
            var model = new ProductModel.AddCrossSellProductModel();
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
        public virtual ProductModel.AddAssociatedProductModel PrepareAssociatedProductModel()
        {
            var model = new ProductModel.AddAssociatedProductModel();
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
        public virtual BulkEditListModel PrepareBulkEditListModel()
        {
            var model = new BulkEditListModel();
            //categories
            model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            var categories = _categoryService.GetAllCategories(showHidden: true);
            foreach (var c in categories)
                model.AvailableCategories.Add(new SelectListItem { Text = c.GetFormattedBreadCrumb(categories), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var m in _manufacturerService.GetAllManufacturers(showHidden: true))
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });

            //product types
            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
            model.AvailableProductTypes.Insert(0, new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });

            return model;
        }
        public virtual (IEnumerable<BulkEditProductModel> bulkEditProductModels, int totalCount) PrepareBulkEditProductModel(BulkEditListModel model, int pageIndex, int pageSize)
        {
            string vendorId = "";
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
                vendorId = _workContext.CurrentVendor.Id;

            var searchCategoryIds = new List<string>();
            if (!String.IsNullOrEmpty(model.SearchCategoryId))
                searchCategoryIds.Add(model.SearchCategoryId);

            var products = _productService.SearchProducts(categoryIds: searchCategoryIds,
                manufacturerId: model.SearchManufacturerId,
                vendorId: vendorId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                pageIndex: pageIndex - 1,
                pageSize: pageSize,
                showHidden: true);

            return (products.Select(x =>
            {
                var productModel = new BulkEditProductModel
                {
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
        public virtual void UpdateBulkEdit(IList<BulkEditProductModel> products)
        {
            foreach (var pModel in products)
            {
                //update
                var product = _productService.GetProductById(pModel.Id);
                if (product != null)
                {
                    //a vendor should have access only to his products
                    if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                        continue;

                    var prevStockQuantity = product.GetTotalStockQuantity();

                    product.Sku = pModel.Sku;
                    product.Price = pModel.Price;
                    product.OldPrice = pModel.OldPrice;
                    product.StockQuantity = pModel.StockQuantity;
                    product.Published = pModel.Published;
                    product.Name = pModel.Name;
                    _productService.UpdateProduct(product);

                    //back in stock notifications
                    if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                        product.BackorderMode == BackorderMode.NoBackorders &&
                        product.AllowBackInStockSubscriptions &&
                        product.GetTotalStockQuantity() > 0 &&
                        prevStockQuantity <= 0 && !product.UseMultipleWarehouses &&
                        product.Published)
                    {
                        _backInStockSubscriptionService.SendNotificationsToSubscribers(product, "");
                    }
                }
            }

        }
        public virtual void DeleteBulkEdit(IList<BulkEditProductModel> products)
        {
            foreach (var pModel in products)
            {
                //delete
                var product = _productService.GetProductById(pModel.Id);
                if (product != null)
                {
                    //a vendor should have access only to his products
                    if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                        continue;

                    _productService.DeleteProduct(product);
                }
            }
        }
        public virtual IList<ProductModel.TierPriceModel> PrepareTierPriceModel(Product product)
        {
            var tierPricesModel = product.TierPrices.OrderBy(x => x.StoreId).ThenBy(x => x.Quantity).ThenBy(x => x.CustomerRoleId).Select(x =>
            {
                string storeName;
                if (!string.IsNullOrEmpty(x.StoreId))
                {
                    var store = _storeService.GetStoreById(x.StoreId);
                    storeName = store != null ? store.Name : "Deleted";
                }
                else
                    storeName = _localizationService.GetResource("Admin.Catalog.Products.TierPrices.Fields.Store.All");

                return new ProductModel.TierPriceModel
                {
                    Id = x.Id,
                    StoreId = x.StoreId,
                    Store = storeName,
                    CustomerRole = !string.IsNullOrEmpty(x.CustomerRoleId) ? _customerService.GetCustomerRoleById(x.CustomerRoleId).Name : _localizationService.GetResource("Admin.Catalog.Products.TierPrices.Fields.CustomerRole.All"),
                    ProductId = product.Id,
                    CustomerRoleId = !string.IsNullOrEmpty(x.CustomerRoleId) ? x.CustomerRoleId : "",
                    Quantity = x.Quantity,
                    Price = x.Price,
                    StartDateTime = x.StartDateTimeUtc.HasValue ? _dateTimeHelper.ConvertToUserTime(x.StartDateTimeUtc.Value, DateTimeKind.Utc) : new DateTime?(),
                    EndDateTime = x.EndDateTimeUtc.HasValue ? _dateTimeHelper.ConvertToUserTime(x.EndDateTimeUtc.Value, DateTimeKind.Utc) : new DateTime?()
                };
            }).ToList();
            return tierPricesModel;
        }
        public virtual (IEnumerable<ProductModel.BidModel> bidModels, int totalCount) PrepareBidMode(string productId, int pageIndex, int pageSize)
        {
            var auctionService = Grand.Core.Infrastructure.EngineContext.Current.Resolve<IAuctionService>();
            var priceFormatter = Grand.Core.Infrastructure.EngineContext.Current.Resolve<IPriceFormatter>();
            var bids = auctionService.GetBidsByProductId(productId, pageIndex - 1, pageSize);
            var bidsModel = bids
                .Select(x => new ProductModel.BidModel
                {
                    BidId = x.Id,
                    Amount = priceFormatter.FormatPrice(x.Amount),
                    Date = _dateTimeHelper.ConvertToUserTime(x.Date, DateTimeKind.Utc),
                    CustomerId = x.CustomerId,
                    Email = _customerService.GetCustomerById(x.CustomerId)?.Email,
                    OrderId = x.OrderId
                }).ToList();

            return (bidsModel, bids.TotalCount);
        }
        public virtual (IEnumerable<ProductModel.ActivityLogModel> activityLogModels, int totalCount) PrepareActivityLogModel(string productId, int pageIndex, int pageSize)
        {
            var activityLog = _customerActivityService.GetProductActivities(null, null, productId, pageIndex - 1, pageSize);
            return (activityLog.Select(x =>
            {
                var customer = _customerService.GetCustomerById(x.CustomerId);
                var m = new ProductModel.ActivityLogModel
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
        public virtual ProductModel.ProductAttributeMappingModel PrepareProductAttributeMappingModel(Product product)
        {
            var model = new ProductModel.ProductAttributeMappingModel();
            model.ProductId = product.Id;
            foreach (var attribute in _productAttributeService.GetAllProductAttributes())
            {
                model.AvailableProductAttribute.Add(new SelectListItem()
                {
                    Value = attribute.Id,
                    Text = attribute.Name
                });
            }
            return model;
        }
        public virtual ProductModel.ProductAttributeMappingModel PrepareProductAttributeMappingModel(Product product, ProductAttributeMapping productAttributeMapping)
        {
            var model = productAttributeMapping.ToModel();
            foreach (var attribute in _productAttributeService.GetAllProductAttributes())
            {
                model.AvailableProductAttribute.Add(new SelectListItem()
                {
                    Value = attribute.Id,
                    Text = attribute.Name,
                    Selected = attribute.Id == model.ProductAttributeId
                });
            }
            return model;
        }
        public virtual ProductModel.ProductAttributeMappingModel PrepareProductAttributeMappingModel(ProductModel.ProductAttributeMappingModel model)
        {
            foreach (var attribute in _productAttributeService.GetAllProductAttributes())
            {
                model.AvailableProductAttribute.Add(new SelectListItem()
                {
                    Value = attribute.Id,
                    Text = attribute.Name
                });
            }
            return model;
        }
        public virtual IList<ProductModel.ProductAttributeMappingModel> PrepareProductAttributeMappingModels(Product product)
        {
            return product.ProductAttributeMappings.OrderBy(x => x.DisplayOrder)
                .Select(x =>
                {
                    var attributeModel = new ProductModel.ProductAttributeMappingModel
                    {
                        Id = x.Id,
                        ProductId = product.Id,
                        ProductAttribute = _productAttributeService.GetProductAttributeById(x.ProductAttributeId)?.Name,
                        ProductAttributeId = x.ProductAttributeId,
                        TextPrompt = x.TextPrompt,
                        IsRequired = x.IsRequired,
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
                        var productAttribute = _productAttributeService.GetProductAttributeById(conditionAttribute.ProductAttributeId);
                        string _paname = productAttribute != null ? productAttribute.Name : "";
                        attributeModel.ConditionString = string.Format("{0}: {1}",
                            System.Net.WebUtility.HtmlEncode(_paname),
                            System.Net.WebUtility.HtmlEncode(conditionValue.Name));
                    }
                    else
                        attributeModel.ConditionString = string.Empty;
                    return attributeModel;
                })
                .ToList();
        }
        public virtual void InsertProductAttributeMappingModel(ProductModel.ProductAttributeMappingModel model)
        {
            //insert mapping
            var productAttributeMapping = model.ToEntity();
            //predefined values
            var predefinedValues = _productAttributeService.GetPredefinedProductAttributeValues(model.ProductAttributeId);
            foreach (var predefinedValue in predefinedValues)
            {
                var pav = predefinedValue.ToEntity();
                //locales
                pav.Locales.Clear();
                var languages = _languageService.GetAllLanguages(true);
                //localization
                foreach (var lang in languages)
                {
                    var name = predefinedValue.GetLocalized(x => x.Name, lang.Id, false, false);
                    if (!String.IsNullOrEmpty(name))
                        pav.Locales.Add(new LocalizedProperty() { LanguageId = lang.Id, LocaleKey = "Name", LocaleValue = name });
                }

                productAttributeMapping.ProductAttributeValues.Add(pav);
            }

            _productAttributeService.InsertProductAttributeMapping(productAttributeMapping);
        }
        public virtual void UpdateProductAttributeMappingModel(ProductModel.ProductAttributeMappingModel model)
        {
            var product = _productService.GetProductById(model.ProductId);
            if (product != null)
            {
                var productAttributeMapping = product.ProductAttributeMappings.Where(x => x.Id == model.Id).FirstOrDefault();
                if (productAttributeMapping != null)
                {
                    productAttributeMapping = model.ToEntity(productAttributeMapping);
                    _productAttributeService.UpdateProductAttributeMapping(productAttributeMapping);
                }
            }
        }
        public virtual ProductModel.ProductAttributeMappingModel PrepareProductAttributeMappingModel(ProductAttributeMapping productAttributeMapping)
        {
            var model = new ProductModel.ProductAttributeMappingModel
            {
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
            return model;
        }
        public virtual void UpdateProductAttributeValidationRulesModel(ProductAttributeMapping productAttributeMapping, ProductModel.ProductAttributeMappingModel model)
        {
            productAttributeMapping.ProductId = model.ProductId;
            productAttributeMapping.ValidationMinLength = model.ValidationMinLength;
            productAttributeMapping.ValidationMaxLength = model.ValidationMaxLength;
            productAttributeMapping.ValidationFileAllowedExtensions = model.ValidationFileAllowedExtensions;
            productAttributeMapping.ValidationFileMaximumSize = model.ValidationFileMaximumSize;
            productAttributeMapping.DefaultValue = model.DefaultValue;
            _productAttributeService.UpdateProductAttributeMapping(productAttributeMapping);
        }
        public virtual ProductAttributeConditionModel PrepareProductAttributeConditionModel(Product product, ProductAttributeMapping productAttributeMapping)
        {
            var model = new ProductAttributeConditionModel();
            model.ProductAttributeMappingId = productAttributeMapping.Id;
            model.EnableCondition = !String.IsNullOrEmpty(productAttributeMapping.ConditionAttributeXml);
            model.ProductId = product.Id;
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
                var pam = _productAttributeService.GetProductAttributeById(attribute.ProductAttributeId);
                var attributeModel = new ProductAttributeConditionModel.ProductAttributeModel
                {
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
                        var attributeValueModel = new ProductAttributeConditionModel.ProductAttributeValueModel
                        {
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
                                    if (!String.IsNullOrEmpty(productAttributeMapping.ConditionAttributeXml))
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
        public virtual void UpdateProductAttributeConditionModel(Product product, ProductAttributeMapping productAttributeMapping, ProductAttributeConditionModel model, Dictionary<string, string> form)
        {
            string attributesXml = null;
            if (model.EnableCondition)
            {
                var attribute = product.ProductAttributeMappings.FirstOrDefault(x => x.Id == model.SelectedProductAttributeId);
                if (attribute != null)
                {
                    string controlId = string.Format("product_attribute_{0}", attribute.Id);
                    switch (attribute.AttributeControlType)
                    {
                        case AttributeControlType.DropdownList:
                        case AttributeControlType.RadioList:
                        case AttributeControlType.ColorSquares:
                        case AttributeControlType.ImageSquares:
                            {
                                var ctrlAttributes = form[controlId];
                                if (!String.IsNullOrEmpty(ctrlAttributes))
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
                                if (!String.IsNullOrEmpty(cblAttributes))
                                {
                                    bool anyValueSelected = false;
                                    foreach (var item in cblAttributes.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                    {
                                        if (!String.IsNullOrEmpty(item))
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
            _productAttributeService.UpdateProductAttributeMapping(productAttributeMapping);
        }
        public virtual IList<ProductModel.ProductAttributeValueModel> PrepareProductAttributeValueModels(Product product, ProductAttributeMapping productAttributeMapping)
        {
            return productAttributeMapping.ProductAttributeValues.OrderBy(x => x.DisplayOrder).Select(x =>
            {
                Product associatedProduct = null;
                if (x.AttributeValueType == AttributeValueType.AssociatedToProduct)
                {
                    associatedProduct = _productService.GetProductById(x.AssociatedProductId);
                }

                var pictureThumbnailUrl = _pictureService.GetPictureUrl(string.IsNullOrEmpty(x.PictureId) ? x.ImageSquaresPictureId : x.PictureId, 75, false);

                //little hack here. Grid is rendered wrong way with <inmg> without "src" attribute
                if (String.IsNullOrEmpty(pictureThumbnailUrl))
                    pictureThumbnailUrl = _pictureService.GetPictureUrl("", 1, true);
                return new ProductModel.ProductAttributeValueModel
                {
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
                    PriceAdjustmentStr = x.AttributeValueType == AttributeValueType.Simple ? x.PriceAdjustment.ToString("G29") : "",
                    WeightAdjustment = x.WeightAdjustment,
                    WeightAdjustmentStr = x.AttributeValueType == AttributeValueType.Simple ? x.WeightAdjustment.ToString("G29") : "",
                    Cost = x.Cost,
                    Quantity = x.Quantity,
                    IsPreSelected = x.IsPreSelected,
                    DisplayOrder = x.DisplayOrder,
                    PictureId = x.PictureId,
                    PictureThumbnailUrl = pictureThumbnailUrl,
                    ProductId = product.Id,
                };
            }).ToList();
        }
        public virtual ProductModel.ProductAttributeValueModel PrepareProductAttributeValueModel(ProductAttributeMapping pa, ProductAttributeValue pav)
        {
            var associatedProduct = _productService.GetProductById(pav.AssociatedProductId);

            var model = new ProductModel.ProductAttributeValueModel
            {
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
            if (model.DisplayColorSquaresRgb && String.IsNullOrEmpty(model.ColorSquaresRgb))
            {
                model.ColorSquaresRgb = "#000000";
            }
            return model;
        }
        public virtual void InsertProductAttributeValueModel(ProductModel.ProductAttributeValueModel model)
        {
            var pav = new ProductAttributeValue
            {
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
            _productAttributeService.InsertProductAttributeValue(pav);
        }
        public virtual void UpdateProductAttributeValueModel(ProductAttributeValue pav, ProductModel.ProductAttributeValueModel model)
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

            _productAttributeService.UpdateProductAttributeValue(pav);
        }
        public virtual ProductModel.ProductAttributeValueModel.AssociateProductToAttributeValueModel PrepareAssociateProductToAttributeValueModel()
        {
            var model = new ProductModel.ProductAttributeValueModel.AssociateProductToAttributeValueModel();
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
        public virtual IList<ProductModel.ProductAttributeCombinationModel> PrepareProductAttributeCombinationModel(Product product)
        {
            var shoppingCartService = Grand.Core.Infrastructure.EngineContext.Current.Resolve<IShoppingCartService>();
            return product.ProductAttributeCombinations
                .Select(x =>
                {
                    var attributesXml = _productAttributeFormatter.FormatAttributes(_productService.GetProductById(product.Id), x.AttributesXml, _workContext.CurrentCustomer, "<br />", true, true, true, false, true, true);
                    var pacModel = new ProductModel.ProductAttributeCombinationModel
                    {
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
                    var warnings = shoppingCartService.GetShoppingCartItemAttributeWarnings(_workContext.CurrentCustomer,
                        ShoppingCartType.ShoppingCart, _productService.GetProductById(product.Id), 1, x.AttributesXml, true);
                    for (int i = 0; i < warnings.Count; i++)
                    {
                        pacModel.Warnings += warnings[i];
                        if (i != warnings.Count - 1)
                            pacModel.Warnings += "<br />";
                    }

                    return pacModel;
                })
                .ToList();
        }
        public virtual ProductAttributeCombinationModel PrepareProductAttributeCombinationModel(Product product, string combinationId)
        {
            var model = new ProductAttributeCombinationModel();
            var wim = new List<ProductAttributeCombinationModel.WarehouseInventoryModel>();
            foreach (var warehouse in _shippingService.GetAllWarehouses())
            {
                var pwiModel = new ProductAttributeCombinationModel.WarehouseInventoryModel
                {
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
                    model.AttributesXML = _productAttributeFormatter.FormatAttributes(product, combination.AttributesXml, _workContext.CurrentCustomer, "<br />", true, true, true, false);
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
                                warehouseInventoryModel.PlannedQuantity = _shipmentService.GetQuantityInShipments(product, combination.AttributesXml, _winv.WarehouseId, true, true);
                            }
                        }
                    }
                }
            }
            return model;
        }
        public virtual IList<string> InsertOrUpdateProductAttributeCombinationPopup(Product product, ProductAttributeCombinationModel model, Dictionary<string, string> form)
        {
            string attributesXml = "";
            var warnings = new List<string>();
            var shoppingCartService = Grand.Core.Infrastructure.EngineContext.Current.Resolve<IShoppingCartService>();
            void PrepareCombinationWarehouseInventory(ProductAttributeCombination combination)
            {
                var warehouses = _shippingService.GetAllWarehouses();

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
                            existingPwI = new ProductCombinationWarehouseInventory
                            {
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
                    string controlId = string.Format("product_attribute_{0}", attribute.Id);
                    switch (attribute.AttributeControlType)
                    {
                        case AttributeControlType.DropdownList:
                        case AttributeControlType.RadioList:
                        case AttributeControlType.ColorSquares:
                        case AttributeControlType.ImageSquares:
                            {
                                var ctrlAttributes = form[controlId];
                                if (!String.IsNullOrEmpty(ctrlAttributes))
                                {
                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, ctrlAttributes);
                                }
                            }
                            break;
                        case AttributeControlType.Checkboxes:
                            {
                                var cblAttributes = form[controlId];
                                if (!String.IsNullOrEmpty(cblAttributes))
                                {
                                    foreach (var item in cblAttributes.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                    {
                                        if (!String.IsNullOrEmpty(item))
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
                                if (!String.IsNullOrEmpty(ctrlAttributes))
                                {
                                    string enteredText = ctrlAttributes.ToString().Trim();
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
                                    selectedDate = new DateTime(Int32.Parse(year), Int32.Parse(month), Int32.Parse(date));
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
                                Guid downloadGuid;
                                Guid.TryParse(form[controlId], out downloadGuid);
                                var download = _downloadService.GetDownloadByGuid(downloadGuid);
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

                warnings.AddRange(shoppingCartService.GetShoppingCartItemAttributeWarnings(_workContext.CurrentCustomer,
                    ShoppingCartType.ShoppingCart, product, 1, attributesXml, true));
                if (product.ProductAttributeCombinations.Where(x => x.AttributesXml == attributesXml).Count() > 0)
                {
                    warnings.Add("This combination attributes exists!");
                }
                if (warnings.Count == 0)
                {
                    var combination = new ProductAttributeCombination
                    {
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
                        PrepareCombinationWarehouseInventory(combination);
                        combination.StockQuantity = combination.WarehouseInventory.Sum(x => x.StockQuantity);
                    }
                    _productAttributeService.InsertProductAttributeCombination(combination);

                    if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
                    {
                        product.StockQuantity = product.ProductAttributeCombinations.Sum(x => x.StockQuantity);
                        _productService.UpdateStockProduct(product);
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
                    PrepareCombinationWarehouseInventory(combination);
                    combination.StockQuantity = combination.WarehouseInventory.Sum(x => x.StockQuantity);
                }
                _productAttributeService.UpdateProductAttributeCombination(combination);

                if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
                {
                    var pr = _productService.GetProductById(model.ProductId);
                    pr.StockQuantity = pr.ProductAttributeCombinations.Sum(x => x.StockQuantity);
                    _productService.UpdateStockProduct(pr);
                }
            }
            return warnings;
        }
        public virtual void GenerateAllAttributeCombinations(Product product)
        {
            var shoppingCartService = Grand.Core.Infrastructure.EngineContext.Current.Resolve<IShoppingCartService>();
            var allAttributesXml = _productAttributeParser.GenerateAllCombinations(product, true);
            int id = 1;
            foreach (var attributesXml in allAttributesXml)
            {
                var existingCombination = _productAttributeParser.FindProductAttributeCombination(product, attributesXml);

                //already exists?
                if (existingCombination != null)
                    continue;

                //new one
                var warnings = new List<string>();
                warnings.AddRange(shoppingCartService.GetShoppingCartItemAttributeWarnings(_workContext.CurrentCustomer,
                    ShoppingCartType.ShoppingCart, product, 1, attributesXml, true));
                if (warnings.Count != 0)
                    continue;

                //save combination
                var combination = new ProductAttributeCombination
                {
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
                _productAttributeService.InsertProductAttributeCombination(combination);
                id++;
            }

            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
            {
                product.StockQuantity = product.ProductAttributeCombinations.Sum(x => x.StockQuantity);
                _productService.UpdateStockProduct(product);
            }

        }
        public virtual IList<ProductModel.ProductAttributeCombinationTierPricesModel> PrepareProductAttributeCombinationTierPricesModel(Product product, string productAttributeCombinationId)
        {
            var tierprices = product.ProductAttributeCombinations.Where(x => x.Id == productAttributeCombinationId).SelectMany(x => x.TierPrices);
            var tierPriceModel = tierprices
                .Select(x =>
                {
                    string storeName;
                    if (!String.IsNullOrEmpty(x.StoreId))
                    {
                        var store = _storeService.GetStoreById(x.StoreId);
                        storeName = store != null ? store.Name : "Deleted";
                    }
                    else
                    {
                        storeName = _localizationService.GetResource("Admin.Catalog.Products.TierPrices.Fields.Store.All");
                    }

                    var priceModel = new ProductModel.ProductAttributeCombinationTierPricesModel
                    {
                        Id = x.Id,
                        CustomerRoleId = x.CustomerRoleId,
                        CustomerRole = !String.IsNullOrEmpty(x.CustomerRoleId) ? _customerService.GetCustomerRoleById(x.CustomerRoleId).Name : _localizationService.GetResource("Admin.Catalog.Products.TierPrices.Fields.CustomerRole.All"),
                        StoreId = x.StoreId,
                        Store = storeName,
                        Price = x.Price,
                        Quantity = x.Quantity
                    };
                    return priceModel;
                })
                .ToList();
            return tierPriceModel;
        }
        public virtual void InsertProductAttributeCombinationTierPricesModel(Product product, ProductAttributeCombination productAttributeCombination, ProductModel.ProductAttributeCombinationTierPricesModel model)
        {
            if (!String.IsNullOrEmpty(model.CustomerRoleId))
                model.CustomerRoleId = model.CustomerRoleId.Trim();
            else
                model.CustomerRoleId = "";

            if (!String.IsNullOrEmpty(model.StoreId))
                model.StoreId = model.StoreId.Trim();
            else
                model.StoreId = "";

            if (productAttributeCombination != null)
            {
                ProductCombinationTierPrices pctp = new ProductCombinationTierPrices();
                pctp.Price = model.Price;
                pctp.Quantity = model.Quantity;
                pctp.StoreId = model.StoreId;
                pctp.CustomerRoleId = model.CustomerRoleId;
                productAttributeCombination.TierPrices.Add(pctp);
                productAttributeCombination.ProductId = product.Id;
                _productAttributeService.UpdateProductAttributeCombination(productAttributeCombination);
            }
        }
        public virtual void UpdateProductAttributeCombinationTierPricesModel(Product product, ProductAttributeCombination productAttributeCombination, ProductModel.ProductAttributeCombinationTierPricesModel model)
        {
            if (!String.IsNullOrEmpty(model.CustomerRoleId))
                model.CustomerRoleId = model.CustomerRoleId.Trim();
            else
                model.CustomerRoleId = "";

            if (!String.IsNullOrEmpty(model.StoreId))
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
                    _productAttributeService.UpdateProductAttributeCombination(productAttributeCombination);
                }
            }
        }
        public virtual void DeleteProductAttributeCombinationTierPrices(Product product, ProductAttributeCombination productAttributeCombination, ProductCombinationTierPrices tierPrice)
        {
            productAttributeCombination.TierPrices.Remove(tierPrice);
            productAttributeCombination.ProductId = product.Id;
            _productAttributeService.UpdateProductAttributeCombination(productAttributeCombination);
        }

        //Pictures
        public virtual IList<ProductModel.ProductPictureModel> PrepareProductPictureModel(Product product)
        {
            return product.ProductPictures.OrderBy(x => x.DisplayOrder).Select(x =>
            {
                var picture = _pictureService.GetPictureById(x.PictureId);
                var m = new ProductModel.ProductPictureModel
                {
                    Id = x.Id,
                    ProductId = product.Id,
                    PictureId = x.PictureId,
                    PictureUrl = picture != null ? _pictureService.GetPictureUrl(picture) : null,
                    OverrideAltAttribute = picture != null ? picture.AltAttribute : null,
                    OverrideTitleAttribute = picture != null ? picture.TitleAttribute : null,
                    DisplayOrder = x.DisplayOrder
                };
                return m;
            })
            .ToList();
        }
        public virtual void InsertProductPicture(Product product, string pictureId, int displayOrder, string overrideAltAttribute, string overrideTitleAttribute)
        {
            var picture = _pictureService.GetPictureById(pictureId);
            if (picture == null)
                throw new ArgumentException("No picture found with the specified id");

            if (product.ProductPictures.Where(x => x.PictureId == pictureId).Count() > 0)
                return;

            _pictureService.UpdatePicture(picture.Id,
                _pictureService.LoadPictureBinary(picture),
                picture.MimeType,
                picture.SeoFilename,
                overrideAltAttribute,
                overrideTitleAttribute);

            _productService.InsertProductPicture(new ProductPicture
            {
                PictureId = pictureId,
                ProductId = product.Id,
                DisplayOrder = displayOrder,
                AltAttribute = overrideAltAttribute,
                MimeType = picture.MimeType,
                SeoFilename = picture.SeoFilename,
                TitleAttribute = overrideTitleAttribute
            });

            _pictureService.SetSeoFilename(pictureId, _pictureService.GetPictureSeName(product.Name));
        }
        public virtual void UpdateProductPicture(ProductModel.ProductPictureModel model)
        {
            var product = _productService.GetProductById(model.ProductId);

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

            var picture = _pictureService.GetPictureById(productPicture.PictureId);
            if (picture == null)
                throw new ArgumentException("No picture found with the specified id");


            productPicture.DisplayOrder = model.DisplayOrder;
            productPicture.MimeType = picture.MimeType;
            productPicture.SeoFilename = picture.SeoFilename;
            productPicture.AltAttribute = model.OverrideAltAttribute;
            productPicture.TitleAttribute = model.OverrideTitleAttribute;

            _productService.UpdateProductPicture(productPicture);

            _pictureService.UpdatePicture(picture.Id,
                _pictureService.LoadPictureBinary(picture),
                picture.MimeType,
                picture.SeoFilename,
                model.OverrideAltAttribute,
                model.OverrideTitleAttribute);
        }
        public virtual void DeleteProductPicture(ProductModel.ProductPictureModel model)
        {
            var product = _productService.GetProductById(model.ProductId);

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
            _productService.DeleteProductPicture(productPicture);

            var picture = _pictureService.GetPictureById(pictureId);
            if (picture != null)
                _pictureService.DeletePicture(picture);
        }
        //Product specification
        public virtual IList<ProductSpecificationAttributeModel> PrepareProductSpecificationAttributeModel(Product product)
        {
            return product.ProductSpecificationAttributes.OrderBy(x => x.DisplayOrder).Select(x =>
            {
                var specificationAttribute = _specificationAttributeService.GetSpecificationAttributeById(x.SpecificationAttributeId);
                var psaModel = new ProductSpecificationAttributeModel
                {
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
                return psaModel;
            }).ToList();
        }
        public virtual void InsertProductSpecificationAttributeModel(ProductModel.AddProductSpecificationAttributeModel model, Product product)
        {
            //we allow filtering only for "Option" attribute type
            if (model.AttributeTypeId != (int)SpecificationAttributeType.Option)
            {
                model.AllowFiltering = false;
                model.SpecificationAttributeOptionId = null;
            }

            var psa = new ProductSpecificationAttribute
            {
                AttributeTypeId = model.AttributeTypeId,
                SpecificationAttributeOptionId = model.SpecificationAttributeOptionId,
                SpecificationAttributeId = model.SpecificationAttributeId,
                ProductId = product.Id,
                CustomValue = model.CustomValue,
                AllowFiltering = model.AllowFiltering,
                ShowOnProductPage = model.ShowOnProductPage,
                DisplayOrder = model.DisplayOrder,
            };

            _specificationAttributeService.InsertProductSpecificationAttribute(psa);
            product.ProductSpecificationAttributes.Add(psa);
        }
        public virtual void UpdateProductSpecificationAttributeModel(Product product, ProductSpecificationAttribute psa, ProductSpecificationAttributeModel model)
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
            _specificationAttributeService.UpdateProductSpecificationAttribute(psa);
        }
        public virtual void DeleteProductSpecificationAttribute(Product product, ProductSpecificationAttribute psa)
        {
            psa.ProductId = product.Id;
            product.ProductSpecificationAttributes.Remove(psa);
            _specificationAttributeService.DeleteProductSpecificationAttribute(psa);
        }
    }
}
