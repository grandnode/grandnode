using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Media;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Seo;
using Grand.Core.Domain.Vendors;
using Grand.Core.Infrastructure;
using Grand.Framework.Security.Captcha;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Discounts;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Messages;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Shipping;
using Grand.Services.Stores;
using Grand.Services.Tax;
using Grand.Services.Vendors;
using Grand.Web.Extensions;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Models.Catalog;
using Grand.Web.Models.Media;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;

namespace Grand.Web.Services
{
    public partial class ProductViewModelService: IProductViewModelService
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly IProductService _productService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IMeasureService _measureService;
        private readonly ICacheManager _cacheManager;
        private readonly IPictureService _pictureService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IWebHelper _webHelper;
        private readonly IProductTemplateService _productTemplateService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IShippingService _shippingService;
        private readonly IVendorService _vendorService;
        private readonly ICategoryService _categoryService;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IProductTagService _productTagService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IDownloadService _downloadService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IProductReservationService _productReservationService;

        private readonly MediaSettings _mediaSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly SeoSettings _seoSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly LocalizationSettings _localizationSettings;

        public ProductViewModelService(IPermissionService permissionService, IWorkContext workContext, IStoreContext storeContext,
            ILocalizationService localizationService, IProductService productService, IPriceCalculationService priceCalculationService,
            ITaxService taxService, ICurrencyService currencyService, IPriceFormatter priceFormatter, IMeasureService measureService,
            ICacheManager cacheManager, IPictureService pictureService, ISpecificationAttributeService specificationAttributeService, IWebHelper webHelper,
            IProductTemplateService productTemplateService, IProductAttributeParser productAttributeParser, IShippingService shippingService,
            IVendorService vendorService, ICategoryService categoryService, IAclService aclService, IStoreMappingService storeMappingService,
            IProductTagService productTagService, IProductAttributeService productAttributeService, IManufacturerService manufacturerService,
            IDateTimeHelper dateTimeHelper, IDownloadService downloadService, IWorkflowMessageService workflowMessageService, IProductReservationService productReservationService,
            MediaSettings mediaSettings, CatalogSettings catalogSettings, SeoSettings seoSettings, VendorSettings vendorSettings, CustomerSettings customerSettings,
            CaptchaSettings captchaSettings, LocalizationSettings localizationSettings)
        {
            this._permissionService = permissionService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._localizationService = localizationService;
            this._productService = productService;
            this._priceCalculationService = priceCalculationService;
            this._taxService = taxService;
            this._currencyService = currencyService;
            this._priceFormatter = priceFormatter;
            this._measureService = measureService;
            this._cacheManager = cacheManager;
            this._pictureService = pictureService;
            this._specificationAttributeService = specificationAttributeService;
            this._webHelper = webHelper;
            this._productTemplateService = productTemplateService;
            this._productAttributeParser = productAttributeParser;
            this._shippingService = shippingService;
            this._vendorService = vendorService;
            this._categoryService = categoryService;
            this._aclService = aclService;
            this._storeMappingService = storeMappingService;
            this._productTagService = productTagService;
            this._productAttributeService = productAttributeService;
            this._manufacturerService = manufacturerService;
            this._dateTimeHelper = dateTimeHelper;
            this._downloadService = downloadService;
            this._workflowMessageService = workflowMessageService;
            this._productReservationService = productReservationService;

            this._mediaSettings = mediaSettings;
            this._catalogSettings = catalogSettings;
            this._seoSettings = seoSettings;
            this._vendorSettings = vendorSettings;
            this._customerSettings = customerSettings;
            this._captchaSettings = captchaSettings;
            this._localizationSettings = localizationSettings;
        }

        public virtual IEnumerable<ProductOverviewModel> PrepareProductOverviewModels(
            IEnumerable<Product> products,
            bool preparePriceModel = true, bool preparePictureModel = true,
            int? productThumbPictureSize = null, bool prepareSpecificationAttributes = false,
            bool forceRedirectionAfterAddingToCart = false)
        {
            if (products == null)
                throw new ArgumentNullException("products");

            var currentCustomer = _workContext.CurrentCustomer;
            var displayPrices = _permissionService.Authorize(StandardPermissionProvider.DisplayPrices, currentCustomer);
            var enableShoppingCart = _permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart, currentCustomer);
            var enableWishlist = _permissionService.Authorize(StandardPermissionProvider.EnableWishlist, currentCustomer);
            var currentCurrency = _workContext.WorkingCurrency;
            var currentStoreId = _storeContext.CurrentStore.Id;
            var currentLanguageId = _workContext.WorkingLanguage;
            int pictureSize = productThumbPictureSize.HasValue ? productThumbPictureSize.Value : _mediaSettings.ProductThumbPictureSize;
            var connectionSecured = _webHelper.IsCurrentConnectionSecured();
            var showSku = _catalogSettings.ShowSkuOnCatalogPages;
            var taxDisplay = _workContext.TaxDisplayType;
            var showQty = _catalogSettings.DisplayQuantityOnCatalogPages;

            var res = new Dictionary<string, string>
            {
                { "Products.CallForPrice", _localizationService.GetResource("Products.CallForPrice", currentLanguageId.Id) },
                { "Products.PriceRangeFrom", _localizationService.GetResource("Products.PriceRangeFrom", currentLanguageId.Id)},
                { "Media.Product.ImageLinkTitleFormat", _localizationService.GetResource("Media.Product.ImageLinkTitleFormat", currentLanguageId.Id) },
                { "Media.Product.ImageAlternateTextFormat", _localizationService.GetResource("Media.Product.ImageAlternateTextFormat", currentLanguageId.Id) }
            };
            
            var models = new List<ProductOverviewModel>();

