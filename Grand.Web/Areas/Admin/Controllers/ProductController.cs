using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Discounts;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Media;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Tax;
using Grand.Framework.Controllers;
using Grand.Framework.Extensions;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Discounts;
using Grand.Services.ExportImport;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Media;
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Shipping;
using Grand.Services.Stores;
using Grand.Services.Tax;
using Grand.Services.Vendors;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Infrastructure.Cache;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Web.Areas.Admin.Models.Orders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Grand.Web.Areas.Admin.Controllers
{
    public partial class ProductController : BaseAdminController
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IProductTemplateService _productTemplateService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ICustomerService _customerService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContext _workContext;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IPictureService _pictureService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly IProductTagService _productTagService;
        private readonly ICopyProductService _copyProductService;
        private readonly IPdfService _pdfService;
        private readonly IExportManager _exportManager;
        private readonly IImportManager _importManager;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IPermissionService _permissionService;
        private readonly IAclService _aclService;
        private readonly IStoreService _storeService;
        private readonly IOrderService _orderService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IVendorService _vendorService;
        private readonly IShippingService _shippingService;
        private readonly IShipmentService _shipmentService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IMeasureService _measureService;
        private readonly MeasureSettings _measureSettings;
        private readonly AdminAreaSettings _adminAreaSettings;
        private readonly TaxSettings _taxSettings;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IDiscountService _discountService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IBackInStockSubscriptionService _backInStockSubscriptionService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IDownloadService _downloadService;
        private readonly IRepository<Product> _productRepository;
        private readonly ICacheManager _cacheManager;
        private readonly IProductReservationService _productReservationService;
        private readonly MediaSettings _mediaSettings;
        private readonly IAuctionService _auctionService;
        private readonly IPriceFormatter _priceFormatter;


        #endregion

        #region Constructors

        public ProductController(IProductService productService,
            IProductTemplateService productTemplateService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            ICustomerService customerService,
            IUrlRecordService urlRecordService,
            IWorkContext workContext,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ISpecificationAttributeService specificationAttributeService,
            IPictureService pictureService,
            ITaxCategoryService taxCategoryService,
            IProductTagService productTagService,
            ICopyProductService copyProductService,
            IPdfService pdfService,
            IExportManager exportManager,
            IImportManager importManager,
            ICustomerActivityService customerActivityService,
            IPermissionService permissionService,
            IAclService aclService,
            IStoreService storeService,
            IOrderService orderService,
            IStoreMappingService storeMappingService,
            IVendorService vendorService,
            IShippingService shippingService,
            IShipmentService shipmentService,
            ICurrencyService currencyService,
            CurrencySettings currencySettings,
            IMeasureService measureService,
            MeasureSettings measureSettings,
            AdminAreaSettings adminAreaSettings,
            TaxSettings taxSettings,
            IDateTimeHelper dateTimeHelper,
            IDiscountService discountService,
            IProductAttributeService productAttributeService,
            IBackInStockSubscriptionService backInStockSubscriptionService,
            IShoppingCartService shoppingCartService,
            IProductAttributeFormatter productAttributeFormatter,
            IProductAttributeParser productAttributeParser,
            IDownloadService downloadService,
            IRepository<Product> productRepository,
            ICacheManager cacheManager,
            IProductReservationService productReservationService,
            MediaSettings mediaSettings,
            IAuctionService auctionService,
            IPriceFormatter priceFormatter)
        {
            this._productService = productService;
            this._productTemplateService = productTemplateService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._customerService = customerService;
            this._urlRecordService = urlRecordService;
            this._workContext = workContext;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._specificationAttributeService = specificationAttributeService;
            this._pictureService = pictureService;
            this._taxCategoryService = taxCategoryService;
            this._productTagService = productTagService;
            this._copyProductService = copyProductService;
            this._pdfService = pdfService;
            this._exportManager = exportManager;
            this._importManager = importManager;
            this._customerActivityService = customerActivityService;
            this._permissionService = permissionService;
            this._aclService = aclService;
            this._storeService = storeService;
            this._orderService = orderService;
            this._storeMappingService = storeMappingService;
            this._vendorService = vendorService;
            this._shippingService = shippingService;
            this._shipmentService = shipmentService;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._measureService = measureService;
            this._measureSettings = measureSettings;
            this._adminAreaSettings = adminAreaSettings;
            this._taxSettings = taxSettings;
            this._dateTimeHelper = dateTimeHelper;
            this._discountService = discountService;
            this._productAttributeService = productAttributeService;
            this._backInStockSubscriptionService = backInStockSubscriptionService;
            this._shoppingCartService = shoppingCartService;
            this._productAttributeFormatter = productAttributeFormatter;
            this._productAttributeParser = productAttributeParser;
            this._downloadService = downloadService;
            this._productRepository = productRepository;
            this._cacheManager = cacheManager;
            this._productReservationService = productReservationService;
            this._mediaSettings = mediaSettings;
            this._auctionService = auctionService;
            this._priceFormatter = priceFormatter;
        }

        #endregion 

        #region Utilities

        [NonAction]
        protected virtual List<LocalizedProperty> UpdateLocales(Product product, ProductModel model)
        {
            List<LocalizedProperty> localized = new List<LocalizedProperty>();

            foreach (var local in model.Locales)
            {
                var seName = product.ValidateSeName(local.SeName, local.Name, false);
                _urlRecordService.SaveSlug(product, seName, local.LanguageId);

                if (!(String.IsNullOrEmpty(seName)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "SeName",
                        LocaleValue = seName,
                    });

                if (!(String.IsNullOrEmpty(local.Name)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "Name",
                        LocaleValue = local.Name,
                    });

                if (!(String.IsNullOrEmpty(local.ShortDescription)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "ShortDescription",
                        LocaleValue = local.ShortDescription,
                    });

                if (!(String.IsNullOrEmpty(local.FullDescription)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "FullDescription",
                        LocaleValue = local.FullDescription,
                    });

                if (!(String.IsNullOrEmpty(local.MetaDescription)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "MetaDescription",
                        LocaleValue = local.MetaDescription,
                    });

                if (!(String.IsNullOrEmpty(local.MetaKeywords)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "MetaKeywords",
                        LocaleValue = local.MetaKeywords,
                    });

                if (!(String.IsNullOrEmpty(local.MetaTitle)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "MetaTitle",
                        LocaleValue = local.MetaTitle,
                    });


            }
            return localized;

        }

        [NonAction]
        protected virtual List<LocalizedProperty> UpdateLocales(ProductTag productTag, ProductTagModel model)
        {
            List<LocalizedProperty> localized = new List<LocalizedProperty>();
            foreach (var local in model.Locales)
            {
                if (!(String.IsNullOrEmpty(local.Name)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "Name",
                        LocaleValue = local.Name,
                    });
            }
            return localized;
        }

        [NonAction]
        protected virtual List<LocalizedProperty> UpdateLocales(ProductAttributeValue pav, ProductModel.ProductAttributeValueModel model)
        {
            List<LocalizedProperty> localized = new List<LocalizedProperty>();
            foreach (var local in model.Locales)
            {
                if (!(String.IsNullOrEmpty(local.Name)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "Name",
                        LocaleValue = local.Name,
                    });
            }
            return localized;
        }

        [NonAction]
        protected virtual void UpdatePictureSeoNames(Product product)
        {
            foreach (var pp in product.ProductPictures)
                _pictureService.SetSeoFilename(pp.PictureId, _pictureService.GetPictureSeName(product.Name));
        }

        [NonAction]
        protected virtual void PrepareAclModel(ProductModel model, Product product, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableCustomerRoles = _customerService
                .GetAllCustomerRoles(true)
                .Select(cr => cr.ToModel())
                .ToList();
            if (!excludeProperties)
            {
                if (product != null)
                {
                    model.SelectedCustomerRoleIds = product.CustomerRoles.ToArray();
                }
            }
        }

        [NonAction]
        protected virtual void PrepareStoresMappingModel(ProductModel model, Product product, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableStores = _storeService
                .GetAllStores()
                .Where(x => (_workContext.CurrentVendor == null ? true : !string.IsNullOrEmpty(_workContext.CurrentVendor.StoreId) ? x.Id == _workContext.CurrentVendor.StoreId ? true : false : true)
                )
                .Select(s => s.ToModel())
                .ToList();
            if (!excludeProperties)
            {
                if (product != null)
                {
                    model.SelectedStoreIds = product.Stores.ToArray();
                }
            }
        }

        [NonAction]
        protected virtual void PrepareAddProductAttributeCombinationModel(ProductAttributeCombinationModel model, Product product)
        {
            if (model == null)
                throw new ArgumentNullException("model");
            if (product == null)
                throw new ArgumentNullException("product");

            if (string.IsNullOrEmpty(model.Id))
            {
                model.ProductId = product.Id;
                model.StockQuantity = 0;
                model.NotifyAdminForQuantityBelow = 1;
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

        [NonAction]
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

        [NonAction]
        protected virtual void SaveProductTags(Product product, string[] productTags)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            //product tags
            var existingProductTags = product.ProductTags.ToList();
            var productTagsToRemove = new List<string>();
            foreach (var existingProductTag in existingProductTags)
            {
                var existingProductTagText = _productTagService.GetProductTagById(existingProductTag);
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
                    productTagsToRemove.Add(existingProductTag);
                }
            }
            foreach (var productTag in productTagsToRemove)
            {
                _productService.DeleteProductTag(new ProductTag() { ProductId = product.Id, Id = productTag });
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
                        Count = 0,
                    };
                    _productTagService.InsertProductTag(productTag);
                }
                else
                {
                    productTag = productTag2;
                }
                if (!product.ProductTagExists(productTag.Id))
                {
                    productTag.ProductId = product.Id;
                    _productService.InsertProductTag(productTag);
                }
            }
        }

        [NonAction]
        protected virtual void PrepareProductModel(ProductModel model, Product product,
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
                    var productTag = _productTagService.GetProductTagById(pt);
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

        [NonAction]
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

        [NonAction]
        protected virtual void SaveProductWarehouseInventory(Product product, ProductModel model)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (model.ManageInventoryMethodId != (int)ManageInventoryMethod.ManageStock)
                return;

            if (!model.UseMultipleWarehouses)
                return;

            var warehouses = _shippingService.GetAllWarehouses();

            foreach (var warehouse in warehouses)
            {
                //parse stock quantity
                int stockQuantity = 0;
                foreach (string formKey in this.Request.Form.Keys)
                    if (formKey.Equals(string.Format("warehouse_qty_{0}", warehouse.Id), StringComparison.OrdinalIgnoreCase))
                    {
                        int.TryParse(this.Request.Form[formKey], out stockQuantity);
                        break;
                    }
                //parse reserved quantity
                int reservedQuantity = 0;
                foreach (string formKey in this.Request.Form.Keys)
                    if (formKey.Equals(string.Format("warehouse_reserved_{0}", warehouse.Id), StringComparison.OrdinalIgnoreCase))
                    {
                        int.TryParse(this.Request.Form[formKey], out reservedQuantity);
                        break;
                    }
                //parse "used" field
                bool used = false;
                foreach (string formKey in this.Request.Form.Keys)
                    if (formKey.Equals(string.Format("warehouse_used_{0}", warehouse.Id), StringComparison.OrdinalIgnoreCase))
                    {
                        used = this.Request.Form[formKey] == warehouse.Id;
                        break;
                    }

                var existingPwI = product.ProductWarehouseInventory.FirstOrDefault(x => x.WarehouseId == warehouse.Id);

                if (existingPwI != null)
                {
                    existingPwI.ProductId = product.Id;
                    if (used)
                    {
                        //update existing record

                        existingPwI.StockQuantity = stockQuantity;
                        existingPwI.ReservedQuantity = reservedQuantity;
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
                    if (used)
                    {
                        //no need to insert a record for qty 0
                        existingPwI = new ProductWarehouseInventory
                        {
                            WarehouseId = warehouse.Id,
                            ProductId = product.Id,
                            StockQuantity = stockQuantity,
                            ReservedQuantity = reservedQuantity
                        };
                        product.ProductWarehouseInventory.Add(existingPwI);
                        _productService.InsertProductWarehouseInventory(existingPwI);
                    }
                }
            }



        }

        [NonAction]
        protected virtual void PrepareProductReviewModel(ProductReviewModel model,
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


        #endregion

        #region Methods

        #region Product list / create / edit / delete

        //list products
        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

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

            return View(model);
        }

        [HttpPost]
        public IActionResult ProductList(DataSourceRequest command, ProductListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

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
                pageIndex: command.Page - 1,
                pageSize: command.PageSize,
                showHidden: true,
                overridePublished: overridePublished
            );
            var gridModel = new DataSourceResult();
            gridModel.Data = products.Select(x =>
            {
                var productModel = x.ToModel();
                //little hack here:
                //ensure that product full descriptions are not returned
                //otherwise, we can get the following error if products have too long descriptions:
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
            });
            gridModel.Total = products.TotalCount;

            return Json(gridModel);
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("go-to-product-by-sku")]
        public IActionResult GoToSku(ProductListModel model)
        {
            string sku = model.GoDirectlyToSku;

            //try to load a product entity
            var product = _productService.GetProductBySku(sku);
            if (product != null)
                return RedirectToAction("Edit", "Product", new { id = product.Id });

            //not found
            return RedirectToAction("List", "Product");
        }

        //create product
        public IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var model = new ProductModel();
            PrepareProductModel(model, null, true, true);
            AddLocales(_languageService, model.Locales);
            PrepareAclModel(model, null, false);
            PrepareStoresMappingModel(model, null, false);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(ProductModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (ModelState.IsValid)
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
                product.Stores = model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>();
                product.CustomerRoles = model.SelectedCustomerRoleIds != null ? model.SelectedCustomerRoleIds.ToList() : new List<string>();

                _productService.InsertProduct(product);

                model.SeName = product.ValidateSeName(model.SeName, product.Name, true);
                product.SeName = model.SeName;
                product.Locales = UpdateLocales(product, model);

                //search engine name
                _urlRecordService.SaveSlug(product, model.SeName, "");
                //tags
                SaveProductTags(product, ParseProductTags(model.ProductTags));
                //warehouses
                SaveProductWarehouseInventory(product, model);
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

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = product.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            PrepareProductModel(model, null, false, true);
            PrepareAclModel(model, null, true);
            PrepareStoresMappingModel(model, null, true);
            return View(model);
        }

        //edit product
        public IActionResult Edit(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(id);
            if (product == null)
                //No product found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List");

            var model = product.ToModel();
            model.Ticks = product.UpdatedOnUtc.Ticks;

            PrepareProductModel(model, product, false, false);
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = product.GetLocalized(x => x.Name, languageId, false, false);
                locale.ShortDescription = product.GetLocalized(x => x.ShortDescription, languageId, false, false);
                locale.FullDescription = product.GetLocalized(x => x.FullDescription, languageId, false, false);
                locale.MetaKeywords = product.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaDescription = product.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaTitle = product.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = product.GetSeName(languageId, false, false);
            });

            PrepareAclModel(model, product, false);
            PrepareStoresMappingModel(model, product, false);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(ProductModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(model.Id);
            if (product == null)
                //No product found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List");

            if (model.Ticks != product.UpdatedOnUtc.Ticks)
            {
                ErrorNotification(_localizationService.GetResource("Admin.Catalog.Products.Fields.ChangedWarning"));
                return RedirectToAction("Edit", new { id = product.Id });
            }
            if (ModelState.IsValid)
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
                product.Stores = model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>();
                product.CustomerRoles = model.SelectedCustomerRoleIds != null ? model.SelectedCustomerRoleIds.ToList() : new List<string>();
                model.SeName = product.ValidateSeName(model.SeName, product.Name, true);
                product.SeName = model.SeName;
                product.Locales = UpdateLocales(product, model);

                //search engine name
                _urlRecordService.SaveSlug(product, model.SeName, "");
                //tags
                SaveProductTags(product, ParseProductTags(model.ProductTags));
                //warehouses
                SaveProductWarehouseInventory(product, model);
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

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Updated"));

                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new { id = product.Id });
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            PrepareProductModel(model, product, false, true);
            PrepareAclModel(model, product, true);
            PrepareStoresMappingModel(model, product, true);
            return View(model);
        }

        //delete product
        [HttpPost]
        public IActionResult Delete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(id);
            if (product == null)
                //No product found with the specified id
                return RedirectToAction("List");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List");

            _productService.DeleteProduct(product);

            //activity log
            _customerActivityService.InsertActivity("DeleteProduct", product.Id, _localizationService.GetResource("ActivityLog.DeleteProduct"), product.Name);

            SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Deleted"));
            return RedirectToAction("List");
        }

        [HttpPost]
        public IActionResult DeleteSelected(ICollection<string> selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var products = new List<Product>();
            if (selectedIds != null)
            {
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

            return Json(new { Result = true });
        }

        [HttpPost]
        public IActionResult CopyProduct(ProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var copyModel = model.CopyProductModel;
            try
            {
                var originalProduct = _productService.GetProductById(copyModel.Id);

                //a vendor should have access only to his products
                if (_workContext.CurrentVendor != null && originalProduct.VendorId != _workContext.CurrentVendor.Id)
                    return RedirectToAction("List");

                var newProduct = _copyProductService.CopyProduct(originalProduct,
                    copyModel.Name, copyModel.Published, copyModel.CopyImages);
                SuccessNotification("The product has been copied successfully");
                return RedirectToAction("Edit", new { id = newProduct.Id });
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
                return RedirectToAction("Edit", new { id = copyModel.Id });
            }
        }

        #endregion

        #region Required products

        [HttpPost]

        public IActionResult LoadProductFriendlyNames(string productIds)
        {
            var result = "";

            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return Json(new { Text = result });

            if (!String.IsNullOrWhiteSpace(productIds))
            {
                var ids = new List<string>();
                var rangeArray = productIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .ToList();

                foreach (string str1 in rangeArray)
                {
                    ids.Add(str1);
                }

                var products = _productService.GetProductsByIds(ids.ToArray());
                for (int i = 0; i <= products.Count - 1; i++)
                {
                    result += products[i].Name;
                    if (i != products.Count - 1)
                        result += ", ";
                }
            }

            return Json(new { Text = result });
        }

        public IActionResult RequiredProductAddPopup(string productIdsInput)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

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


            ViewBag.productIdsInput = productIdsInput;
            return View(model);
        }

        [HttpPost]
        public IActionResult RequiredProductAddPopupList(DataSourceRequest command, ProductModel.AddRequiredProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }

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

        #endregion

        #region Product categories

        [HttpPost]
        public IActionResult ProductCategoryList(DataSourceRequest command, string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();
            var product = _productService.GetProductById(productId);

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            var productCategories = product.ProductCategories.OrderBy(x => x.DisplayOrder);
            var productCategoriesModel = productCategories
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

            var gridModel = new DataSourceResult
            {
                Data = productCategoriesModel,
                Total = productCategoriesModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ProductCategoryInsert(ProductModel.ProductCategoryModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var productId = model.ProductId;
            var categoryId = model.CategoryId;

            var product = _productService.GetProductById(productId);
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            if (product.ProductCategories.Where(x => x.CategoryId == categoryId).Count() == 0)
            {
                var productCategory = new ProductCategory
                {
                    ProductId = productId,
                    CategoryId = categoryId,
                    DisplayOrder = model.DisplayOrder,
                };
                //a vendor cannot edit "IsFeaturedProduct" property
                if (_workContext.CurrentVendor == null)
                {
                    productCategory.IsFeaturedProduct = model.IsFeaturedProduct;
                }
                _categoryService.InsertProductCategory(productCategory);
            }

            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult ProductCategoryUpdate(ProductModel.ProductCategoryModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(model.ProductId);
            var productCategory = product.ProductCategories.Where(x => x.Id == model.Id).FirstOrDefault();
            if (productCategory == null)
                throw new ArgumentException("No product category mapping found with the specified id");

            if (product.ProductCategories.Where(x => x.Id != model.Id && x.CategoryId == model.CategoryId).Any())
                return Json(new DataSourceResult { Errors = "This category is already mapped with this product" });

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
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

            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult ProductCategoryDelete(string id, string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

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
                    return Content("This is not your product");
                }
            }

            _categoryService.DeleteProductCategory(productCategory);

            return new NullJsonResult();
        }

        #endregion

        #region Product manufacturers

        [HttpPost]
        public IActionResult ProductManufacturerList(DataSourceRequest command, string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {

                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            var productManufacturers = product.ProductManufacturers.OrderBy(x => x.DisplayOrder);
            var productManufacturersModel = productManufacturers
                .Select(x => new ProductModel.ProductManufacturerModel
                {
                    Id = x.Id,
                    Manufacturer = _manufacturerService.GetManufacturerById(x.ManufacturerId).Name,
                    ProductId = product.Id,
                    ManufacturerId = x.ManufacturerId,
                    IsFeaturedProduct = x.IsFeaturedProduct,
                    DisplayOrder = x.DisplayOrder
                })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = productManufacturersModel,
                Total = productManufacturersModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ProductManufacturerInsert(ProductModel.ProductManufacturerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var manufacturerId = model.ManufacturerId;
            var product = _productService.GetProductById(model.ProductId);
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {

                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
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

            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult ProductManufacturerUpdate(ProductModel.ProductManufacturerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(model.ProductId);
            var productManufacturer = product.ProductManufacturers.Where(x => x.Id == model.Id).FirstOrDefault();
            if (productManufacturer == null)
                throw new ArgumentException("No product manufacturer mapping found with the specified id");

            if (product.ProductManufacturers.Where(x => x.Id != model.Id && x.ManufacturerId == model.ManufacturerId).Any())
                return Json(new DataSourceResult { Errors = "This manufacturer is already mapped with this product" });

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {

                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
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

            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult ProductManufacturerDelete(string id, string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

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
                    return Content("This is not your product");
                }
            }

            _manufacturerService.DeleteProductManufacturer(productManufacturer);

            return new NullJsonResult();
        }

        #endregion

        #region Related products

        [HttpPost]
        public IActionResult RelatedProductList(DataSourceRequest command, string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {

                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            var relatedProducts = product.RelatedProducts.OrderBy(x => x.DisplayOrder);
            var relatedProductsModel = relatedProducts
                .Select(x => new ProductModel.RelatedProductModel
                {
                    Id = x.Id,
                    ProductId1 = productId,
                    ProductId2 = x.ProductId2,
                    Product2Name = _productService.GetProductById(x.ProductId2).Name,
                    DisplayOrder = x.DisplayOrder
                })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = relatedProductsModel,
                Total = relatedProductsModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult RelatedProductUpdate(ProductModel.RelatedProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

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
                    return Content("This is not your product");
                }
            }
            relatedProduct.ProductId1 = model.ProductId1;
            relatedProduct.DisplayOrder = model.DisplayOrder;
            _productService.UpdateRelatedProduct(relatedProduct);

            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult RelatedProductDelete(ProductModel.RelatedProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(model.ProductId1);
            var relatedProduct = product.RelatedProducts.Where(x => x.Id == model.Id).FirstOrDefault();
            if (relatedProduct == null)
                throw new ArgumentException("No related product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {

                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }
            relatedProduct.ProductId1 = model.ProductId1;
            _productService.DeleteRelatedProduct(relatedProduct);

            return new NullJsonResult();
        }

        public IActionResult RelatedProductAddPopup(string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

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

            return View(model);
        }

        [HttpPost]
        public IActionResult RelatedProductAddPopupList(DataSourceRequest command, ProductModel.AddRelatedProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }

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
        public IActionResult RelatedProductAddPopup(ProductModel.AddRelatedProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (model.SelectedProductIds != null)
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

            //a vendor should have access only to his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            ViewBag.RefreshPage = true;
            return View(model);
        }

        #endregion


        #region Bundle products

        [HttpPost]
        public IActionResult BundleProductList(DataSourceRequest command, string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {

                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            var bundleProducts = product.BundleProducts.OrderBy(x => x.DisplayOrder);
            var bundleProductsModel = bundleProducts
                .Select(x => new ProductModel.BundleProductModel
                {
                    Id = x.Id,
                    ProductBundleId = productId,
                    ProductId = x.ProductId,
                    ProductName = _productService.GetProductById(x.ProductId).Name,
                    DisplayOrder = x.DisplayOrder,
                    Quantity = x.Quantity
                })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = bundleProductsModel,
                Total = bundleProductsModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult BundleProductUpdate(ProductModel.BundleProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

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
                    return Content("This is not your product");
                }
            }
            bundleProduct.ProductBundleId = model.ProductBundleId;
            bundleProduct.ProductId = model.ProductId;
            bundleProduct.Quantity = model.Quantity > 0 ? model.Quantity : 1;
            bundleProduct.DisplayOrder = model.DisplayOrder;
            _productService.UpdateBundleProduct(bundleProduct);

            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult BundleProductDelete(ProductModel.BundleProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(model.ProductBundleId);
            var bundleProduct = product.BundleProducts.Where(x => x.Id == model.Id).FirstOrDefault();
            if (bundleProduct == null)
                throw new ArgumentException("No bundle product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {

                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }
            bundleProduct.ProductBundleId = model.ProductBundleId;
            _productService.DeleteBundleProduct(bundleProduct);

            return new NullJsonResult();
        }

        public IActionResult BundleProductAddPopup(string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

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

            return View(model);
        }

        [HttpPost]
        public IActionResult BundleProductAddPopupList(DataSourceRequest command, ProductModel.AddBundleProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }

            var searchCategoryIds = new List<string>();
            if (!String.IsNullOrEmpty(model.SearchCategoryId))
                searchCategoryIds.Add(model.SearchCategoryId);

            var products = _productService.SearchProducts(
                categoryIds: searchCategoryIds,
                manufacturerId: model.SearchManufacturerId,
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                productType: ProductType.SimpleProduct,
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
        public IActionResult BundleProductAddPopup(ProductModel.AddBundleProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (model.SelectedProductIds != null)
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

            //a vendor should have access only to his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            ViewBag.RefreshPage = true;
            return View(model);
        }

        #endregion

        #region Cross-sell products

        [HttpPost]
        public IActionResult CrossSellProductList(DataSourceRequest command, string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            var crossSellProducts = product.CrossSellProduct;
            var crossSellProductsModel = crossSellProducts
                .Select(x => new ProductModel.CrossSellProductModel
                {
                    Id = x,
                    Product2Name = _productService.GetProductById(x).Name,
                })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = crossSellProductsModel,
                Total = crossSellProductsModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult CrossSellProductDelete(string id, string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);

            var crossSellProduct = product.CrossSellProduct.Where(x => x == id).FirstOrDefault();
            if (String.IsNullOrEmpty(crossSellProduct))
                throw new ArgumentException("No cross-sell product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            CrossSellProduct crosssell = new CrossSellProduct()
            {
                ProductId1 = productId,
                ProductId2 = crossSellProduct
            };

            _productService.DeleteCrossSellProduct(crosssell);

            return new NullJsonResult();
        }

        public IActionResult CrossSellProductAddPopup(string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

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

            return View(model);
        }

        [HttpPost]
        public IActionResult CrossSellProductAddPopupList(DataSourceRequest command, ProductModel.AddCrossSellProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }
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
        public IActionResult CrossSellProductAddPopup(ProductModel.AddCrossSellProductModel model)
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

            //a vendor should have access only to his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            ViewBag.RefreshPage = true;

            return View(model);
        }

        #endregion

        #region Associated products

        [HttpPost]
        public IActionResult AssociatedProductList(DataSourceRequest command, string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                var product = _productService.GetProductById(productId);
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            //a vendor should have access only to his products
            var vendorId = "";
            if (_workContext.CurrentVendor != null)
            {
                vendorId = _workContext.CurrentVendor.Id;
            }

            var associatedProducts = _productService.GetAssociatedProducts(parentGroupedProductId: productId,
                vendorId: vendorId,
                showHidden: true);
            var associatedProductsModel = associatedProducts
                .Select(x => new ProductModel.AssociatedProductModel
                {
                    Id = x.Id,
                    ProductName = x.Name,
                    DisplayOrder = x.DisplayOrder
                })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = associatedProductsModel,
                Total = associatedProductsModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult AssociatedProductUpdate(ProductModel.AssociatedProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var associatedProduct = _productService.GetProductById(model.Id);
            if (associatedProduct == null)
                throw new ArgumentException("No associated product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && associatedProduct.VendorId != _workContext.CurrentVendor.Id)
            {
                return Content("This is not your product");
            }

            associatedProduct.DisplayOrder = model.DisplayOrder;
            _productService.UpdateAssociatedProduct(associatedProduct);

            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult AssociatedProductDelete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(id);
            if (product == null)
                throw new ArgumentException("No associated product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            product.ParentGroupedProductId = "";
            _productService.UpdateAssociatedProduct(product);

            return new NullJsonResult();
        }

        public IActionResult AssociatedProductAddPopup(string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

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

            return View(model);
        }

        [HttpPost]
        public IActionResult AssociatedProductAddPopupList(DataSourceRequest command, ProductModel.AddAssociatedProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }
            var searchCategoryIds = new List<string>();
            if (!String.IsNullOrEmpty(model.SearchCategoryId))
                searchCategoryIds.Add(model.SearchCategoryId);

            var products = _productService.SearchProducts(
                categoryIds: searchCategoryIds,
                manufacturerId: model.SearchManufacturerId,
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                productType: ProductType.SimpleProduct,
                keywords: model.SearchProductName,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize,
                showHidden: true
                );
            var gridModel = new DataSourceResult();
            gridModel.Data = products.Select(x =>
            {
                var productModel = x.ToModel();
                //display already associated products
                var parentGroupedProduct = _productService.GetProductById(x.ParentGroupedProductId);
                if (parentGroupedProduct != null)
                {
                    productModel.AssociatedToProductId = x.ParentGroupedProductId;
                    productModel.AssociatedToProductName = parentGroupedProduct.Name;
                }
                return productModel;
            });
            gridModel.Total = products.TotalCount;

            return Json(gridModel);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public IActionResult AssociatedProductAddPopup(ProductModel.AddAssociatedProductModel model)
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
                        //a vendor should have access only to his products
                        if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                            continue;

                        product.ParentGroupedProductId = model.ProductId;
                        _productService.UpdateAssociatedProduct(product);
                    }
                }
            }

            //a vendor should have access only to his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            ViewBag.RefreshPage = true;
            return View(model);
        }

        #endregion

        #region Product pictures


        public IActionResult ProductPictureAdd(string pictureId, int displayOrder,
            string overrideAltAttribute, string overrideTitleAttribute,
            string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (String.IsNullOrEmpty(pictureId))
                throw new ArgumentException();

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List");

            var picture = _pictureService.GetPictureById(pictureId);
            if (picture == null)
                throw new ArgumentException("No picture found with the specified id");

            if (product.ProductPictures.Where(x => x.PictureId == pictureId).Count() > 0)
                return null;

            _pictureService.UpdatePicture(picture.Id,
                _pictureService.LoadPictureBinary(picture),
                picture.MimeType,
                picture.SeoFilename,
                overrideAltAttribute,
                overrideTitleAttribute);

            _productService.InsertProductPicture(new ProductPicture
            {
                PictureId = pictureId,
                ProductId = productId,
                DisplayOrder = displayOrder,
                AltAttribute = overrideAltAttribute,
                MimeType = picture.MimeType,
                SeoFilename = picture.SeoFilename,
                TitleAttribute = overrideTitleAttribute
            });

            _pictureService.SetSeoFilename(pictureId, _pictureService.GetPictureSeName(product.Name));

            return Json(new { Result = true });
        }

        [HttpPost]
        public IActionResult ProductPictureList(DataSourceRequest command, string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            var productPictures = product.ProductPictures.OrderBy(x => x.DisplayOrder);
            var productPicturesModel = productPictures
                .Select(x =>
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

            var gridModel = new DataSourceResult
            {
                Data = productPicturesModel,
                Total = productPicturesModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ProductPictureUpdate(ProductModel.ProductPictureModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

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
                    return Content("This is not your product");
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

            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult ProductPictureDelete(ProductModel.ProductPictureModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

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
                    return Content("This is not your product");
                }
            }
            var pictureId = productPicture.PictureId;
            _productService.DeleteProductPicture(productPicture);

            var picture = _pictureService.GetPictureById(pictureId);
            if (picture != null)
                _pictureService.DeletePicture(picture);

            return new NullJsonResult();
        }

        #endregion

        #region Product specification attributes


        public IActionResult ProductSpecificationAttributeAdd(int attributeTypeId, string specificationAttributeId, string specificationAttributeOptionId,
            string customValue, bool allowFiltering, bool showOnProductPage,
            int displayOrder, string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return RedirectToAction("List");
                }
            }

            //we allow filtering only for "Option" attribute type
            if (attributeTypeId != (int)SpecificationAttributeType.Option)
            {
                allowFiltering = false;
                specificationAttributeOptionId = null;
            }
            //we don't allow CustomValue for "Option" attribute type
            if (attributeTypeId == (int)SpecificationAttributeType.Option)
            {
                customValue = null;
            }
            var psa = new ProductSpecificationAttribute
            {
                AttributeTypeId = attributeTypeId,
                SpecificationAttributeOptionId = specificationAttributeOptionId,
                SpecificationAttributeId = specificationAttributeId,
                ProductId = productId,
                CustomValue = customValue,
                AllowFiltering = allowFiltering,
                ShowOnProductPage = showOnProductPage,
                DisplayOrder = displayOrder,
            };

            _specificationAttributeService.InsertProductSpecificationAttribute(psa);
            product.ProductSpecificationAttributes.Add(psa);

            return Json(new { Result = true });
        }

        [HttpPost]
        public IActionResult ProductSpecAttrList(DataSourceRequest command, string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor should have access only to his products
            var product = _productService.GetProductById(productId);
            if (_workContext.CurrentVendor != null)
            {
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

            var productrSpecs = product.ProductSpecificationAttributes.OrderBy(x => x.DisplayOrder);

            var productrSpecsModel = productrSpecs
                .Select(x =>
                {
                    var specificationAttribute = _specificationAttributeService.GetSpecificationAttributeById(x.SpecificationAttributeId);
                    var psaModel = new ProductSpecificationAttributeModel
                    {
                        Id = x.Id,
                        AttributeTypeId = (int)x.AttributeType,
                        ProductSpecificationId = specificationAttribute.Id,
                        AttributeId = x.SpecificationAttributeId,
                        ProductId = productId,
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
                })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = productrSpecsModel,
                Total = productrSpecsModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ProductSpecAttrUpdate(ProductSpecificationAttributeModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(model.ProductId);
            var psa = product.ProductSpecificationAttributes.Where(x => x.SpecificationAttributeId == model.ProductSpecificationId).Where(x => x.Id == model.Id).FirstOrDefault();
            if (psa == null)
                return Content("No product specification attribute found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }

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

            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult ProductSpecAttrDelete(string id, string ProductSpecificationId, string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            var psa = product.ProductSpecificationAttributes.Where(x => x.Id == id && x.SpecificationAttributeId == ProductSpecificationId).FirstOrDefault();
            if (psa == null)
                throw new ArgumentException("No specification attribute found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                if (product != null && product.VendorId != _workContext.CurrentVendor.Id)
                {
                    return Content("This is not your product");
                }
            }
            psa.ProductId = product.Id;
            product.ProductSpecificationAttributes.Remove(psa);
            _specificationAttributeService.DeleteProductSpecificationAttribute(psa);


            return new NullJsonResult();
        }

        #endregion

        #region Product tags

        public IActionResult ProductTags()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProductTags))
                return AccessDeniedView();

            return View();
        }

        [HttpPost]
        public IActionResult ProductTags(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProductTags))
                return AccessDeniedView();

            var tags = _productTagService.GetAllProductTags()
                //order by product count
                .OrderByDescending(x => _productTagService.GetProductCount(x.Id, ""))
                .Select(x => new ProductTagModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    ProductCount = _productTagService.GetProductCount(x.Id, "")
                })
                .ToList();

            var gridModel = new DataSourceResult
            {
                Data = tags.PagedForCommand(command),
                Total = tags.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ProductTagDelete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProductTags))
                return AccessDeniedView();

            var tag = _productTagService.GetProductTagById(id);
            if (tag == null)
                throw new ArgumentException("No product tag found with the specified id");
            _productTagService.DeleteProductTag(tag);

            return new NullJsonResult();
        }

        //edit
        public IActionResult EditProductTag(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProductTags))
                return AccessDeniedView();

            var productTag = _productTagService.GetProductTagById(id);
            if (productTag == null)
                //No product tag found with the specified id
                return RedirectToAction("List");

            var model = new ProductTagModel
            {
                Id = productTag.Id,
                Name = productTag.Name,
                ProductCount = _productTagService.GetProductCount(productTag.Id, "")
            };
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = productTag.GetLocalized(x => x.Name, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost]
        public IActionResult EditProductTag(ProductTagModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProductTags))
                return AccessDeniedView();

            var productTag = _productTagService.GetProductTagById(model.Id);
            if (productTag == null)
                //No product tag found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                productTag.Name = model.Name;
                productTag.Locales = UpdateLocales(productTag, model);
                _productTagService.UpdateProductTag(productTag);

                ViewBag.RefreshPage = true;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion

        #region Purchased with order

        [HttpPost]
        public IActionResult PurchasedWithOrders(DataSourceRequest command, string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            var orders = _orderService.SearchOrders(
                productId: productId,
                pageIndex: command.Page - 1,
                pageSize: command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = orders.Select(x =>
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
                }),
                Total = orders.TotalCount
            };

            return Json(gridModel);
        }

        #endregion

        #region Reviews

        [HttpPost]
        public IActionResult Reviews(DataSourceRequest command, string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            var productReviews = _productService.GetAllProductReviews("", null,
                null, null, "", null, productId);

            var gridModel = new DataSourceResult
            {
                Data = productReviews.PagedForCommand(command).Select(x =>
                {
                    var m = new ProductReviewModel();
                    PrepareProductReviewModel(m, x, false, true);
                    return m;
                }),
                Total = productReviews.Count,
            };

            return Json(gridModel);
        }

        #endregion

        #region Export / Import

        [HttpPost, ActionName("List")]
        [FormValueRequired("download-catalog-pdf")]
        public IActionResult DownloadCatalogAsPdf(ProductListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

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

            try
            {
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    _pdfService.PrintProductsToPdf(stream, products);
                    bytes = stream.ToArray();
                }
                return File(bytes, "application/pdf", "pdfcatalog.pdf");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }


        [HttpPost, ActionName("List")]
        [FormValueRequired("exportxml-all")]
        public IActionResult ExportXmlAll(ProductListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

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

            try
            {
                var xml = _exportManager.ExportProductsToXml(products);
                return File(Encoding.UTF8.GetBytes(xml), "application/xml", "products.xml");

            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public IActionResult ExportXmlSelected(string selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var products = new List<Product>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x)
                    .ToArray();
                products.AddRange(_productService.GetProductsByIds(ids));
            }
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                products = products.Where(p => p.VendorId == _workContext.CurrentVendor.Id).ToList();
            }

            var xml = _exportManager.ExportProductsToXml(products);
            return File(Encoding.UTF8.GetBytes(xml), "application/xml", "products.xml");

        }


        [HttpPost, ActionName("List")]
        [FormValueRequired("exportexcel-all")]
        public IActionResult ExportExcelAll(ProductListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

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
            try
            {
                byte[] bytes = _exportManager.ExportProductsToXlsx(products);
                return File(bytes, "text/xls", "products.xlsx");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public IActionResult ExportExcelSelected(string selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var products = new List<Product>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x)
                    .ToArray();
                products.AddRange(_productService.GetProductsByIds(ids));
            }
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                products = products.Where(p => p.VendorId == _workContext.CurrentVendor.Id).ToList();
            }

            byte[] bytes = _exportManager.ExportProductsToXlsx(products);
            return File(bytes, "text/xls", "products.xlsx");
        }

        [HttpPost]
        public IActionResult ImportExcel(IFormFile importexcelfile)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor cannot import products
            if (_workContext.CurrentVendor != null)
                return AccessDeniedView();

            try
            {
                if (importexcelfile != null && importexcelfile.Length > 0)
                {
                    _importManager.ImportProductsFromXlsx(importexcelfile.OpenReadStream());
                }
                else
                {
                    ErrorNotification(_localizationService.GetResource("Admin.Common.UploadFile"));
                    return RedirectToAction("List");
                }
                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Imported"));
                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
                return RedirectToAction("List");
            }

        }

        #endregion

        #region Low stock reports

        public IActionResult LowStockReport()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            return View();
        }
        [HttpPost]
        public IActionResult LowStockReportList(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            string vendorId = "";
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
                vendorId = _workContext.CurrentVendor.Id;

            IList<Product> products;
            IList<ProductAttributeCombination> combinations;
            _productService.GetLowStockProducts(vendorId, out products, out combinations);

            var models = new List<LowStockProductModel>();
            //products
            foreach (var product in products)
            {
                var lowStockModel = new LowStockProductModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    ManageInventoryMethod = product.ManageInventoryMethod.GetLocalizedEnum(_localizationService, _workContext.WorkingLanguage.Id),
                    StockQuantity = product.GetTotalStockQuantity(),
                    Published = product.Published
                };
                models.Add(lowStockModel);
            }
            //combinations
            foreach (var combination in combinations)
            {
                var product = _productService.GetProductById(combination.ProductId);
                var lowStockModel = new LowStockProductModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    Attributes = _productAttributeFormatter.FormatAttributes(product, combination.AttributesXml, _workContext.CurrentCustomer, "<br />", true, true, true, false),
                    ManageInventoryMethod = product.ManageInventoryMethod.GetLocalizedEnum(_localizationService, _workContext.WorkingLanguage.Id),
                    StockQuantity = combination.StockQuantity,
                    Published = product.Published
                };
                models.Add(lowStockModel);
            }
            var gridModel = new DataSourceResult
            {
                Data = models.PagedForCommand(command),
                Total = models.Count
            };

            return Json(gridModel);
        }

        #endregion

        #region Bulk editing

        public IActionResult BulkEdit()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

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

            return View(model);
        }

        [HttpPost]
        public IActionResult BulkEditSelect(DataSourceRequest command, BulkEditListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

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
                pageIndex: command.Page - 1,
                pageSize: command.PageSize,
                showHidden: true);
            var gridModel = new DataSourceResult();
            gridModel.Data = products.Select(x =>
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
            });
            gridModel.Total = products.TotalCount;

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult BulkEditUpdate(IEnumerable<BulkEditProductModel> products)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (products != null)
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

            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult BulkEditDelete(IEnumerable<BulkEditProductModel> products)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (products != null)
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
            return new NullJsonResult();
        }

        #endregion

        #region Tier prices

        [HttpPost]
        public IActionResult TierPriceList(DataSourceRequest command, string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

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
                    StartDateTimeUtc = x.StartDateTimeUtc,
                    EndDateTimeUtc = x.EndDateTimeUtc
                };
            }).ToList();

            var gridModel = new DataSourceResult
            {
                Data = tierPricesModel,
                Total = tierPricesModel.Count
            };

            return Json(gridModel);
        }

        public IActionResult TierPriceCreatePopup(string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var model = new ProductModel.TierPriceModel();
            model.ProductId = productId;
            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var store in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = store.Name, Value = store.Id.ToString() });

            //customer roles
            model.AvailableCustomerRoles.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var role in _customerService.GetAllCustomerRoles(true))
                model.AvailableCustomerRoles.Add(new SelectListItem { Text = role.Name, Value = role.Id.ToString() });

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public IActionResult TierPriceCreatePopup(ProductModel.TierPriceModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(model.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            if (ModelState.IsValid)
            {
                var tierPrice = new TierPrice
                {
                    ProductId = model.ProductId,
                    StoreId = model.StoreId,
                    CustomerRoleId = model.CustomerRoleId,
                    Quantity = model.Quantity,
                    Price = model.Price,
                    StartDateTimeUtc = model.StartDateTimeUtc,
                    EndDateTimeUtc = model.EndDateTimeUtc
                };
                _productService.InsertTierPrice(tierPrice);

                //update "HasTierPrices" property
                _productService.UpdateHasTierPricesProperty(product.Id);

                ViewBag.RefreshPage = true;

                return View(model);
            }

            //If we got this far, something failed, redisplay form

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var store in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = store.Name, Value = store.Id.ToString() });

            //customer roles
            model.AvailableCustomerRoles.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var role in _customerService.GetAllCustomerRoles(true))
                model.AvailableCustomerRoles.Add(new SelectListItem { Text = role.Name, Value = role.Id.ToString() });

            return View(model);
        }

        public IActionResult TierPriceEditPopup(string id, string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var tierPrice = product.TierPrices.Where(x => x.Id == id).FirstOrDefault();
            if (tierPrice == null)
                return RedirectToAction("List", "Product");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            var model = new ProductModel.TierPriceModel
            {
                Id = tierPrice.Id,
                CustomerRoleId = tierPrice.CustomerRoleId,
                StoreId = tierPrice.StoreId,
                Quantity = tierPrice.Quantity,
                Price = tierPrice.Price,
                StartDateTimeUtc = tierPrice.StartDateTimeUtc,
                EndDateTimeUtc = tierPrice.EndDateTimeUtc,
                ProductId = productId
            };

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var store in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = store.Name, Value = store.Id.ToString() });

            //customer roles
            model.AvailableCustomerRoles.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var role in _customerService.GetAllCustomerRoles(true))
                model.AvailableCustomerRoles.Add(new SelectListItem { Text = role.Name, Value = role.Id.ToString() });

            return View(model);
        }

        [HttpPost]
        public IActionResult TierPriceEditPopup(string productId, ProductModel.TierPriceModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var tierPrice = product.TierPrices.Where(x => x.Id == model.Id).FirstOrDefault();
            if (tierPrice == null)
                return RedirectToAction("List", "Product");


            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            if (ModelState.IsValid)
            {
                tierPrice.StoreId = model.StoreId;
                tierPrice.CustomerRoleId = model.CustomerRoleId;
                tierPrice.Quantity = model.Quantity;
                tierPrice.Price = model.Price;
                tierPrice.StartDateTimeUtc = model.StartDateTimeUtc;
                tierPrice.EndDateTimeUtc = model.EndDateTimeUtc;
                tierPrice.ProductId = productId;
                _productService.UpdateTierPrice(tierPrice);

                ViewBag.RefreshPage = true;
                return View(model);
            }

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var store in _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = store.Name, Value = store.Id.ToString() });

            //customer roles
            model.AvailableCustomerRoles.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var role in _customerService.GetAllCustomerRoles(true))
                model.AvailableCustomerRoles.Add(new SelectListItem { Text = role.Name, Value = role.Id.ToString() });

            return View(model);
        }

        [HttpPost]
        public IActionResult TierPriceDelete(string id, string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var tierPrice = product.TierPrices.Where(x => x.Id == id).FirstOrDefault();
            if (tierPrice == null)
                throw new ArgumentException("No tier price found with the specified id");
            tierPrice.ProductId = product.Id;

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            _productService.DeleteTierPrice(tierPrice);

            //update "HasTierPrices" property
            _productService.UpdateHasTierPricesProperty(product.Id);

            return new NullJsonResult();
        }

        #endregion

        #region Product attributes

        [HttpPost]
        public IActionResult ProductAttributeMappingList(DataSourceRequest command, string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            var attributes = product.ProductAttributeMappings.OrderBy(x => x.DisplayOrder);
            var attributesModel = attributes
                .Select(x =>
                {
                    var attributeModel = new ProductModel.ProductAttributeMappingModel
                    {
                        Id = x.Id,
                        ProductId = product.Id,
                        ProductAttribute = _productAttributeService.GetProductAttributeById(x.ProductAttributeId).Name,
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

            var gridModel = new DataSourceResult
            {
                Data = attributesModel,
                Total = attributesModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ProductAttributeMappingInsert(ProductModel.ProductAttributeMappingModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(model.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
            {
                return Content("This is not your product");
            }

            //insert mapping
            var productAttributeMapping = new ProductAttributeMapping
            {
                ProductId = model.ProductId,
                ProductAttributeId = model.ProductAttributeId,
                TextPrompt = model.TextPrompt,
                IsRequired = model.IsRequired,
                AttributeControlTypeId = model.AttributeControlTypeId,
                DisplayOrder = model.DisplayOrder,
            };


            //predefined values
            var predefinedValues = _productAttributeService.GetPredefinedProductAttributeValues(model.ProductAttributeId);
            foreach (var predefinedValue in predefinedValues)
            {
                var pav = new ProductAttributeValue
                {
                    ProductAttributeMappingId = productAttributeMapping.Id,
                    AttributeValueType = AttributeValueType.Simple,
                    Name = predefinedValue.Name,
                    PriceAdjustment = predefinedValue.PriceAdjustment,
                    WeightAdjustment = predefinedValue.WeightAdjustment,
                    Cost = predefinedValue.Cost,
                    IsPreSelected = predefinedValue.IsPreSelected,
                    DisplayOrder = predefinedValue.DisplayOrder,
                    ProductId = model.ProductId,
                };
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

            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult ProductAttributeMappingUpdate(ProductModel.ProductAttributeMappingModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(model.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var productAttributeMapping = product.ProductAttributeMappings.Where(x => x.Id == model.Id).FirstOrDefault();
            if (productAttributeMapping == null)
                throw new ArgumentException("No product attribute mapping found with the specified id");


            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            productAttributeMapping.ProductId = model.ProductId;
            productAttributeMapping.ProductAttributeId = model.ProductAttributeId;
            productAttributeMapping.TextPrompt = model.TextPrompt;
            productAttributeMapping.IsRequired = model.IsRequired;
            productAttributeMapping.AttributeControlTypeId = model.AttributeControlTypeId;
            productAttributeMapping.DisplayOrder = model.DisplayOrder;
            _productAttributeService.UpdateProductAttributeMapping(productAttributeMapping);

            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult ProductAttributeMappingDelete(string id, string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var productAttributeMapping = product.ProductAttributeMappings.Where(x => x.Id == id).FirstOrDefault();
            if (productAttributeMapping == null)
                throw new ArgumentException("No product attribute mapping found with the specified id");

            productAttributeMapping.ProductId = productId;
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            _productAttributeService.DeleteProductAttributeMapping(productAttributeMapping);

            return new NullJsonResult();
        }

        //edit
        public IActionResult ProductAttributeValidationRulesPopup(string id, string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var productAttributeMapping = product.ProductAttributeMappings.Where(x => x.Id == id).FirstOrDefault();
            if (productAttributeMapping == null)
                //No attribute value found with the specified id
                return RedirectToAction("List", "Product");


            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

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
            return View(model);
        }

        [HttpPost]
        public IActionResult ProductAttributeValidationRulesPopup(ProductModel.ProductAttributeMappingModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(model.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var productAttributeMapping = product.ProductAttributeMappings.Where(x => x.Id == model.Id).FirstOrDefault();
            if (productAttributeMapping == null)
                //No attribute value found with the specified id
                return RedirectToAction("List", "Product");


            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            if (ModelState.IsValid)
            {
                productAttributeMapping.ProductId = model.ProductId;
                productAttributeMapping.ValidationMinLength = model.ValidationMinLength;
                productAttributeMapping.ValidationMaxLength = model.ValidationMaxLength;
                productAttributeMapping.ValidationFileAllowedExtensions = model.ValidationFileAllowedExtensions;
                productAttributeMapping.ValidationFileMaximumSize = model.ValidationFileMaximumSize;
                productAttributeMapping.DefaultValue = model.DefaultValue;
                _productAttributeService.UpdateProductAttributeMapping(productAttributeMapping);

                ViewBag.RefreshPage = true;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            model.ValidationRulesAllowed = productAttributeMapping.ValidationRulesAllowed();
            model.AttributeControlTypeId = productAttributeMapping.AttributeControlTypeId;
            return View(model);
        }

        #endregion


        #region Product attributes. Condition

        public IActionResult ProductAttributeConditionPopup(string productId, string productAttributeMappingId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();
            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var productAttributeMapping = product.ProductAttributeMappings.Where(x => x.Id == productAttributeMappingId).FirstOrDefault();
            if (productAttributeMapping == null)
                //No attribute value found with the specified id
                return RedirectToAction("List", "Product");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");


            var model = new ProductAttributeConditionModel();
            model.ProductAttributeMappingId = productAttributeMapping.Id;
            model.EnableCondition = !String.IsNullOrEmpty(productAttributeMapping.ConditionAttributeXml);
            model.ProductId = productId;

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

            return View(model);
        }

        [HttpPost]
        public IActionResult ProductAttributeConditionPopup(ProductAttributeConditionModel model, IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(model.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var productAttributeMapping = product.ProductAttributeMappings.Where(x => x.Id == model.ProductAttributeMappingId).FirstOrDefault();
            if (productAttributeMapping == null)
                //No attribute value found with the specified id
                return RedirectToAction("List", "Product");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

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

            ViewBag.RefreshPage = true;
            return View(model);
        }

        #endregion


        #region Product attribute values

        //list
        public IActionResult EditAttributeValues(string productAttributeMappingId, string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var productAttributeMapping = product.ProductAttributeMappings.Where(x => x.Id == productAttributeMappingId).FirstOrDefault();
            if (productAttributeMapping == null)
                throw new ArgumentException("No product attribute mapping found with the specified id");


            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            var productAttribute = _productAttributeService.GetProductAttributeById(productAttributeMapping.ProductAttributeId);
            var model = new ProductModel.ProductAttributeValueListModel
            {
                ProductName = product.Name,
                ProductId = product.Id,
                ProductAttributeName = productAttribute.Name,
                ProductAttributeMappingId = productAttributeMappingId,
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult ProductAttributeValueList(string productAttributeMappingId, string productId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var productAttributeMapping = product.ProductAttributeMappings.Where(x => x.Id == productAttributeMappingId).FirstOrDefault();
            if (productAttributeMapping == null)
                throw new ArgumentException("No product attribute mapping found with the specified id");



            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            var values = productAttributeMapping.ProductAttributeValues.OrderBy(x => x.DisplayOrder);
            var gridModel = new DataSourceResult
            {
                Data = values.Select(x =>
                {
                    Product associatedProduct = null;
                    if (x.AttributeValueType == AttributeValueType.AssociatedToProduct)
                    {
                        associatedProduct = _productService.GetProductById(x.AssociatedProductId);
                    }

                    var pictureThumbnailUrl = _pictureService.GetPictureUrl(string.IsNullOrEmpty(x.PictureId) ? x.ImageSquaresPictureId:x.PictureId, 75, false);

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
                        ProductId = productId,
                    };
                }),
                Total = values.Count()
            };

            return Json(gridModel);
        }

        //create
        public IActionResult ProductAttributeValueCreatePopup(string productAttributeMappingId, string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var productAttributeMapping = product.ProductAttributeMappings.Where(x => x.Id == productAttributeMappingId).FirstOrDefault();
            if (productAttributeMapping == null)
                throw new ArgumentException("No product attribute mapping found with the specified id");


            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            var model = new ProductModel.ProductAttributeValueModel();
            model.ProductAttributeMappingId = productAttributeMappingId;
            model.ProductId = productId;

            //color squares
            model.DisplayColorSquaresRgb = productAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares;
            model.ColorSquaresRgb = "#000000";
            //image squares
            model.DisplayImageSquaresPicture = productAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares;

            //default qantity for associated product
            model.Quantity = 1;

            //locales
            AddLocales(_languageService, model.Locales);

            //pictures
            model.ProductPictureModels = product.ProductPictures
                .Select(x => new ProductModel.ProductPictureModel
                {
                    Id = x.Id,
                    ProductId = product.Id,
                    PictureId = x.PictureId,
                    PictureUrl = _pictureService.GetPictureUrl(x.PictureId),
                    DisplayOrder = x.DisplayOrder
                })
                .ToList();

            return View(model);
        }

        [HttpPost]
        public IActionResult ProductAttributeValueCreatePopup(ProductModel.ProductAttributeValueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(model.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var productAttributeMapping = product.ProductAttributeMappings.Where(x => x.Id == model.ProductAttributeMappingId).FirstOrDefault();
            if (productAttributeMapping == null)
                //No product attribute found with the specified id
                return RedirectToAction("List", "Product");


            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            if (productAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares)
            {
                //ensure valid color is chosen/entered
                if (String.IsNullOrEmpty(model.ColorSquaresRgb))
                    ModelState.AddModelError("", "Color is required");
                //TO DO
                //try
                //{
                //    //ensure color is valid (can be instanciated)
                //    System.Drawing.ColorTranslator.FromHtml(model.ColorSquaresRgb);
                //}
                //catch (Exception exc)
                //{
                //    ModelState.AddModelError("", exc.Message);
                //}
            }

            //ensure a picture is uploaded
            if (productAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares && String.IsNullOrEmpty(model.ImageSquaresPictureId))
            {
                ModelState.AddModelError("", "Image is required");
            }

            if (ModelState.IsValid)
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
                pav.Locales = UpdateLocales(pav, model);

                _productAttributeService.InsertProductAttributeValue(pav);
                ViewBag.RefreshPage = true;
                return View(model);
            }

            //If we got this far, something failed, redisplay form


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

            return View(model);
        }

        //edit
        public IActionResult ProductAttributeValueEditPopup(string id, string productId, string productAttributeMappingId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var pa = product.ProductAttributeMappings.Where(x => x.Id == productAttributeMappingId).FirstOrDefault();
            if (pa == null)
                return RedirectToAction("List", "Product");

            var pav = pa.ProductAttributeValues.Where(x => x.Id == id).FirstOrDefault();
            if (pav == null)
                //No attribute value found with the specified id
                return RedirectToAction("List", "Product");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

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
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = pav.GetLocalized(x => x.Name, languageId, false, false);
            });
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

            return View(model);
        }

        [HttpPost]
        public IActionResult ProductAttributeValueEditPopup(string productId, ProductModel.ProductAttributeValueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var pav = product.ProductAttributeMappings.Where(x => x.Id == model.ProductAttributeMappingId).FirstOrDefault().ProductAttributeValues.Where(x => x.Id == model.Id).FirstOrDefault();
            if (pav == null)
                //No attribute value found with the specified id
                return RedirectToAction("List", "Product");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");
            var productAttributeMapping = product.ProductAttributeMappings.Where(x => x.Id == model.ProductAttributeMappingId).FirstOrDefault();
            if (productAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares)
            {
                //ensure valid color is chosen/entered
                if (String.IsNullOrEmpty(model.ColorSquaresRgb))
                    ModelState.AddModelError("", "Color is required");
            }

            //ensure a picture is uploaded
            if (productAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares && String.IsNullOrEmpty(model.ImageSquaresPictureId))
            {
                ModelState.AddModelError("", "Image is required");
            }

            if (ModelState.IsValid)
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
                pav.Locales = UpdateLocales(pav, model);

                _productAttributeService.UpdateProductAttributeValue(pav);

                ViewBag.RefreshPage = true;

                return View(model);
            }

            //If we got this far, something failed, redisplay form

            //pictures
            model.ProductPictureModels = product.ProductPictures
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

            return View(model);
        }

        //delete
        [HttpPost]
        public IActionResult ProductAttributeValueDelete(string Id, string pam, string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var pav = product.ProductAttributeMappings.Where(x => x.Id == pam).FirstOrDefault().ProductAttributeValues.Where(x => x.Id == Id).FirstOrDefault();
            if (pav == null)
                throw new ArgumentException("No product attribute value found with the specified id");

            pav.ProductAttributeMappingId = pam;
            pav.ProductId = productId;
            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            _productAttributeService.DeleteProductAttributeValue(pav);

            return new NullJsonResult();
        }





        public IActionResult AssociateProductToAttributeValuePopup()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

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

            return View(model);
        }

        [HttpPost]
        public IActionResult AssociateProductToAttributeValuePopupList(DataSourceRequest command,
            ProductModel.ProductAttributeValueModel.AssociateProductToAttributeValueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null)
            {
                model.SearchVendorId = _workContext.CurrentVendor.Id;
            }

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
        public IActionResult AssociateProductToAttributeValuePopup(ProductModel.ProductAttributeValueModel.AssociateProductToAttributeValueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var associatedProduct = _productService.GetProductById(model.AssociatedToProductId);
            if (associatedProduct == null)
                return Content("Cannot load a product");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && associatedProduct.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            //a vendor should have access only to his products
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;
            ViewBag.RefreshPage = true;
            ViewBag.productId = associatedProduct.Id;
            ViewBag.productName = associatedProduct.Name;
            return View(model);
        }


        #endregion

        #region Product attribute combinations

        [HttpPost]
        public IActionResult ProductAttributeCombinationList(DataSourceRequest command, string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            var combinations = product.ProductAttributeCombinations;
            var combinationsModel = combinations
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
                    var warnings = _shoppingCartService.GetShoppingCartItemAttributeWarnings(_workContext.CurrentCustomer,
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

            var gridModel = new DataSourceResult
            {
                Data = combinationsModel,
                Total = combinationsModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ProductAttributeCombinationDelete(string id, string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            var combination = product.ProductAttributeCombinations.Where(x => x.Id == id).FirstOrDefault();
            if (combination == null)
                throw new ArgumentException("No product attribute combination found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            combination.ProductId = productId;

            _productAttributeService.DeleteProductAttributeCombination(combination);

            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
            {
                var pr = _productService.GetProductById(productId);
                pr.StockQuantity = pr.ProductAttributeCombinations.Sum(x => x.StockQuantity);
                _productService.UpdateStockProduct(pr);
            }

            return new NullJsonResult();
        }

        //edit
        public IActionResult AttributeCombinationPopup(string productId, string Id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                //No product found with the specified id
                return RedirectToAction("List", "Product");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

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

            if (!string.IsNullOrEmpty(Id))
            {
                var combination = product.ProductAttributeCombinations.FirstOrDefault(x => x.Id == Id);
                if (combination != null)
                {
                    model = combination.ToModel();
                    model.UseMultipleWarehouses = product.UseMultipleWarehouses;
                    model.WarehouseInventoryModels = wim;
                    model.ProductId = productId;
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
            PrepareAddProductAttributeCombinationModel(model, product);
            return View(model);
        }

        [HttpPost]

        public IActionResult AttributeCombinationPopup(string productId,
            ProductAttributeCombinationModel model, IFormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                //No product found with the specified id
                return RedirectToAction("List", "Product");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return RedirectToAction("List", "Product");

            //attributes
            string attributesXml = "";
            var warnings = new List<string>();

            void PrepareCombinationWarehouseInventory(ProductAttributeCombination combination)
            {
                var warehouses = _shippingService.GetAllWarehouses();

                foreach (var warehouse in warehouses)
                {
                    //parse stock quantity
                    int stockQuantity = 0;
                    foreach (string formKey in this.Request.Form.Keys)
                        if (formKey.Equals(string.Format("warehouse_qty_{0}", warehouse.Id), StringComparison.OrdinalIgnoreCase))
                        {
                            int.TryParse(this.Request.Form[formKey], out stockQuantity);
                            break;
                        }
                    //parse reserved quantity
                    int reservedQuantity = 0;
                    foreach (string formKey in this.Request.Form.Keys)
                        if (formKey.Equals(string.Format("warehouse_reserved_{0}", warehouse.Id), StringComparison.OrdinalIgnoreCase))
                        {
                            int.TryParse(this.Request.Form[formKey], out reservedQuantity);
                            break;
                        }
                    //parse "used" field
                    bool used = false;
                    foreach (string formKey in this.Request.Form.Keys)
                        if (formKey.Equals(string.Format("warehouse_used_{0}", warehouse.Id), StringComparison.OrdinalIgnoreCase))
                        {
                            used = this.Request.Form[formKey] == warehouse.Id;
                            break;
                        }

                    var existingPwI = combination.WarehouseInventory.FirstOrDefault(x => x.WarehouseId == warehouse.Id);

                    if (existingPwI != null)
                    {
                        if (used)
                        {
                            //update 
                            existingPwI.StockQuantity = stockQuantity;
                            existingPwI.ReservedQuantity = reservedQuantity;
                        }
                        else
                        {
                            //delete 
                            combination.WarehouseInventory.Remove(existingPwI);
                        }
                    }
                    else
                    {
                        if (used)
                        {
                            //no need to insert a record for qty 0
                            existingPwI = new ProductCombinationWarehouseInventory
                            {
                                WarehouseId = warehouse.Id,
                                StockQuantity = stockQuantity,
                                ReservedQuantity = reservedQuantity
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
                    attribute.ProductId = productId;
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
                    attribute.ProductId = productId;
                    var conditionMet = _productAttributeParser.IsConditionMet(product, attribute, attributesXml);
                    if (conditionMet.HasValue && !conditionMet.Value)
                    {
                        attributesXml = _productAttributeParser.RemoveProductAttribute(attributesXml, attribute);
                    }
                }


                #endregion

                warnings.AddRange(_shoppingCartService.GetShoppingCartItemAttributeWarnings(_workContext.CurrentCustomer,
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
                        var pr = _productService.GetProductById(productId);
                        pr.StockQuantity = pr.ProductAttributeCombinations.Sum(x => x.StockQuantity);
                        _productService.UpdateStockProduct(pr);
                    }
                    ViewBag.RefreshPage = true;
                    return View(model);
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

                ViewBag.RefreshPage = true;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            PrepareAddProductAttributeCombinationModel(model, product);
            model.Warnings = warnings;
            return View(model);
        }

        [HttpPost]
        public IActionResult GenerateAllAttributeCombinations(string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

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
                warnings.AddRange(_shoppingCartService.GetShoppingCartItemAttributeWarnings(_workContext.CurrentCustomer,
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
                var pr = _productService.GetProductById(productId);
                pr.StockQuantity = pr.ProductAttributeCombinations.Sum(x => x.StockQuantity);
                _productService.UpdateStockProduct(pr);
            }
            return Json(new { Success = true });
        }

        [HttpPost]
        public IActionResult ClearAllAttributeCombinations(string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            foreach (var combination in product.ProductAttributeCombinations.ToList())
            {
                combination.ProductId = productId;
                _productAttributeService.DeleteProductAttributeCombination(combination);
            }

            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
            {
                product.StockQuantity = 0;
                _productService.UpdateStockProduct(product);
            }
            return Json(new { Success = true });
        }

        #region Product Attribute combination - tier prices

        [HttpPost]
        public IActionResult ProductAttributeCombinationTierPriceList(DataSourceRequest command, string productId, string productAttributeCombinationId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

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

            var gridModel = new DataSourceResult
            {
                Data = tierPriceModel,
                Total = tierPriceModel.Count
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult ProductAttributeCombinationTierPriceInsert(string productId, string productAttributeCombinationId, ProductModel.ProductAttributeCombinationTierPricesModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            if (!String.IsNullOrEmpty(model.CustomerRoleId))
                model.CustomerRoleId = model.CustomerRoleId.Trim();
            else
                model.CustomerRoleId = "";

            if (!String.IsNullOrEmpty(model.StoreId))
                model.StoreId = model.StoreId.Trim();
            else
                model.StoreId = "";

            var combination = product.ProductAttributeCombinations.FirstOrDefault(x => x.Id == productAttributeCombinationId);
            if (combination != null)
            {
                ProductCombinationTierPrices pctp = new ProductCombinationTierPrices();
                pctp.Price = model.Price;
                pctp.Quantity = model.Quantity;
                pctp.StoreId = model.StoreId;
                pctp.CustomerRoleId = model.CustomerRoleId;
                combination.TierPrices.Add(pctp);
                combination.ProductId = productId;
                combination.Id = productAttributeCombinationId;
                _productAttributeService.UpdateProductAttributeCombination(combination);
            }
            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult ProductAttributeCombinationTierPriceUpdate(string productId, string productAttributeCombinationId, ProductModel.ProductAttributeCombinationTierPricesModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            if (!String.IsNullOrEmpty(model.CustomerRoleId))
                model.CustomerRoleId = model.CustomerRoleId.Trim();
            else
                model.CustomerRoleId = "";

            if (!String.IsNullOrEmpty(model.StoreId))
                model.StoreId = model.StoreId.Trim();
            else
                model.StoreId = "";

            var combination = product.ProductAttributeCombinations.FirstOrDefault(x => x.Id == productAttributeCombinationId);
            if (combination != null)
            {
                var tierPrice = combination.TierPrices.FirstOrDefault(x => x.Id == model.Id);
                if (tierPrice != null)
                {
                    tierPrice.Price = model.Price;
                    tierPrice.Quantity = model.Quantity;
                    tierPrice.StoreId = model.StoreId;
                    tierPrice.CustomerRoleId = model.CustomerRoleId;
                    combination.ProductId = productId;
                    combination.Id = productAttributeCombinationId;
                    _productAttributeService.UpdateProductAttributeCombination(combination);
                }
            }
            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult ProductAttributeCombinationTierPriceDelete(string productId, string productAttributeCombinationId, string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            if (_workContext.CurrentVendor != null && product.VendorId != _workContext.CurrentVendor.Id)
                return Content("This is not your product");

            var combination = product.ProductAttributeCombinations.FirstOrDefault(x => x.Id == productAttributeCombinationId);
            if (combination != null)
            {
                var tierPrice = combination.TierPrices.FirstOrDefault(x => x.Id == id);
                if (tierPrice != null)
                {
                    combination.TierPrices.Remove(tierPrice);
                    combination.ProductId = productId;
                    combination.Id = productAttributeCombinationId;
                    _productAttributeService.UpdateProductAttributeCombination(combination);
                }
            }
            return new NullJsonResult();
        }

        #endregion
        #endregion

        #endregion

        #region Activity log

        [HttpPost]
        public IActionResult ListActivityLog(DataSourceRequest command, string productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return Content("");

            var activityLog = _customerActivityService.GetProductActivities(null, null, productId, command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = activityLog.Select(x =>
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

                }),
                Total = activityLog.TotalCount
            };

            return Json(gridModel);
        }

        #endregion

        #region Reservation

        [HttpPost]
        public IActionResult ListReservations(DataSourceRequest command, string productId)
        {
            var reservations = _productReservationService.GetProductReservationsByProductId(productId, null, null, command.Page - 1, command.PageSize);
            var reservationModel = reservations
                .Select(x => new ProductModel.ReservationModel
                {
                    ReservationId = x.Id,
                    Date = x.Date,
                    OrderId = x.OrderId,
                    Parameter = x.Parameter,
                    Resource = x.Resource,
                    Duration = x.Duration
                }).ToList();

            var gridModel = new DataSourceResult
            {
                Data = reservationModel,
                Total = reservations.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult GenerateCalendar(string productId, ProductModel.GenerateCalendarModel model)
        {
            var reservations = _productReservationService.GetProductReservationsByProductId(productId, null, null);
            if (reservations.Any())
            {
                var product = _productService.GetProductById(productId);
                if (product == null)
                    throw new ArgumentNullException("product");

                if (((product.IntervalUnitType == IntervalUnit.Minute || product.IntervalUnitType == IntervalUnit.Hour) && (IntervalUnit)model.Interval == IntervalUnit.Day) ||
                    (product.IntervalUnitType == IntervalUnit.Day) && (((IntervalUnit)model.IntervalUnit == IntervalUnit.Minute || (IntervalUnit)model.IntervalUnit == IntervalUnit.Hour)))
                {
                    return Json(new { errors = _localizationService.GetResource("Admin.Catalog.Products.Calendar.CannotChangeInterval") });
                }
            }
            _productService.UpdateIntervalProperties(productId, model.Interval, (IntervalUnit)model.IntervalUnit, model.IncBothDate);

            if (!ModelState.IsValid)
            {
                Dictionary<string, Dictionary<string, object>> error = (Dictionary<string, Dictionary<string, object>>)ModelState.SerializeErrors();
                string s = "";
                foreach (var error1 in error)
                {
                    foreach (var error2 in error1.Value)
                    {
                        string[] v = (string[])error2.Value;
                        s += v[0] + "\n";
                    }
                }

                return Json(new { errors = s });
            }

            int minutesToAdd = 0;
            if ((IntervalUnit)model.IntervalUnit == IntervalUnit.Minute)
            {
                minutesToAdd = model.Interval;
            }
            else if ((IntervalUnit)model.IntervalUnit == IntervalUnit.Hour)
            {
                minutesToAdd = model.Interval * 60;
            }
            else if ((IntervalUnit)model.IntervalUnit == IntervalUnit.Day)
            {
                minutesToAdd = model.Interval * 60 * 24;
            }

            int _hourFrom = model.StartTime.Hour;
            int _minutesFrom = model.StartTime.Minute;
            int _hourTo = model.EndTime.Hour;
            int _minutesTo = model.EndTime.Minute;
            DateTime _dateFrom = new DateTime(model.StartDateUtc.Year, model.StartDateUtc.Month, model.StartDateUtc.Day, 0, 0, 0, 0);
            DateTime _dateTo = new DateTime(model.EndDateUtc.Year, model.EndDateUtc.Month, model.EndDateUtc.Day, 23, 59, 59, 999);
            if ((IntervalUnit)model.IntervalUnit == IntervalUnit.Day)
            {
                model.Quantity = 1;
                model.Parameter = "";
            }
            else
            {
                model.Resource = "";
            }

            List<DateTime> dates = new List<DateTime>();
            int counter = 0;
            for (DateTime iterator = _dateFrom; iterator <= _dateTo; iterator += new TimeSpan(0, minutesToAdd, 0))
            {
                if ((IntervalUnit)model.IntervalUnit != IntervalUnit.Day)
                {
                    if (iterator.Hour >= _hourFrom && iterator.Hour <= _hourTo)
                    {
                        if (iterator.Hour == _hourTo)
                        {
                            if (iterator.Minute > _minutesTo)
                            {
                                continue;
                            }
                        }
                        if (iterator.Hour == _hourFrom)
                        {
                            if (iterator.Minute < _minutesFrom)
                            {
                                continue;
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }
                }

                if ((iterator.DayOfWeek == DayOfWeek.Monday && !model.Monday) ||
                   (iterator.DayOfWeek == DayOfWeek.Tuesday && !model.Tuesday) ||
                   (iterator.DayOfWeek == DayOfWeek.Wednesday && !model.Wednesday) ||
                   (iterator.DayOfWeek == DayOfWeek.Thursday && !model.Thursday) ||
                   (iterator.DayOfWeek == DayOfWeek.Friday && !model.Friday) ||
                   (iterator.DayOfWeek == DayOfWeek.Saturday && !model.Saturday) ||
                   (iterator.DayOfWeek == DayOfWeek.Sunday && !model.Sunday))
                {
                    continue;
                }

                for (int i = 0; i < model.Quantity; i++)
                {
                    dates.Add(iterator);
                    try
                    {
                        var insert = true;
                        if (((IntervalUnit)model.IntervalUnit) == IntervalUnit.Day)
                        {
                            if (reservations.Where(x => x.Resource == model.Resource && x.Date == iterator).Any())
                                insert = false;
                        }


                        if (insert)
                        {
                            if (counter++ > 1000)
                                break;

                            _productReservationService.InsertProductReservation(new ProductReservation
                            {
                                OrderId = "",
                                Date = iterator,
                                ProductId = productId,
                                Resource = model.Resource,
                                Parameter = model.Parameter,
                                Duration = model.Interval + " " + ((IntervalUnit)model.IntervalUnit).GetLocalizedEnum(_localizationService, _workContext),
                            });
                        }
                    }
                    catch { }
                }
            }

            return Json(new { success = true });
        }

        public IActionResult ClearCalendar(string productId)
        {
            var toDelete = _productReservationService.GetProductReservationsByProductId(productId, true, null);
            foreach (var record in toDelete)
            {
                _productReservationService.DeleteProductReservation(record);
            }

            return Json("");
        }

        public IActionResult ClearOld(string productId)
        {
            var toDelete = _productReservationService.GetProductReservationsByProductId(productId, true, null).Where(x => x.Date < DateTime.UtcNow);
            foreach (var record in toDelete)
            {
                _productReservationService.DeleteProductReservation(record);
            }

            return Json("");
        }

        [HttpPost]
        public IActionResult ProductReservationDelete(ProductModel.ReservationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var toDelete = _productReservationService.GetProductReservation(model.ReservationId);
            if (toDelete != null)
            {
                if (string.IsNullOrEmpty(toDelete.OrderId))
                    _productReservationService.DeleteProductReservation(toDelete);
                else
                    return Json(new DataSourceResult { Errors = _localizationService.GetResource("Admin.Catalog.ProductReservations.CantDeleteWithOrder") });
            }

            return Json("");
        }

        #endregion

        #region Bids

        [HttpPost]
        public IActionResult ListBids(DataSourceRequest command, string productId)
        {
            var bids = _auctionService.GetBidsByProductId(productId, command.Page - 1, command.PageSize);
            var bidsModel = bids
                .Select(x => new ProductModel.BidModel
                {
                    BidId = x.Id,
                    Amount = _priceFormatter.FormatPrice(x.Amount),
                    Date = _dateTimeHelper.ConvertToUserTime(x.Date, DateTimeKind.Utc),
                    CustomerId = x.CustomerId,
                    Email = _customerService.GetCustomerById(x.CustomerId)?.Email,
                    OrderId = x.OrderId
                }).ToList();

            var gridModel = new DataSourceResult
            {
                Data = bidsModel,
                Total = bids.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult BidDelete(ProductModel.BidModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var toDelete = _auctionService.GetBid(model.BidId);
            if (toDelete != null)
            {
                var product = _productService.GetProductById(toDelete.ProductId);
                if (product == null)
                    return Json(new DataSourceResult { Errors = "Product not exists" });

                if (string.IsNullOrEmpty(toDelete.OrderId))
                {
                    //activity log
                    _customerActivityService.InsertActivity("DeleteBid", toDelete.ProductId, _localizationService.GetResource("ActivityLog.DeleteBid"), product.Name);

                    //delete bid
                    _auctionService.DeleteBid(toDelete);
                    return Json("");
                }
                else
                    return Json(new DataSourceResult { Errors = _localizationService.GetResource("Admin.Catalog.Products.Bids.CantDeleteWithOrder") });
            }
            return Json(new DataSourceResult { Errors = "Bid not exists" });
        }

        #endregion
    }
}