            foreach (var product in products)
            {
                var model = new ProductOverviewModel
                {
                    Id = product.Id,
                    Name = product.GetLocalized(x => x.Name, currentLanguageId.Id),
                    ShortDescription = product.GetLocalized(x => x.ShortDescription, currentLanguageId.Id),
                    FullDescription = product.GetLocalized(x => x.FullDescription, currentLanguageId.Id),
                    SeName = product.GetSeName(currentLanguageId.Id),
                    ProductType = product.ProductType,
                    Sku = product.Sku,
                    Gtin = product.Gtin,
                    Flag = product.Flag,
                    ManufacturerPartNumber = product.ManufacturerPartNumber,
                    IsFreeShipping = product.IsFreeShipping,
                    ShowSku = showSku,
                    TaxDisplayType = taxDisplay,
                    EndTime = product.AvailableEndDateTimeUtc,
                    ShowQty = showQty,
                    MarkAsNew = product.MarkAsNew &&
                        (!product.MarkAsNewStartDateTimeUtc.HasValue || product.MarkAsNewStartDateTimeUtc.Value < DateTime.UtcNow) &&
                        (!product.MarkAsNewEndDateTimeUtc.HasValue || product.MarkAsNewEndDateTimeUtc.Value > DateTime.UtcNow)
                };
                //price
                if (preparePriceModel)
                {
                    #region Prepare product price

                    var priceModel = new ProductOverviewModel.ProductPriceModel
                    {
                        ForceRedirectionAfterAddingToCart = forceRedirectionAfterAddingToCart
                    };

                    switch (product.ProductType)
                    {
                        case ProductType.GroupedProduct:
                            {
                                #region Grouped product

                                var associatedProducts = _productService.GetAssociatedProducts(product.Id, currentStoreId);

                                //add to cart button (ignore "DisableBuyButton" property for grouped products)
                                priceModel.DisableBuyButton = !enableShoppingCart || !displayPrices;

                                //add to wishlist button (ignore "DisableWishlistButton" property for grouped products)
                                priceModel.DisableWishlistButton = !enableWishlist || !displayPrices;

                                //compare products
                                priceModel.DisableAddToCompareListButton = !_catalogSettings.CompareProductsEnabled;

                                //catalog price, not used in views, but it's for front developer
                                if (product.CatalogPrice > 0)
                                {
                                    decimal catalogPrice = _currencyService.ConvertFromPrimaryStoreCurrency(product.CatalogPrice, currentCurrency);
                                    priceModel.CatalogPrice = _priceFormatter.FormatPrice(catalogPrice, true, currentCurrency);
                                }

                                switch (associatedProducts.Count)
                                {
                                    case 0:
                                        {

                                        }
                                        break;
                                    default:
                                        {
                                            //we have at least one associated product
                                            //compare products
                                            priceModel.DisableAddToCompareListButton = !_catalogSettings.CompareProductsEnabled;
                                            if (displayPrices)
                                            {
                                                //find a minimum possible price
                                                decimal? minPossiblePrice = null;
                                                Product minPriceProduct = null;
                                                foreach (var associatedProduct in associatedProducts)
                                                {
                                                    //calculate for the maximum quantity (in case if we have tier prices)
                                                    var tmpPrice = _priceCalculationService.GetFinalPrice(associatedProduct,
                                                        currentCustomer, decimal.Zero, true, int.MaxValue);
                                                    if (!minPossiblePrice.HasValue || tmpPrice < minPossiblePrice.Value)
                                                    {
                                                        minPriceProduct = associatedProduct;
                                                        minPossiblePrice = tmpPrice;
                                                    }
                                                }
                                                if (minPriceProduct != null && !minPriceProduct.CustomerEntersPrice)
                                                {
                                                    if (minPriceProduct.CallForPrice)
                                                    {
                                                        priceModel.OldPrice = null;
                                                        priceModel.Price = res["Products.CallForPrice"];
                                                    }
                                                    else if (minPossiblePrice.HasValue)
                                                    {
                                                        //calculate prices
                                                        decimal taxRate;
                                                        decimal finalPriceBase = _taxService.GetProductPrice(minPriceProduct, minPossiblePrice.Value, out taxRate);
                                                        decimal finalPrice = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceBase, currentCurrency);

                                                        priceModel.OldPrice = null;
                                                        priceModel.Price = String.Format(res["Products.PriceRangeFrom"], _priceFormatter.FormatPrice(finalPrice, true, currentCurrency));
                                                        priceModel.PriceValue = finalPrice;

                                                        //PAngV baseprice (used in Germany)
                                                        priceModel.BasePricePAngV = product.FormatBasePrice(finalPrice,
                                                            _localizationService, _measureService, _currencyService, _workContext, _priceFormatter);
                                                    }
                                                    else
                                                    {
                                                        //Actually it's not possible (we presume that minimalPrice always has a value)
                                                        //We never should get here
                                                        Debug.WriteLine("Cannot calculate minPrice for product #{0}", product.Id);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                //hide prices
                                                priceModel.OldPrice = null;
                                                priceModel.Price = null;
                                            }
                                        }
                                        break;
                                }

                                #endregion
                            }
                            break;
                        case ProductType.SimpleProduct:
                        case ProductType.Reservation:
                        default:
                            {
                                #region Simple product

                                //add to cart button
                                priceModel.DisableBuyButton = product.DisableBuyButton || !enableShoppingCart || !displayPrices;

                                //add to wishlist button
                                priceModel.DisableWishlistButton = product.DisableWishlistButton || !enableWishlist || !displayPrices;
                                //compare products
                                priceModel.DisableAddToCompareListButton = !_catalogSettings.CompareProductsEnabled;

                                //pre-order
                                if (product.AvailableForPreOrder)
                                {
                                    priceModel.AvailableForPreOrder = !product.PreOrderAvailabilityStartDateTimeUtc.HasValue ||
                                        product.PreOrderAvailabilityStartDateTimeUtc.Value >= DateTime.UtcNow;
                                    priceModel.PreOrderAvailabilityStartDateTimeUtc = product.PreOrderAvailabilityStartDateTimeUtc;
                                }

                                //catalog price, not used in views, but it's for front developer
                                if (product.CatalogPrice > 0)
                                {
                                    decimal catalogPrice = _currencyService.ConvertFromPrimaryStoreCurrency(product.CatalogPrice, currentCurrency);
                                    priceModel.CatalogPrice = _priceFormatter.FormatPrice(catalogPrice, true, currentCurrency);
                                }

                                //start price for product auction
                                if (product.StartPrice > 0)
                                {
                                    decimal startPrice = _currencyService.ConvertFromPrimaryStoreCurrency(product.StartPrice, currentCurrency);
                                    priceModel.StartPrice = _priceFormatter.FormatPrice(startPrice, true, currentCurrency);
                                    priceModel.StartPriceValue = startPrice;
                                }

                                //highest bid for product auction
                                if (product.HighestBid > 0)
                                {
                                    decimal highestBid = _currencyService.ConvertFromPrimaryStoreCurrency(product.HighestBid, currentCurrency);
                                    priceModel.HighestBid = _priceFormatter.FormatPrice(highestBid, true, currentCurrency);
                                    priceModel.HighestBidValue = highestBid;
                                }

                                //prices
                                if (displayPrices)
                                {
                                    if (!product.CustomerEntersPrice)
                                    {
                                        if (product.CallForPrice)
                                        {
                                            //call for price
                                            priceModel.OldPrice = null;
                                            priceModel.Price = res["Products.CallForPrice"];
                                        }
                                        else
                                        {
                                            //prices

                                            //calculate for the maximum quantity (in case if we have tier prices)
                                            decimal minPossiblePrice = _priceCalculationService.GetFinalPrice(product,
                                                currentCustomer, decimal.Zero, true, int.MaxValue);

                                            decimal taxRate;
                                            decimal oldPriceBase = _taxService.GetProductPrice(product, product.OldPrice, out taxRate);
                                            decimal finalPriceBase = _taxService.GetProductPrice(product, minPossiblePrice, out taxRate);

                                            decimal oldPrice = _currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, currentCurrency);
                                            decimal finalPrice = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceBase, currentCurrency);

                                            //do we have tier prices configured?
                                            var tierPrices = new List<TierPrice>();
                                            if (product.HasTierPrices)
                                            {
                                                tierPrices.AddRange(product.TierPrices.OrderBy(tp => tp.Quantity)
                                                    .FilterByStore(currentStoreId)
                                                    .FilterForCustomer(currentCustomer)
                                                    .FilterByDate()
                                                    .RemoveDuplicatedQuantities());
                                            }
                                            //When there is just one tier (with  qty 1), 
                                            //there are no actual savings in the list.
                                            bool displayFromMessage = tierPrices.Any() && !(tierPrices.Count == 1 && tierPrices[0].Quantity <= 1);
                                            if (displayFromMessage)
                                            {
                                                priceModel.OldPrice = null;
                                                priceModel.Price = String.Format(res["Products.PriceRangeFrom"], _priceFormatter.FormatPrice(finalPrice, true, currentCurrency));
                                                priceModel.PriceValue = finalPrice;
                                            }
                                            else
                                            {
                                                if (finalPriceBase != oldPriceBase && oldPriceBase != decimal.Zero)
                                                {
                                                    priceModel.OldPrice = _priceFormatter.FormatPrice(oldPrice, true, currentCurrency);
                                                    priceModel.Price = _priceFormatter.FormatPrice(finalPrice, true, currentCurrency);
                                                    priceModel.PriceValue = finalPrice;
                                                }
                                                else
                                                {
                                                    priceModel.OldPrice = null;
                                                    priceModel.Price = _priceFormatter.FormatPrice(finalPrice, true, currentCurrency);
                                                    priceModel.PriceValue = finalPrice;
                                                }
                                            }
                                            if (product.ProductType == ProductType.Reservation)
                                            {
                                                //rental product
                                                priceModel.OldPrice = _priceFormatter.FormatReservationProductPeriod(product, priceModel.OldPrice);
                                                priceModel.Price = _priceFormatter.FormatReservationProductPeriod(product, priceModel.Price);
                                            }


                                            //property for German market
                                            //we display tax/shipping info only with "shipping enabled" for this product
                                            //we also ensure this it's not free shipping
                                            priceModel.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoProductBoxes && product.IsShipEnabled && !product.IsFreeShipping;

                                            //PAngV baseprice (used in Germany)
                                            if (product.BasepriceEnabled)
                                                priceModel.BasePricePAngV = product.FormatBasePrice(finalPrice, _localizationService, _measureService, _currencyService, _workContext, _priceFormatter);

                                        }
                                    }
                                }
                                else
                                {
                                    //hide prices
                                    priceModel.OldPrice = null;
                                    priceModel.Price = null;
                                }

                                #endregion
                            }
                            break;
                    }

                    model.ProductPrice = priceModel;

                    #endregion
                }

                //picture
                if (preparePictureModel)
                {
                    #region Prepare product picture
                    //prepare picture model
                    var defaultProductPictureCacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_DEFAULTPICTURE_MODEL_KEY, product.Id, pictureSize, true, currentLanguageId, connectionSecured, currentStoreId);
                    model.DefaultPictureModel = _cacheManager.Get(defaultProductPictureCacheKey, () =>
                    {
                        var picture = product.ProductPictures.OrderBy(x=>x.DisplayOrder).FirstOrDefault();
                        if (picture == null)
                            picture = new ProductPicture();

                        var pictureModel = new PictureModel
                        {
                            ImageUrl = _pictureService.GetPictureUrl(picture.PictureId, pictureSize),
                            FullSizeImageUrl = _pictureService.GetPictureUrl(picture.PictureId)
                        };
                        //"title" attribute
                        pictureModel.Title = (picture != null && !string.IsNullOrEmpty(picture.TitleAttribute)) ?
                            picture.TitleAttribute :
                            string.Format(res["Media.Product.ImageLinkTitleFormat"], model.Name);
                        //"alt" attribute
                        pictureModel.AlternateText = (picture != null && !string.IsNullOrEmpty(picture.AltAttribute)) ?
                            picture.AltAttribute :
                            string.Format(res["Media.Product.ImageAlternateTextFormat"], model.Name);

                        return pictureModel;
                    });

                    #endregion
                }

                //specs
                if (prepareSpecificationAttributes)
                {
                    model.SpecificationAttributeModels = PrepareProductSpecificationModel(product);
                }

                //reviews
                model.ReviewOverviewModel = PrepareProductReviewOverviewModel(product);

                models.Add(model);
            }
            return models;
        }

        public virtual IList<ProductSpecificationModel> PrepareProductSpecificationModel(
            Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            string cacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_SPECS_MODEL_KEY, product.Id, _workContext.WorkingLanguage.Id);
            return _cacheManager.Get(cacheKey, () =>
                product.ProductSpecificationAttributes.Where(x => x.ShowOnProductPage)
                .Select(psa =>
                {
                    var specificationAttribute = _specificationAttributeService.GetSpecificationAttributeById(psa.SpecificationAttributeId);
                    var m = new ProductSpecificationModel
                    {
                        SpecificationAttributeId = psa.SpecificationAttributeId,
                        SpecificationAttributeName = specificationAttribute.GetLocalized(x => x.Name),
                        ColorSquaresRgb = specificationAttribute.SpecificationAttributeOptions.Where(x => x.Id == psa.SpecificationAttributeOptionId).FirstOrDefault() != null ? specificationAttribute.SpecificationAttributeOptions.Where(x => x.Id == psa.SpecificationAttributeOptionId).FirstOrDefault().ColorSquaresRgb : "",
                    };

                    switch (psa.AttributeType)
                    {
                        case SpecificationAttributeType.Option:
                            m.ValueRaw = WebUtility.HtmlEncode(specificationAttribute.SpecificationAttributeOptions.Where(x => x.Id == psa.SpecificationAttributeOptionId).FirstOrDefault().GetLocalized(x => x.Name));
                            break;
                        case SpecificationAttributeType.CustomText:
                            m.ValueRaw = WebUtility.HtmlEncode(psa.CustomValue);
                            break;
                        case SpecificationAttributeType.CustomHtmlText:
                            m.ValueRaw = psa.CustomValue;
                            break;
                        case SpecificationAttributeType.Hyperlink:
                            m.ValueRaw = string.Format("<a href='{0}' target='_blank'>{0}</a>", psa.CustomValue);
                            break;
                        default:
                            break;
                    }
                    return m;
                }).ToList()
            );
        }

        public virtual ProductReviewOverviewModel PrepareProductReviewOverviewModel(
           Product product)
        {
            ProductReviewOverviewModel productReview = null;

            if (_catalogSettings.ShowProductReviewsPerStore)
            {
                string cacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_REVIEWS_MODEL_KEY, product.Id, _storeContext.CurrentStore.Id);

                productReview = _cacheManager.Get(cacheKey, () =>
                {
                    return new ProductReviewOverviewModel
                    {
                        RatingSum = _productService.RatingSumProduct(product.Id, _catalogSettings.ShowProductReviewsPerStore ? _storeContext.CurrentStore.Id : ""),
                        TotalReviews = _productService.TotalReviewsProduct(product.Id, _catalogSettings.ShowProductReviewsPerStore ? _storeContext.CurrentStore.Id : ""),
                    };
                });
            }
            else
            {
                productReview = new ProductReviewOverviewModel()
                {
                    RatingSum = product.ApprovedRatingSum,
                    TotalReviews = product.ApprovedTotalReviews
                };
            }
            if (productReview != null)
            {
                productReview.ProductId = product.Id;
                productReview.AllowCustomerReviews = product.AllowCustomerReviews;
            }
            return productReview;
        }

        public virtual string PrepareProductTemplateViewPath(string productTemplateId)
        {
            if (String.IsNullOrEmpty(productTemplateId))
                throw new ArgumentNullException("product");

            var templateCacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_TEMPLATE_MODEL_KEY, productTemplateId);
            var productTemplateViewPath = _cacheManager.Get(templateCacheKey, () =>
            {
                var template = _productTemplateService.GetProductTemplateById(productTemplateId);
                if (template == null)
                    template = _productTemplateService.GetAllProductTemplates().FirstOrDefault();
                if (template == null)
                    throw new Exception("No default template could be loaded");
                return template.ViewPath;
            });

            return productTemplateViewPath;
        }

        
        public virtual ProductDetailsModel PrepareProductDetailsPage(Product product,
            ShoppingCartItem updatecartitem = null, bool isAssociatedProduct = false)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            #region Standard properties

            var model = new ProductDetailsModel
            {
                Id = product.Id,
                Name = product.GetLocalized(x => x.Name),
                ShortDescription = product.GetLocalized(x => x.ShortDescription),
                FullDescription = product.GetLocalized(x => x.FullDescription),
                Flag = product.Flag,
                MetaKeywords = product.GetLocalized(x => x.MetaKeywords),
                MetaDescription = product.GetLocalized(x => x.MetaDescription),
                MetaTitle = product.GetLocalized(x => x.MetaTitle),
                SeName = product.GetSeName(),
                ShowSku = _catalogSettings.ShowSkuOnProductDetailsPage,
                Sku = product.Sku,
                ShowManufacturerPartNumber = _catalogSettings.ShowManufacturerPartNumber,
                FreeShippingNotificationEnabled = _catalogSettings.ShowFreeShippingNotification,
                ManufacturerPartNumber = product.ManufacturerPartNumber,
                ShowGtin = _catalogSettings.ShowGtin,
                Gtin = product.Gtin,
                StockAvailability = product.FormatStockMessage("", _localizationService, _productAttributeParser, _storeContext),
                HasSampleDownload = product.IsDownload && product.HasSampleDownload,
                DisplayDiscontinuedMessage = 
                    (!product.Published && _catalogSettings.DisplayDiscontinuedMessageForUnpublishedProducts) ||
                    (product.ProductType == ProductType.Auction && product.AuctionEnded) || 
                    (product.AvailableEndDateTimeUtc.HasValue && product.AvailableEndDateTimeUtc.Value < DateTime.UtcNow)

            };

            //automatically generate product description?
            if (_seoSettings.GenerateProductMetaDescription && String.IsNullOrEmpty(model.MetaDescription))
            {
                //based on short description
                model.MetaDescription = model.ShortDescription;
            }

            //shipping info
            model.IsShipEnabled = product.IsShipEnabled;
            if (product.IsShipEnabled)
            {
                model.IsFreeShipping = product.IsFreeShipping;
                //delivery date
                var deliveryDate = _shippingService.GetDeliveryDateById(product.DeliveryDateId);
                if (deliveryDate != null)
                {
                    model.DeliveryDate = deliveryDate.GetLocalized(dd => dd.Name);
                    model.DeliveryColorSquaresRgb = deliveryDate.ColorSquaresRgb;
                }
            }
            //additional shipping charge
            model.AdditionalShippingCharge = product.AdditionalShippingCharge;
            if (model.AdditionalShippingCharge > 0)
                model.AdditionalShippingChargeStr = _priceFormatter.FormatPrice(_taxService.GetShippingPrice(model.AdditionalShippingCharge, _workContext.CurrentCustomer));

            //is product returnable
            model.NotReturnable = product.NotReturnable;
            //email a friend
            model.EmailAFriendEnabled = _catalogSettings.EmailAFriendEnabled;
            //ask question product
            model.AskQuestionEnabled = _catalogSettings.AskQuestionEnabled;
            //ask question us on the product
            model.AskQuestionOnProduct = _catalogSettings.AskQuestionOnProduct;
            if (model.AskQuestionOnProduct)
                model.ProductAskQuestion = PrepareProductAskQuestionSimpleModel(product);

            //compare products
            model.CompareProductsEnabled = _catalogSettings.CompareProductsEnabled;
            //store name
            model.CurrentStoreName = _storeContext.CurrentStore.GetLocalized(x => x.Name);
            //product type
            model.ProductType = product.ProductType;
            #endregion

            #region Vendor details

            //vendor
            if (_vendorSettings.ShowVendorOnProductDetailsPage)
            {
                var vendor = _vendorService.GetVendorById(product.VendorId);
                if (vendor != null && !vendor.Deleted && vendor.Active)
                {
                    model.ShowVendor = true;

                    model.VendorModel = new VendorBriefInfoModel
                    {
                        Id = vendor.Id,
                        Name = vendor.GetLocalized(x => x.Name),
                        SeName = vendor.GetSeName(),
                    };
                }
            }

            #endregion

            #region Page sharing

            if (_catalogSettings.ShowShareButton && !String.IsNullOrEmpty(_catalogSettings.PageShareCode))
            {
                var shareCode = _catalogSettings.PageShareCode;
                if (_webHelper.IsCurrentConnectionSecured())
                {
                    //need to change the addthis link to be https linked when the page is, so that the page doesnt ask about mixed mode when viewed in https...
                    shareCode = shareCode.Replace("http://", "https://");
                }
                model.PageShareCode = shareCode;
            }

            #endregion

            #region Back in stock subscriptions

            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                product.BackorderMode == BackorderMode.NoBackorders &&
                product.AllowBackInStockSubscriptions &&
                product.GetTotalStockQuantity(warehouseId: _storeContext.CurrentStore.DefaultWarehouseId) <= 0)
            {
                //out of stock
                model.DisplayBackInStockSubscription = true;
            }

            #endregion

            #region Breadcrumb

            //do not prepare this model for the associated products. anyway it's not used
            if (_catalogSettings.CategoryBreadcrumbEnabled && !isAssociatedProduct)
            {
                var breadcrumbCacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_BREADCRUMB_MODEL_KEY,
                    product.Id,
                    _workContext.WorkingLanguage.Id,
                    string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                    _storeContext.CurrentStore.Id);
                model.Breadcrumb = _cacheManager.Get(breadcrumbCacheKey, () =>
                {
                    var breadcrumbModel = new ProductDetailsModel.ProductBreadcrumbModel
                    {
                        
                        Enabled = _catalogSettings.CategoryBreadcrumbEnabled,
                        ProductId = product.Id,
                        ProductName = product.GetLocalized(x => x.Name),
                        ProductSeName = product.GetSeName()
                    };
                    var productCategories = product.ProductCategories; 
                    if (productCategories.Any())
                    {
                        var category = _categoryService.GetCategoryById(productCategories.FirstOrDefault().CategoryId);
                        if (category != null)
                        {
                            foreach (var catBr in category.GetCategoryBreadCrumb(_categoryService, _aclService, _storeMappingService))
                            {
                                breadcrumbModel.CategoryBreadcrumb.Add(new CategorySimpleModel
                                {
                                    Id = catBr.Id,
                                    Name = catBr.GetLocalized(x => x.Name),
                                    SeName = catBr.GetSeName(),
                                    IncludeInTopMenu = catBr.IncludeInTopMenu
                                });
                            }
                        }
                    }
                    return breadcrumbModel;
                });
            }

            #endregion

            #region Product tags

            //do not prepare this model for the associated products. anyway it's not used
            if (!isAssociatedProduct)
            {
                var productTagsCacheKey = string.Format(ModelCacheEventConsumer.PRODUCTTAG_BY_PRODUCT_MODEL_KEY, product.Id, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);
                model.ProductTags = _cacheManager.Get(productTagsCacheKey, () =>
                {
                    List<ProductTagModel> tags = new List<ProductTagModel>();
                    foreach (var item in product.ProductTags)
                    {
                        var tag = _productTagService.GetProductTagById(item);
                        if (tag != null)
                        {
                            tags.Add(new ProductTagModel()
                            {
                                Id = tag.Id,
                                Name = tag.GetLocalized(y => y.Name),
                                SeName = tag.GetSeName(),
                                ProductCount = _productTagService.GetProductCount(tag.Id, _storeContext.CurrentStore.Id)
                            });
                        }
                    }
                    return tags;
                });

            }

            #endregion

            #region Pictures

            model.DefaultPictureZoomEnabled = _mediaSettings.DefaultPictureZoomEnabled;
            //default picture
            var defaultPictureSize = isAssociatedProduct ?
                _mediaSettings.AssociatedProductPictureSize :
                _mediaSettings.ProductDetailsPictureSize;
            //prepare picture models
            var productPicturesCacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_DETAILS_PICTURES_MODEL_KEY, product.Id, defaultPictureSize, isAssociatedProduct, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
            var cachedPictures = _cacheManager.Get(productPicturesCacheKey, () =>
            {
                var defaultPicture = product.ProductPictures.OrderBy(x=>x.DisplayOrder).FirstOrDefault();
                if (defaultPicture == null)
                    defaultPicture = new ProductPicture();

                var defaultPictureModel = new PictureModel
                {
                    ImageUrl = _pictureService.GetPictureUrl(defaultPicture.PictureId, defaultPictureSize, !isAssociatedProduct),
                    FullSizeImageUrl = _pictureService.GetPictureUrl(defaultPicture.PictureId, 0, !isAssociatedProduct),
                };
                //"title" attribute
                defaultPictureModel.Title = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.TitleAttribute)) ?
                    defaultPicture.TitleAttribute :
                    string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat.Details"), model.Name);
                //"alt" attribute
                defaultPictureModel.AlternateText = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.AltAttribute)) ?
                    defaultPicture.AltAttribute :
                    string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat.Details"), model.Name);

                //all pictures
                var pictureModels = new List<PictureModel>();
                foreach (var picture in product.ProductPictures.OrderBy(x => x.DisplayOrder))
                {
                    var pictureModel = new PictureModel
                    {
                        ThumbImageUrl = _pictureService.GetPictureUrl(picture.PictureId, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage),
                        ImageUrl = _pictureService.GetPictureUrl(picture.PictureId, _mediaSettings.ProductDetailsPictureSize),
                        FullSizeImageUrl = _pictureService.GetPictureUrl(picture.PictureId),
                        Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat.Details"), model.Name),
                        AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat.Details"), model.Name),
                    };
                    //"title" attribute
                    pictureModel.Title = !string.IsNullOrEmpty(picture.TitleAttribute) ?
                        picture.TitleAttribute :
                        string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat.Details"), model.Name);
                    //"alt" attribute
                    pictureModel.AlternateText = !string.IsNullOrEmpty(picture.AltAttribute) ?
                        picture.AltAttribute :
                        string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat.Details"), model.Name);

                    pictureModels.Add(pictureModel);
                }

                return new { DefaultPictureModel = defaultPictureModel, PictureModels = pictureModels };
            });
            model.DefaultPictureModel = cachedPictures.DefaultPictureModel;
            model.PictureModels = cachedPictures.PictureModels;

            #endregion

            #region Product price

            model.ProductPrice.ProductId = product.Id;
            if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
            {
                model.ProductPrice.HidePrices = false;
                if (product.CustomerEntersPrice)
                {
                    model.ProductPrice.CustomerEntersPrice = true;
                }
                else
                {
                    if (product.CallForPrice)
                    {
                        model.ProductPrice.CallForPrice = true;
                    }
                    else
                    {
                        decimal taxRate;
                        decimal oldPriceBase = _taxService.GetProductPrice(product, product.OldPrice, out taxRate);
                        decimal finalPriceWithoutDiscountBase = _taxService.GetProductPrice(product, _priceCalculationService.GetFinalPrice(product, _workContext.CurrentCustomer, includeDiscounts: false), out taxRate);
                        decimal finalPriceWithDiscountBase = _taxService.GetProductPrice(product, _priceCalculationService.GetFinalPrice(product, _workContext.CurrentCustomer, includeDiscounts: true), out taxRate);

                        decimal oldPrice = _currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, _workContext.WorkingCurrency);
                        decimal finalPriceWithoutDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithoutDiscountBase, _workContext.WorkingCurrency);
                        decimal finalPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase, _workContext.WorkingCurrency);

                        if (finalPriceWithoutDiscountBase != oldPriceBase && oldPriceBase > decimal.Zero)
                            model.ProductPrice.OldPrice = _priceFormatter.FormatPrice(oldPrice);

                        model.ProductPrice.Price = _priceFormatter.FormatPrice(finalPriceWithoutDiscount);

                        if (product.CatalogPrice > 0)
                        {
                            var catalogPrice = _currencyService.ConvertFromPrimaryStoreCurrency(product.CatalogPrice, _workContext.WorkingCurrency);
                            model.ProductPrice.CatalogPrice = _priceFormatter.FormatPrice(catalogPrice);
                        }

                        if (finalPriceWithoutDiscountBase != finalPriceWithDiscountBase)
                            model.ProductPrice.PriceWithDiscount = _priceFormatter.FormatPrice(finalPriceWithDiscount);

                        model.ProductPrice.PriceValue = finalPriceWithDiscount;

                        //property for German market
                        //we display tax/shipping info only with "shipping enabled" for this product
                        //we also ensure this it's not free shipping
                        model.ProductPrice.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoProductDetailsPage
                            && product.IsShipEnabled &&
                            !product.IsFreeShipping;

                        //PAngV baseprice (used in Germany)
                        model.ProductPrice.BasePricePAngV = product.FormatBasePrice(finalPriceWithDiscountBase,
                            _localizationService, _measureService, _currencyService, _workContext, _priceFormatter);

                        //currency code
                        model.ProductPrice.CurrencyCode = _workContext.WorkingCurrency.CurrencyCode;

                        //reservation
                        if (product.ProductType == ProductType.Reservation)
                        {
                            model.ProductPrice.IsReservation = true;
                            var priceStr = _priceFormatter.FormatPrice(finalPriceWithDiscount);
                            model.ProductPrice.ReservationPrice = _priceFormatter.FormatReservationProductPeriod(product, priceStr);
                        }
                        //auction
                        if (product.ProductType == ProductType.Auction)
                        {
                            model.ProductPrice.IsAuction = true;
                            model.ProductPrice.HighestBid = _priceFormatter.FormatPrice(product.HighestBid);
                            model.ProductPrice.HighestBidValue = product.HighestBid;
                            model.ProductPrice.DisableBuyButton = product.DisableBuyButton;
                            model.ProductPrice.StartPriceValue = product.StartPrice;
                            model.ProductPrice.StartPrice = _priceFormatter.FormatPrice(product.StartPrice);
                        }
                    }
                }
            }
            else
            {
                model.ProductPrice.HidePrices = true;
                model.ProductPrice.OldPrice = null;
                model.ProductPrice.CatalogPrice = null;
                model.ProductPrice.Price = null;
            }
            #endregion

            #region 'Add to cart' model

            model.AddToCart.ProductId = product.Id;
            if (updatecartitem != null)
            {
                model.AddToCart.UpdatedShoppingCartItemId = updatecartitem.Id;
                model.AddToCart.UpdateShoppingCartItemType = updatecartitem.ShoppingCartType;
            }

            //quantity
            model.AddToCart.EnteredQuantity = updatecartitem != null ? updatecartitem.Quantity : product.OrderMinimumQuantity;
            model.AddToCart.MeasureUnit = !String.IsNullOrEmpty(product.UnitId) ? _measureService.GetMeasureUnitById(product.UnitId).Name : string.Empty;

            //allowed quantities
            var allowedQuantities = product.ParseAllowedQuantities();
            foreach (var qty in allowedQuantities)
            {
                model.AddToCart.AllowedQuantities.Add(new SelectListItem
                {
                    Text = qty.ToString(),
                    Value = qty.ToString(),
                    Selected = updatecartitem != null && updatecartitem.Quantity == qty
                });
            }
            //minimum quantity notification
            if (product.OrderMinimumQuantity > 1)
            {
                model.AddToCart.MinimumQuantityNotification = string.Format(_localizationService.GetResource("Products.MinimumQuantityNotification"), product.OrderMinimumQuantity);
            }
            //'add to cart', 'add to wishlist' buttons
            model.AddToCart.DisableBuyButton = product.DisableBuyButton || !_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart);
            model.AddToCart.DisableWishlistButton = product.DisableWishlistButton || !_permissionService.Authorize(StandardPermissionProvider.EnableWishlist);
            if (!_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
            {
                model.AddToCart.DisableBuyButton = true;
                model.AddToCart.DisableWishlistButton = true;
            }
            //pre-order
            if (product.AvailableForPreOrder)
            {
                model.AddToCart.AvailableForPreOrder = !product.PreOrderAvailabilityStartDateTimeUtc.HasValue ||
                    product.PreOrderAvailabilityStartDateTimeUtc.Value >= DateTime.UtcNow;
                model.AddToCart.PreOrderAvailabilityStartDateTimeUtc = product.PreOrderAvailabilityStartDateTimeUtc;
            }

            //customer entered price
            model.AddToCart.CustomerEntersPrice = product.CustomerEntersPrice;
            if (model.AddToCart.CustomerEntersPrice)
            {
                decimal minimumCustomerEnteredPrice = _currencyService.ConvertFromPrimaryStoreCurrency(product.MinimumCustomerEnteredPrice, _workContext.WorkingCurrency);
                decimal maximumCustomerEnteredPrice = _currencyService.ConvertFromPrimaryStoreCurrency(product.MaximumCustomerEnteredPrice, _workContext.WorkingCurrency);

                model.AddToCart.CustomerEnteredPrice = updatecartitem != null ? updatecartitem.CustomerEnteredPrice : minimumCustomerEnteredPrice;
                model.AddToCart.CustomerEnteredPriceRange = string.Format(_localizationService.GetResource("Products.EnterProductPrice.Range"),
                    _priceFormatter.FormatPrice(minimumCustomerEnteredPrice, false, false),
                    _priceFormatter.FormatPrice(maximumCustomerEnteredPrice, false, false));
            }


            #endregion

            #region Gift card

            model.GiftCard.IsGiftCard = product.IsGiftCard;
            if (model.GiftCard.IsGiftCard)
            {
                model.GiftCard.GiftCardType = product.GiftCardType;

                if (updatecartitem == null)
                {
                    model.GiftCard.SenderName = _workContext.CurrentCustomer.GetFullName();
                    model.GiftCard.SenderEmail = _workContext.CurrentCustomer.Email;
                }
                else
                {
                    string giftCardRecipientName, giftCardRecipientEmail, giftCardSenderName, giftCardSenderEmail, giftCardMessage;
                    _productAttributeParser.GetGiftCardAttribute(updatecartitem.AttributesXml,
                        out giftCardRecipientName, out giftCardRecipientEmail,
                        out giftCardSenderName, out giftCardSenderEmail, out giftCardMessage);

                    model.GiftCard.RecipientName = giftCardRecipientName;
                    model.GiftCard.RecipientEmail = giftCardRecipientEmail;
                    model.GiftCard.SenderName = giftCardSenderName;
                    model.GiftCard.SenderEmail = giftCardSenderEmail;
                    model.GiftCard.Message = giftCardMessage;
                }
            }

            #endregion

            #region Product attributes

            //performance optimization
            //We cache a value indicating whether a product has attributes
            IList<ProductAttributeMapping> productAttributeMapping = product.ProductAttributeMappings.ToList();

            foreach (var attribute in productAttributeMapping)
            {
                var productAttribute = _productAttributeService.GetProductAttributeById(attribute.ProductAttributeId);
                var attributeModel = new ProductDetailsModel.ProductAttributeModel
                {
                    Id = attribute.Id,
                    ProductId = product.Id,
                    ProductAttributeId = attribute.ProductAttributeId,
                    Name = productAttribute.GetLocalized(x => x.Name),
                    Description = productAttribute.GetLocalized(x => x.Description),
                    TextPrompt = attribute.TextPrompt,
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                    DefaultValue = updatecartitem != null ? null : attribute.DefaultValue,
                    HasCondition = !String.IsNullOrEmpty(attribute.ConditionAttributeXml)
                };
                if (!String.IsNullOrEmpty(attribute.ValidationFileAllowedExtensions))
                {
                    attributeModel.AllowedFileExtensions = attribute.ValidationFileAllowedExtensions
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = attribute.ProductAttributeValues; 
                    foreach (var attributeValue in attributeValues)
                    {
                        var valueModel = new ProductDetailsModel.ProductAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = attributeValue.GetLocalized(x => x.Name),
                            ColorSquaresRgb = attributeValue.ColorSquaresRgb, //used with "Color squares" attribute type
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(valueModel);

                        //display price if allowed
                        if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
                        {
                            decimal taxRate;
                            decimal attributeValuePriceAdjustment = _priceCalculationService.GetProductAttributeValuePriceAdjustment(attributeValue);
                            decimal priceAdjustmentBase = _taxService.GetProductPrice(product, attributeValuePriceAdjustment, out taxRate);
                            decimal priceAdjustment = _currencyService.ConvertFromPrimaryStoreCurrency(priceAdjustmentBase, _workContext.WorkingCurrency);
                            if (priceAdjustmentBase > decimal.Zero)
                                valueModel.PriceAdjustment = "+" + _priceFormatter.FormatPrice(priceAdjustment, false, false);
                            else if (priceAdjustmentBase < decimal.Zero)
                                valueModel.PriceAdjustment = "-" + _priceFormatter.FormatPrice(-priceAdjustment, false, false);

                            valueModel.PriceAdjustmentValue = priceAdjustment;
                        }

                        //"image square" picture (with with "image squares" attribute type only)
                        if (!String.IsNullOrEmpty(attributeValue.ImageSquaresPictureId))
                        {
                            var productAttributeImageSquarePictureCacheKey = string.Format(ModelCacheEventConsumer.PRODUCTATTRIBUTE_IMAGESQUARE_PICTURE_MODEL_KEY,
                            attributeValue.ImageSquaresPictureId,
                            _webHelper.IsCurrentConnectionSecured(),
                            _storeContext.CurrentStore.Id);
                            valueModel.ImageSquaresPictureModel = _cacheManager.Get(productAttributeImageSquarePictureCacheKey, () =>
                            {
                                var imageSquaresPicture = _pictureService.GetPictureById(attributeValue.ImageSquaresPictureId);
                                if (imageSquaresPicture != null)
                                {
                                    return new PictureModel
                                    {
                                        FullSizeImageUrl = _pictureService.GetPictureUrl(imageSquaresPicture),
                                        ImageUrl = _pictureService.GetPictureUrl(imageSquaresPicture, _mediaSettings.ImageSquarePictureSize)
                                    };
                                }
                                return new PictureModel();
                            });
                        }

                        //picture of a product attribute value
                        if (!String.IsNullOrEmpty(attributeValue.PictureId))
                        {
                            var productAttributePictureCacheKey = string.Format(ModelCacheEventConsumer.PRODUCTATTRIBUTE_PICTURE_MODEL_KEY,
                                attributeValue.PictureId,
                                _webHelper.IsCurrentConnectionSecured(),
                                _storeContext.CurrentStore.Id);
                            valueModel.PictureModel = _cacheManager.Get(productAttributePictureCacheKey, () =>
                            {
                                var valuePicture = _pictureService.GetPictureById(attributeValue.PictureId);
                                if (valuePicture != null)
                                {
                                    return new PictureModel
                                    {
                                        FullSizeImageUrl = _pictureService.GetPictureUrl(valuePicture),
                                        ImageUrl = _pictureService.GetPictureUrl(valuePicture, defaultPictureSize)
                                    };
                                }
                                return new PictureModel();
                            });
                        }
                    }
                }

                //set already selected attributes (if we're going to update the existing shopping cart item)
                if (updatecartitem != null)
                {
                    switch (attribute.AttributeControlType)
                    {
                        case AttributeControlType.DropdownList:
                        case AttributeControlType.RadioList:
                        case AttributeControlType.Checkboxes:
                        case AttributeControlType.ColorSquares:
                        case AttributeControlType.ImageSquares:
                            {
                                if (!String.IsNullOrEmpty(updatecartitem.AttributesXml))
                                {
                                    //clear default selection
                                    foreach (var item in attributeModel.Values)
                                        item.IsPreSelected = false;

                                    //select new values
                                    var selectedValues = _productAttributeParser.ParseProductAttributeValues(product, updatecartitem.AttributesXml);
                                    foreach (var attributeValue in selectedValues)
                                        foreach (var item in attributeModel.Values)
                                            if (attributeValue.Id == item.Id)
                                                item.IsPreSelected = true;

                                }
                            }
                            break;
                        case AttributeControlType.ReadonlyCheckboxes:
                            {
                                //do nothing
                                //values are already pre-set
                            }
                            break;
                        case AttributeControlType.TextBox:
                        case AttributeControlType.MultilineTextbox:
                            {
                                if (!String.IsNullOrEmpty(updatecartitem.AttributesXml))
                                {
                                    var enteredText = _productAttributeParser.ParseValues(updatecartitem.AttributesXml, attribute.Id);
                                    if (enteredText.Any())
                                        attributeModel.DefaultValue = enteredText[0];
                                }
                            }
                            break;
                        case AttributeControlType.Datepicker:
                            {
                                //keep in mind my that the code below works only in the current culture
                                var selectedDateStr = _productAttributeParser.ParseValues(updatecartitem.AttributesXml, attribute.Id);
                                if (selectedDateStr.Any())
                                {
                                    DateTime selectedDate;
                                    if (DateTime.TryParseExact(selectedDateStr[0], "D", CultureInfo.CurrentCulture,
                                                           DateTimeStyles.None, out selectedDate))
                                    {
                                        //successfully parsed
                                        attributeModel.SelectedDay = selectedDate.Day;
                                        attributeModel.SelectedMonth = selectedDate.Month;
                                        attributeModel.SelectedYear = selectedDate.Year;
                                    }
                                }

                            }
                            break;
                        case AttributeControlType.FileUpload:
                            {
                                if (!String.IsNullOrEmpty(updatecartitem.AttributesXml))
                                {
                                    var downloadGuidStr = _productAttributeParser.ParseValues(updatecartitem.AttributesXml, attribute.Id).FirstOrDefault();
                                    Guid downloadGuid;
                                    Guid.TryParse(downloadGuidStr, out downloadGuid);
                                    var download = _downloadService.GetDownloadByGuid(downloadGuid);
                                    if (download != null)
                                        attributeModel.DefaultValue = download.DownloadGuid.ToString();
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }

                model.ProductAttributes.Add(attributeModel);
            }

            #endregion 

            #region Product specifications

            model.ProductSpecifications = PrepareProductSpecificationModel(product);

            #endregion

            #region Product review overview

            model.ProductReviewOverview = PrepareProductReviewOverviewModel(product);

            #endregion

            #region Tier prices

            if (product.HasTierPrices && _permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
            {
                model.TierPrices = product.TierPrices.OrderBy(x => x.Quantity)
                                    .FilterByStore(_storeContext.CurrentStore.Id)
                                    .FilterForCustomer(_workContext.CurrentCustomer)
                                    .FilterByDate()
                                    .RemoveDuplicatedQuantities().Select(tierPrice =>
                                    {
                                        decimal taxRate;
                                        var priceBase = _taxService.GetProductPrice(product, _priceCalculationService.GetFinalPrice(product,
                                            _workContext.CurrentCustomer, decimal.Zero, _catalogSettings.DisplayTierPricesWithDiscounts, tierPrice.Quantity), out taxRate);
                                        var price = _currencyService.ConvertFromPrimaryStoreCurrency(priceBase, _workContext.WorkingCurrency);

                                        return new ProductDetailsModel.TierPriceModel
                                        {
                                            Quantity = tierPrice.Quantity,
                                            Price = _priceFormatter.FormatPrice(price, false, false)
                                        };
                                    }).ToList();
            }

            #endregion

            #region Manufacturers

            string manufacturersCacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_MANUFACTURERS_MODEL_KEY,
                product.Id,
                _workContext.WorkingLanguage.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            model.ProductManufacturers = _cacheManager.Get(manufacturersCacheKey, () =>
                product.ProductManufacturers.Select(x => _manufacturerService.GetManufacturerById(x.ManufacturerId).ToModel()).ToList()
                );

            #endregion

            #region Associated products

            if (product.ProductType == ProductType.GroupedProduct)
            {
                //ensure no circular references
                if (!isAssociatedProduct)
                {
                    var associatedProducts = _productService.GetAssociatedProducts(product.Id, _storeContext.CurrentStore.Id);
                    foreach (var associatedProduct in associatedProducts)
                        model.AssociatedProducts.Add(PrepareProductDetailsPage(associatedProduct, null, true));
                }
            }

            #endregion

            #region Product reservations
            if (product.ProductType == ProductType.Reservation)
            {
                model.AddToCart.IsReservation = true;
                var reservations = _productReservationService.GetProductReservationsByProductId(product.Id, true, null).ToList();
                var inCart = _workContext.CurrentCustomer.ShoppingCartItems.Where(x => !string.IsNullOrEmpty(x.ReservationId)).ToList();
                foreach (var cartItem in inCart)
                {
                    var matching = reservations.Where(x => x.Id == cartItem.ReservationId);
                    if (matching.Any())
                    {
                        reservations.Remove(matching.First());
                    }
                }

                if (reservations.Any())
                {
                    var first = reservations.Where(x => x.Date >= DateTime.UtcNow).OrderBy(x => x.Date).FirstOrDefault();
                    if (first != null)
                    {
                        model.StartDate = first.Date;
                    }
                    else
                    {
                        model.StartDate = DateTime.UtcNow;
                    }
                }

                model.IntervalUnit = product.IntervalUnitType;
                model.Interval = product.Interval;
                model.IncBothDate = product.IncBothDate;

                var list = reservations.GroupBy(x => x.Parameter).ToList().Select(x => x.Key);
                foreach (var item in list)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        model.Parameters.Add(new SelectListItem { Text = item, Value = item });
                    }
                }

                if (model.Parameters.Any())
                {
                    model.Parameters.Insert(0, new SelectListItem { Text = "", Value = "" });
                }
            }

            #endregion Product reservations

            #region Product Bundle

            if(product.ProductType == ProductType.BundledProduct)
            {
                foreach (var bundle in product.BundleProducts.OrderBy(x=>x.DisplayOrder))
                {
                    var p1 = _productService.GetProductById(bundle.ProductId);
                    if(p1!=null && p1.Published && _aclService.Authorize(p1) && _storeMappingService.Authorize(p1) && p1.IsAvailable())
                    {

                        var bundleProduct = new ProductDetailsModel.ProductBundleModel()
                        {
                            ProductId = p1.Id,
                            Name = p1.GetLocalized(x => x.Name),
                            ShortDescription = p1.GetLocalized(x => x.ShortDescription),
                            SeName = p1.GetSeName(),
                            Sku = p1.Sku,
                            Mpn = p1.ManufacturerPartNumber,
                            Gtin = p1.Gtin,
                            Quantity = bundle.Quantity
                        };
                        if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
                        {
                            decimal taxRateBundle;
                            decimal finalPriceWithDiscountBase = _taxService.GetProductPrice(p1, _priceCalculationService.GetFinalPrice(p1, _workContext.CurrentCustomer, includeDiscounts: true), out taxRateBundle);
                            decimal finalPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase, _workContext.WorkingCurrency);
                            bundleProduct.Price = _priceFormatter.FormatPrice(finalPriceWithDiscount);
                            bundleProduct.PriceValue = finalPriceWithDiscount;
                        }

                        //prepare picture model
                        var productbundlePicturesCacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_DETAILS_PICTURES_MODEL_KEY, 
                            p1.Id, _mediaSettings.ProductBundlePictureSize, false, _workContext.WorkingLanguage.Id, 
                            _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);

                        bundleProduct.DefaultPictureModel = _cacheManager.Get(productbundlePicturesCacheKey, () =>
                        {
                            var picture = p1.ProductPictures.OrderBy(x => x.DisplayOrder).FirstOrDefault();
                            if (picture == null)
                                picture = new ProductPicture();

                            var pictureModel = new PictureModel
                            {
                                ImageUrl = _pictureService.GetPictureUrl(picture.PictureId, _mediaSettings.ProductBundlePictureSize),
                                FullSizeImageUrl = _pictureService.GetPictureUrl(picture.PictureId)
                            };
                            //"title" attribute
                            pictureModel.Title = (picture != null && !string.IsNullOrEmpty(picture.TitleAttribute)) ?
                                picture.TitleAttribute :
                                string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat.Details"), p1.Name);
                            //"alt" attribute
                            pictureModel.AlternateText = (picture != null && !string.IsNullOrEmpty(picture.AltAttribute)) ?
                                picture.AltAttribute :
                                string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat.Details"), p1.Name);

                            return pictureModel;
                        });

                        model.ProductBundleModels.Add(bundleProduct);
                    }

                }

            }
            #endregion

            #region Auctions

            model.StartPrice = product.StartPrice;
            model.HighestBidValue = product.HighestBid;
            model.AddToCart.IsAuction = true;
            model.EndTime = product.AvailableEndDateTimeUtc;
            model.AuctionEnded = product.AuctionEnded;

            #endregion

            return model;
        }

        public virtual void PrepareProductReviewsModel(ProductReviewsModel model, Product product, int size = 0)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (model == null)
                throw new ArgumentNullException("model");

            model.ProductId = product.Id;
            model.ProductName = product.GetLocalized(x => x.Name);
            model.ProductSeName = product.GetSeName();
            var customerService = EngineContext.Current.Resolve<ICustomerService>();
            var productReviews = _productService.GetAllProductReviews("", true, null, null, "", _catalogSettings.ShowProductReviewsPerStore ? _storeContext.CurrentStore.Id : "", product.Id, size);
            foreach (var pr in productReviews)
            {
                var customer = customerService.GetCustomerById(pr.CustomerId);
                model.Items.Add(new ProductReviewModel
                {
                    Id = pr.Id,
                    CustomerId = pr?.CustomerId,
                    CustomerName = customer?.FormatUserName(),
                    AllowViewingProfiles = _customerSettings.AllowViewingProfiles && customer != null && !customer.IsGuest(),
                    Title = pr.Title,
                    ReviewText = pr.ReviewText,
                    ReplyText = pr.ReplyText,
                    Signature = pr.Signature,
                    Rating = pr.Rating,
                    Helpfulness = new ProductReviewHelpfulnessModel
                    {
                        ProductId = product.Id,
                        ProductReviewId = pr.Id,
                        HelpfulYesTotal = pr.HelpfulYesTotal,
                        HelpfulNoTotal = pr.HelpfulNoTotal,
                    },
                    WrittenOnStr = _dateTimeHelper.ConvertToUserTime(pr.CreatedOnUtc, DateTimeKind.Utc).ToString("g"),
                });
            }

            model.AddProductReview.CanCurrentCustomerLeaveReview = _catalogSettings.AllowAnonymousUsersToReviewProduct || !_workContext.CurrentCustomer.IsGuest();
            model.AddProductReview.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnProductReviewPage;
        }

        public virtual ProductReview InsertProductReview(Product product, ProductReviewsModel model)
        {
            //save review
            int rating = model.AddProductReview.Rating;
            if (rating < 1 || rating > 5)
                rating = _catalogSettings.DefaultProductRatingValue;
            bool isApproved = !_catalogSettings.ProductReviewsMustBeApproved;

            var productReview = new ProductReview
            {
                ProductId = product.Id,
                StoreId = _storeContext.CurrentStore.Id,
                CustomerId = _workContext.CurrentCustomer.Id,
                Title = model.AddProductReview.Title,
                ReviewText = model.AddProductReview.ReviewText,
                Rating = rating,
                HelpfulYesTotal = 0,
                HelpfulNoTotal = 0,
                IsApproved = isApproved,
                CreatedOnUtc = DateTime.UtcNow,
            };
            _productService.InsertProductReview(productReview);

            if (!_workContext.CurrentCustomer.HasContributions)
            {
                EngineContext.Current.Resolve<ICustomerService>().UpdateContributions(_workContext.CurrentCustomer);
            }

            //update product totals
            _productService.UpdateProductReviewTotals(product);

            //notify store owner
            if (_catalogSettings.NotifyStoreOwnerAboutNewProductReviews)
                _workflowMessageService.SendProductReviewNotificationMessage(productReview, _localizationSettings.DefaultAdminLanguageId);


            return productReview;
        }

        public virtual void SendProductEmailAFriendMessage(Product product, ProductEmailAFriendModel model)
        {
            _workflowMessageService.SendProductEmailAFriendMessage(_workContext.CurrentCustomer,
                    _workContext.WorkingLanguage.Id, product,
                    model.YourEmailAddress, model.FriendEmail,
                    Core.Html.HtmlHelper.FormatText(model.PersonalMessage, false, true, false, false, false, false));
        }

        public virtual void SendProductAskQuestionMessage(Product product, ProductAskQuestionModel model)
        {
            _workflowMessageService.SendProductQuestionMessage( _workContext.CurrentCustomer,
                    _workContext.WorkingLanguage.Id, product, model.Email, model.FullName, model.Phone,
                    Core.Html.HtmlHelper.FormatText(model.Message, false, true, false, false, false, false));

        }

        public virtual ProductDetailsAttributeChangeModel PrepareProductDetailsAttributeChangeModel
            (Product product, bool validateAttributeConditions, bool loadPicture, IFormCollection form)
        {
            var model = new ProductDetailsAttributeChangeModel();

            var shoppingCartViewModelService = EngineContext.Current.Resolve<IShoppingCartViewModelService>();

            string attributeXml = shoppingCartViewModelService.ParseProductAttributes(product, form);

            //rental attributes
            DateTime? rentalStartDate = null;
            DateTime? rentalEndDate = null;
            if (product.ProductType == ProductType.Reservation)
            {
                shoppingCartViewModelService.ParseReservationDates(product, form, out rentalStartDate, out rentalEndDate);
            }

            model.Sku = product.FormatSku(attributeXml, _productAttributeParser);
            model.Mpn = product.FormatMpn(attributeXml, _productAttributeParser);
            model.Gtin = product.FormatGtin(attributeXml, _productAttributeParser);

            if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices) && !product.CustomerEntersPrice && product.ProductType != ProductType.Auction)
            {
                //we do not calculate price of "customer enters price" option is enabled
                decimal finalPrice = _priceCalculationService.GetUnitPrice(product,
                    _workContext.CurrentCustomer,
                    ShoppingCartType.ShoppingCart,
                    1, attributeXml, 0,
                    rentalStartDate, rentalEndDate,
                    true, out decimal discountAmount, out List<AppliedDiscount> scDiscounts);
                decimal finalPriceWithDiscountBase = _taxService.GetProductPrice(product, finalPrice, out decimal taxRate);
                decimal finalPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase, _workContext.WorkingCurrency);
                model.Price = _priceFormatter.FormatPrice(finalPriceWithDiscount);
            }
            //stock
            model.StockAvailability = product.FormatStockMessage(attributeXml, _localizationService, _productAttributeParser, _storeContext);

            //conditional attributes
            if (validateAttributeConditions)
            {
                var attributes = product.ProductAttributeMappings;
                foreach (var attribute in attributes)
                {
                    var conditionMet = _productAttributeParser.IsConditionMet(product, attribute, attributeXml);
                    if (conditionMet.HasValue)
                    {
                        if (conditionMet.Value)
                            model.EnabledAttributeMappingIds.Add(attribute.Id);
                        else
                            model.DisabledAttributeMappingids.Add(attribute.Id);
                    }
                }
            }
            //picture. used when we want to override a default product picture when some attribute is selected
            if (loadPicture)
            {

                //first, try to get product attribute combination picture
                var pictureId = product.ProductAttributeCombinations.Where(x => x.AttributesXml == attributeXml).FirstOrDefault()?.PictureId ?? "";
                //then, let's see whether we have attribute values with pictures
                if (string.IsNullOrEmpty(pictureId))
                {
                    pictureId = _productAttributeParser.ParseProductAttributeValues(product, attributeXml)
                        .FirstOrDefault(attributeValue => !string.IsNullOrEmpty(attributeValue.PictureId))?.PictureId ?? "";
                }

                if (!string.IsNullOrEmpty(pictureId))
                {
                    var productAttributePictureCacheKey = string.Format(ModelCacheEventConsumer.PRODUCTATTRIBUTE_PICTURE_MODEL_KEY,
                        pictureId, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
                    var pictureModel = _cacheManager.Get(productAttributePictureCacheKey, () =>
                    {
                        var picture = _pictureService.GetPictureById(pictureId);
                        return picture == null ? new PictureModel() : new PictureModel
                        {
                            FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                            ImageUrl = _pictureService.GetPictureUrl(picture, _mediaSettings.ProductDetailsPictureSize)
                        };
                    });
                    model.PictureFullSizeUrl = pictureModel.FullSizeImageUrl;
                    model.PictureDefaultSizeUrl = pictureModel.ImageUrl;
                }

            }

            return model;
        }


        public virtual ProductAskQuestionModel PrepareProductAskQuestionModel(Product product)
        {
            var customer = _workContext.CurrentCustomer;

            var model = new ProductAskQuestionModel();
            model.Id = product.Id;
            model.ProductName = product.GetLocalized(x => x.Name);
            model.ProductSeName = product.GetSeName();
            model.Email = customer.Email;
            model.FullName = customer.GetFullName();
            model.Phone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
            model.Message = "";
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnAskQuestionPage;

            return model;
        }

        public virtual ProductAskQuestionSimpleModel PrepareProductAskQuestionSimpleModel(Product product)
        {
            var customer = _workContext.CurrentCustomer;

            var model = new ProductAskQuestionSimpleModel();
            model.Id = product.Id;
            model.AskQuestionEmail = customer.Email;
            model.AskQuestionFullName = customer.GetFullName();
            model.AskQuestionPhone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone);
            model.AskQuestionMessage = "";
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnAskQuestionPage;
            return model;
        }
    }
}