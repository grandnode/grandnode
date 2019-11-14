using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Media;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Seo;
using Grand.Core.Domain.Tax;
using Grand.Core.Domain.Vendors;
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
using Grand.Services.Orders;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Shipping;
using Grand.Services.Stores;
using Grand.Services.Tax;
using Grand.Services.Vendors;
using Grand.Web.Extensions;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Interfaces;
using Grand.Web.Models.Catalog;
using Grand.Web.Models.Media;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Grand.Web.Services
{
    public partial class ProductViewModelService : IProductViewModelService
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
        private readonly IServiceProvider _serviceProvider;
        private readonly MediaSettings _mediaSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly SeoSettings _seoSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;

        public ProductViewModelService(IPermissionService permissionService, IWorkContext workContext, IStoreContext storeContext,
            ILocalizationService localizationService, IProductService productService, IPriceCalculationService priceCalculationService,
            ITaxService taxService, ICurrencyService currencyService, IPriceFormatter priceFormatter, IMeasureService measureService,
            ICacheManager cacheManager, IPictureService pictureService, ISpecificationAttributeService specificationAttributeService, IWebHelper webHelper,
            IProductTemplateService productTemplateService, IProductAttributeParser productAttributeParser, IShippingService shippingService,
            IVendorService vendorService, ICategoryService categoryService, IAclService aclService, IStoreMappingService storeMappingService,
            IProductTagService productTagService, IProductAttributeService productAttributeService, IManufacturerService manufacturerService,
            IDateTimeHelper dateTimeHelper, IDownloadService downloadService, IWorkflowMessageService workflowMessageService, IProductReservationService productReservationService,
            IServiceProvider serviceProvider,
            MediaSettings mediaSettings, CatalogSettings catalogSettings, SeoSettings seoSettings, VendorSettings vendorSettings, CustomerSettings customerSettings,
            CaptchaSettings captchaSettings, LocalizationSettings localizationSettings, ShoppingCartSettings shoppingCartSettings)
        {
            _permissionService = permissionService;
            _workContext = workContext;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _productService = productService;
            _priceCalculationService = priceCalculationService;
            _taxService = taxService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _measureService = measureService;
            _cacheManager = cacheManager;
            _pictureService = pictureService;
            _specificationAttributeService = specificationAttributeService;
            _webHelper = webHelper;
            _productTemplateService = productTemplateService;
            _productAttributeParser = productAttributeParser;
            _shippingService = shippingService;
            _vendorService = vendorService;
            _categoryService = categoryService;
            _aclService = aclService;
            _storeMappingService = storeMappingService;
            _productTagService = productTagService;
            _productAttributeService = productAttributeService;
            _manufacturerService = manufacturerService;
            _dateTimeHelper = dateTimeHelper;
            _downloadService = downloadService;
            _workflowMessageService = workflowMessageService;
            _productReservationService = productReservationService;
            _serviceProvider = serviceProvider;
            _mediaSettings = mediaSettings;
            _catalogSettings = catalogSettings;
            _seoSettings = seoSettings;
            _vendorSettings = vendorSettings;
            _customerSettings = customerSettings;
            _captchaSettings = captchaSettings;
            _localizationSettings = localizationSettings;
            _shoppingCartSettings = shoppingCartSettings;
        }

        public virtual async Task<IEnumerable<ProductOverviewModel>> PrepareProductOverviewModels(
            IEnumerable<Product> products,
            bool preparePriceModel = true, bool preparePictureModel = true,
            int? productThumbPictureSize = null, bool prepareSpecificationAttributes = false,
            bool forceRedirectionAfterAddingToCart = false)
        {
            if (products == null)
                throw new ArgumentNullException("products");

            var currentCustomer = _workContext.CurrentCustomer;
            var displayPricesTask = _permissionService.Authorize(StandardPermissionProvider.DisplayPrices, currentCustomer);
            var enableShoppingCartTask = _permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart, currentCustomer);
            var enableWishlistTask = _permissionService.Authorize(StandardPermissionProvider.EnableWishlist, currentCustomer);
            var currentCurrency = _workContext.WorkingCurrency;
            var currentStoreId = _storeContext.CurrentStore.Id;
            var currentLanguage = _workContext.WorkingLanguage;
            int pictureSize = productThumbPictureSize.HasValue ? productThumbPictureSize.Value : _mediaSettings.ProductThumbPictureSize;
            var connectionSecured = _webHelper.IsCurrentConnectionSecured();
            var showSku = _catalogSettings.ShowSkuOnCatalogPages;
            var taxDisplay = _workContext.TaxDisplayType;
            bool priceIncludesTax = taxDisplay == TaxDisplayType.IncludingTax;
            var showQty = _catalogSettings.DisplayQuantityOnCatalogPages;

            var res = new Dictionary<string, string>
            {
                { "Products.CallForPrice", _localizationService.GetResource("Products.CallForPrice", currentLanguage.Id) },
                { "Products.PriceRangeFrom", _localizationService.GetResource("Products.PriceRangeFrom", currentLanguage.Id)},
                { "Media.Product.ImageLinkTitleFormat", _localizationService.GetResource("Media.Product.ImageLinkTitleFormat", currentLanguage.Id) },
                { "Media.Product.ImageAlternateTextFormat", _localizationService.GetResource("Media.Product.ImageAlternateTextFormat", currentLanguage.Id) }
            };

            await Task.WhenAll(new Task[] { displayPricesTask, enableShoppingCartTask, enableWishlistTask });

            var displayPrices = displayPricesTask.Result;
            var enableShoppingCart = enableShoppingCartTask.Result;
            var enableWishlist = enableWishlistTask.Result;

            var models = new List<ProductOverviewModel>();

            var lvl1BackgroundTasks = new List<Task>();

            foreach (var product in products)
            {
                var model = new ProductOverviewModel {
                    Id = product.Id,
                    Name = product.GetLocalized(x => x.Name, currentLanguage.Id),
                    ShortDescription = product.GetLocalized(x => x.ShortDescription, currentLanguage.Id),
                    FullDescription = product.GetLocalized(x => x.FullDescription, currentLanguage.Id),
                    SeName = product.GetSeName(currentLanguage.Id),
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
                    GenericAttributes = product.GenericAttributes,
                    MarkAsNew = product.MarkAsNew &&
                        (!product.MarkAsNewStartDateTimeUtc.HasValue || product.MarkAsNewStartDateTimeUtc.Value < DateTime.UtcNow) &&
                        (!product.MarkAsNewEndDateTimeUtc.HasValue || product.MarkAsNewEndDateTimeUtc.Value > DateTime.UtcNow)
                };

                //price
                if (preparePriceModel)
                {
                    #region Prepare product price

                    var priceModel = new ProductOverviewModel.ProductPriceModel {
                        ForceRedirectionAfterAddingToCart = forceRedirectionAfterAddingToCart
                    };

                    switch (product.ProductType)
                    {
                        case ProductType.GroupedProduct:
                            {
                                #region Grouped product

                                var associatedProducts = await _productService.GetAssociatedProducts(product.Id, currentStoreId);

                                //add to cart button (ignore "DisableBuyButton" property for grouped products)
                                priceModel.DisableBuyButton = !enableShoppingCart || !displayPrices;

                                //add to wishlist button (ignore "DisableWishlistButton" property for grouped products)
                                priceModel.DisableWishlistButton = !enableWishlist || !displayPrices;

                                //compare products
                                priceModel.DisableAddToCompareListButton = !_catalogSettings.CompareProductsEnabled;

                                //catalog price, not used in views, but it's for front developer
                                if (product.CatalogPrice > 0)
                                {
                                    decimal catalogPrice = await _currencyService.ConvertFromPrimaryStoreCurrency(product.CatalogPrice, currentCurrency);
                                    priceModel.CatalogPrice = _priceFormatter.FormatPrice(catalogPrice, true, currentCurrency, currentLanguage, priceIncludesTax);
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
                                                    var tmpPrice = (await _priceCalculationService.GetFinalPrice(associatedProduct,
                                                        currentCustomer, decimal.Zero, true, int.MaxValue)).finalPrice;
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
                                                        decimal finalPriceBase = (await _taxService.GetProductPrice(minPriceProduct, minPossiblePrice.Value, priceIncludesTax, currentCustomer)).productprice;
                                                        decimal finalPrice = await _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceBase, currentCurrency);

                                                        priceModel.OldPrice = null;
                                                        priceModel.Price = String.Format(res["Products.PriceRangeFrom"], _priceFormatter.FormatPrice(finalPrice, true, currentCurrency, currentLanguage, priceIncludesTax));
                                                        priceModel.PriceValue = finalPrice;

                                                        //PAngV baseprice (used in Germany)
                                                        if (product.BasepriceEnabled)
                                                            priceModel.BasePricePAngV = await product.FormatBasePrice(finalPrice, _localizationService, _measureService, _currencyService, _workContext, _priceFormatter);
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
                                    decimal catalogPrice = await _currencyService.ConvertFromPrimaryStoreCurrency(product.CatalogPrice, currentCurrency);
                                    priceModel.CatalogPrice = _priceFormatter.FormatPrice(catalogPrice, true, currentCurrency, currentLanguage, priceIncludesTax);
                                }

                                //start price for product auction
                                if (product.StartPrice > 0)
                                {
                                    decimal startPrice = await _currencyService.ConvertFromPrimaryStoreCurrency(product.StartPrice, currentCurrency);
                                    priceModel.StartPrice = _priceFormatter.FormatPrice(startPrice, true, currentCurrency, currentLanguage, priceIncludesTax);
                                    priceModel.StartPriceValue = startPrice;
                                }

                                //highest bid for product auction
                                if (product.HighestBid > 0)
                                {
                                    decimal highestBid = await _currencyService.ConvertFromPrimaryStoreCurrency(product.HighestBid, currentCurrency);
                                    priceModel.HighestBid = _priceFormatter.FormatPrice(highestBid, true, currentCurrency, currentLanguage, priceIncludesTax);
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
                                            var infoprice = (await _priceCalculationService.GetFinalPrice(product,
                                                currentCustomer, decimal.Zero, true, int.MaxValue));

                                            priceModel.AppliedDiscounts = infoprice.appliedDiscounts;
                                            priceModel.PreferredTierPrice = infoprice.preferredTierPrice;

                                            decimal minPossiblePrice = infoprice.finalPrice;

                                            decimal oldPriceBase = (await _taxService.GetProductPrice(product, product.OldPrice, priceIncludesTax, currentCustomer)).productprice;
                                            decimal finalPriceBase = (await _taxService.GetProductPrice(product, minPossiblePrice, priceIncludesTax, currentCustomer)).productprice;

                                            decimal oldPrice = await _currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, currentCurrency);
                                            decimal finalPrice = await _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceBase, currentCurrency);

                                            //do we have tier prices configured?
                                            var tierPrices = new List<TierPrice>();
                                            if (product.TierPrices.Any())
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
                                                priceModel.Price = String.Format(res["Products.PriceRangeFrom"], _priceFormatter.FormatPrice(finalPrice, true, currentCurrency, currentLanguage, priceIncludesTax));
                                                priceModel.PriceValue = finalPrice;
                                            }
                                            else
                                            {
                                                if (finalPriceBase != oldPriceBase && oldPriceBase != decimal.Zero)
                                                {
                                                    priceModel.OldPrice = _priceFormatter.FormatPrice(oldPrice, true, currentCurrency, currentLanguage, priceIncludesTax);
                                                    priceModel.OldPriceValue = oldPrice;
                                                    priceModel.Price = _priceFormatter.FormatPrice(finalPrice, true, currentCurrency, currentLanguage, priceIncludesTax);
                                                    priceModel.PriceValue = finalPrice;
                                                }
                                                else
                                                {
                                                    priceModel.OldPrice = null;
                                                    priceModel.Price = _priceFormatter.FormatPrice(finalPrice, true, currentCurrency, currentLanguage, priceIncludesTax);
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
                                                priceModel.BasePricePAngV = await product.FormatBasePrice(finalPrice, _localizationService, _measureService, _currencyService, _workContext, _priceFormatter);

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
                    var defaultProductPictureCacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_DEFAULTPICTURE_MODEL_KEY, product.Id, pictureSize, true, currentLanguage, connectionSecured, currentStoreId);

                    lvl1BackgroundTasks.Add(_cacheManager.GetAsync(defaultProductPictureCacheKey, async () =>
                    {
                        var picture = product.ProductPictures.OrderBy(x => x.DisplayOrder).FirstOrDefault();
                        if (picture == null)
                            picture = new ProductPicture();

                        var pictureModel = new PictureModel {
                            Id = picture.PictureId
                        };

                        await _pictureService.GetPictureUrl(picture.PictureId, pictureSize).ContinueWith(t => pictureModel.ImageUrl = t.Result);
                        await _pictureService.GetPictureUrl(picture.PictureId).ContinueWith(t => pictureModel.FullSizeImageUrl = t.Result);

                        //"title" attribute
                        pictureModel.Title = (picture != null && !string.IsNullOrEmpty(picture.TitleAttribute)) ?
                            picture.TitleAttribute :
                            string.Format(res["Media.Product.ImageLinkTitleFormat"], model.Name);
                        //"alt" attribute
                        pictureModel.AlternateText = (picture != null && !string.IsNullOrEmpty(picture.AltAttribute)) ?
                            picture.AltAttribute :
                            string.Format(res["Media.Product.ImageAlternateTextFormat"], model.Name);

                        return pictureModel;
                    }).ContinueWith(t => model.DefaultPictureModel = t.Result));

                    //prepare second picture model
                    if (_catalogSettings.SecondPictureOnCatalogPages)
                    {
                        var secondProductPictureCacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_SECOND_DEFAULTPICTURE_MODEL_KEY, product.Id, pictureSize, true, currentLanguage, connectionSecured, currentStoreId);

                        lvl1BackgroundTasks.Add(_cacheManager.GetAsync(secondProductPictureCacheKey, async () =>
                        {
                            var picture = product.ProductPictures.OrderBy(x => x.DisplayOrder).Skip(1).Take(1).FirstOrDefault();
                            if (picture == null)
                                return new PictureModel();

                            var pictureModel = new PictureModel {
                                Id = picture.PictureId
                            };

                            await _pictureService.GetPictureUrl(picture.PictureId, pictureSize).ContinueWith(t => pictureModel.ImageUrl = t.Result);
                            await _pictureService.GetPictureUrl(picture.PictureId).ContinueWith(t => pictureModel.FullSizeImageUrl = t.Result);

                            //"title" attribute
                            pictureModel.Title = (picture != null && !string.IsNullOrEmpty(picture.TitleAttribute)) ?
                                    picture.TitleAttribute :
                                    string.Format(res["Media.Product.ImageLinkTitleFormat"], model.Name);
                            //"alt" attribute
                            pictureModel.AlternateText = (picture != null && !string.IsNullOrEmpty(picture.AltAttribute)) ?
                                    picture.AltAttribute :
                                    string.Format(res["Media.Product.ImageAlternateTextFormat"], model.Name);

                            return pictureModel;
                        }).ContinueWith(t => model.SecondPictureModel = t.Result));
                    }
                    #endregion
                }

                //specs
                if (prepareSpecificationAttributes)
                {
                    lvl1BackgroundTasks.Add(PrepareProductSpecificationModel(product).ContinueWith(t => model.SpecificationAttributeModels = t.Result));
                }

                //reviews
                lvl1BackgroundTasks.Add(PrepareProductReviewOverviewModel(product).ContinueWith(t => model.ReviewOverviewModel = t.Result));

                models.Add(model);
            }

            await Task.WhenAll(lvl1BackgroundTasks);

            return models;
        }

        public virtual async Task<IList<ProductSpecificationModel>> PrepareProductSpecificationModel(
            Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            string cacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_SPECS_MODEL_KEY, product.Id, _workContext.WorkingLanguage.Id);
            return await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var spa = new List<ProductSpecificationModel>();
                foreach (var item in product.ProductSpecificationAttributes.Where(x => x.ShowOnProductPage).OrderBy(x => x.DisplayOrder))
                {
                    var specificationAttribute = await _specificationAttributeService.GetSpecificationAttributeById(item.SpecificationAttributeId);
                    var m = new ProductSpecificationModel {
                        SpecificationAttributeId = item.SpecificationAttributeId,
                        SpecificationAttributeName = specificationAttribute.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                        ColorSquaresRgb = specificationAttribute.SpecificationAttributeOptions.Where(x => x.Id == item.SpecificationAttributeOptionId).FirstOrDefault() != null ? specificationAttribute.SpecificationAttributeOptions.Where(x => x.Id == item.SpecificationAttributeOptionId).FirstOrDefault().ColorSquaresRgb : "",
                    };

                    switch (item.AttributeType)
                    {
                        case SpecificationAttributeType.Option:
                            m.ValueRaw = WebUtility.HtmlEncode(specificationAttribute.SpecificationAttributeOptions.Where(x => x.Id == item.SpecificationAttributeOptionId).FirstOrDefault().GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id));
                            break;
                        case SpecificationAttributeType.CustomText:
                            m.ValueRaw = WebUtility.HtmlEncode(item.CustomValue);
                            break;
                        case SpecificationAttributeType.CustomHtmlText:
                            m.ValueRaw = item.CustomValue;
                            break;
                        case SpecificationAttributeType.Hyperlink:
                            m.ValueRaw = string.Format("<a href='{0}' target='_blank'>{0}</a>", item.CustomValue);
                            break;
                        default:
                            break;
                    }
                    spa.Add(m);

                }
                return spa;
            }

            );
        }

        public virtual async Task<ProductReviewOverviewModel> PrepareProductReviewOverviewModel(
           Product product)
        {
            ProductReviewOverviewModel productReview = null;

            if (_catalogSettings.ShowProductReviewsPerStore)
            {
                string cacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_REVIEWS_MODEL_KEY, product.Id, _storeContext.CurrentStore.Id);

                productReview = await _cacheManager.GetAsync(cacheKey, async () =>
                {
                    return new ProductReviewOverviewModel {
                        RatingSum = await _productService.RatingSumProduct(product.Id, _catalogSettings.ShowProductReviewsPerStore ? _storeContext.CurrentStore.Id : ""),
                        TotalReviews = await _productService.TotalReviewsProduct(product.Id, _catalogSettings.ShowProductReviewsPerStore ? _storeContext.CurrentStore.Id : ""),
                    };
                });
            }
            else
            {
                productReview = new ProductReviewOverviewModel() {
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

        public virtual async Task<string> PrepareProductTemplateViewPath(string productTemplateId)
        {
            if (String.IsNullOrEmpty(productTemplateId))
                throw new ArgumentNullException("product");

            var templateCacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_TEMPLATE_MODEL_KEY, productTemplateId);
            var productTemplateViewPath = await _cacheManager.GetAsync(templateCacheKey, async () =>
            {
                var template = await _productTemplateService.GetProductTemplateById(productTemplateId);
                if (template == null)
                    template = (await _productTemplateService.GetAllProductTemplates()).FirstOrDefault();
                if (template == null)
                    throw new Exception("No default template could be loaded");
                return template.ViewPath;
            });

            return productTemplateViewPath;
        }


        public virtual async Task<ProductDetailsModel> PrepareProductDetailsPage(Product product,
            ShoppingCartItem updatecartitem = null, bool isAssociatedProduct = false)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            #region Standard properties

            var model = new ProductDetailsModel {
                Id = product.Id,
                Name = product.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                ShortDescription = product.GetLocalized(x => x.ShortDescription, _workContext.WorkingLanguage.Id),
                FullDescription = product.GetLocalized(x => x.FullDescription, _workContext.WorkingLanguage.Id),
                Flag = product.Flag,
                MetaKeywords = product.GetLocalized(x => x.MetaKeywords, _workContext.WorkingLanguage.Id),
                MetaDescription = product.GetLocalized(x => x.MetaDescription, _workContext.WorkingLanguage.Id),
                MetaTitle = product.GetLocalized(x => x.MetaTitle, _workContext.WorkingLanguage.Id),
                SeName = product.GetSeName(_workContext.WorkingLanguage.Id),
                ShowSku = _catalogSettings.ShowSkuOnProductDetailsPage,
                Sku = product.Sku,
                ShowManufacturerPartNumber = _catalogSettings.ShowManufacturerPartNumber,
                FreeShippingNotificationEnabled = _catalogSettings.ShowFreeShippingNotification,
                ManufacturerPartNumber = product.ManufacturerPartNumber,
                ShowGtin = _catalogSettings.ShowGtin,
                Gtin = product.Gtin,
                StockAvailability = product.FormatStockMessage("", updatecartitem != null ? updatecartitem.WarehouseId : _storeContext.CurrentStore.DefaultWarehouseId, _localizationService, _productAttributeParser),
                GenericAttributes = product.GenericAttributes,
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

            //warehouse
            model.AllowToSelectWarehouse = _shoppingCartSettings.AllowToSelectWarehouse;
            if (model.AllowToSelectWarehouse)
            {
                foreach (var warehouse in await _shippingService.GetAllWarehouses())
                {
                    var productwarehouse = product.ProductWarehouseInventory.FirstOrDefault(x => x.WarehouseId == warehouse.Id);
                    model.ProductWarehouses.Add(new ProductDetailsModel.ProductWarehouseModel {
                        Use = productwarehouse != null,
                        StockQuantity = productwarehouse?.StockQuantity ?? 0,
                        ReservedQuantity = productwarehouse?.ReservedQuantity ?? 0,
                        WarehouseId = warehouse.Id,
                        Name = warehouse.Name,
                        Selected = updatecartitem != null && updatecartitem?.WarehouseId == warehouse.Id
                    });
                }
            }
            //shipping info
            model.IsShipEnabled = product.IsShipEnabled;
            if (product.IsShipEnabled)
            {
                model.IsFreeShipping = product.IsFreeShipping;
                //delivery date
                var deliveryDate = await _shippingService.GetDeliveryDateById(product.DeliveryDateId);
                if (deliveryDate != null)
                {
                    model.DeliveryDate = deliveryDate.GetLocalized(dd => dd.Name, _workContext.WorkingLanguage.Id);
                    model.DeliveryColorSquaresRgb = deliveryDate.ColorSquaresRgb;
                }
            }
            //additional shipping charge
            model.AdditionalShippingCharge = product.AdditionalShippingCharge;
            if (model.AdditionalShippingCharge > 0)
                model.AdditionalShippingChargeStr = _priceFormatter.FormatPrice((await _taxService.GetShippingPrice(model.AdditionalShippingCharge, _workContext.CurrentCustomer)).shippingPrice);

            //is product returnable
            model.NotReturnable = product.NotReturnable;
            //email a friend
            model.EmailAFriendEnabled = _catalogSettings.EmailAFriendEnabled;
            //ask question product
            model.AskQuestionEnabled = _catalogSettings.AskQuestionEnabled;
            //ask question us on the product
            model.AskQuestionOnProduct = _catalogSettings.AskQuestionOnProduct;
            if (model.AskQuestionOnProduct)
                model.ProductAskQuestion = await PrepareProductAskQuestionSimpleModel(product);

            //compare products
            model.CompareProductsEnabled = _catalogSettings.CompareProductsEnabled;
            //store name
            model.CurrentStoreName = _storeContext.CurrentStore.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id);
            //product type
            model.ProductType = product.ProductType;
            #endregion

            #region Vendor details

            //vendor
            if (_vendorSettings.ShowVendorOnProductDetailsPage)
            {
                var vendor = await _vendorService.GetVendorById(product.VendorId);
                if (vendor != null && !vendor.Deleted && vendor.Active)
                {
                    model.ShowVendor = true;

                    model.VendorModel = new VendorBriefInfoModel {
                        Id = vendor.Id,
                        Name = vendor.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                        SeName = vendor.GetSeName(_workContext.WorkingLanguage.Id),
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

            if ((product.ManageInventoryMethod == ManageInventoryMethod.ManageStock || product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes) &&
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
                model.Breadcrumb = await _cacheManager.GetAsync(breadcrumbCacheKey, async () =>
                {
                    var breadcrumbModel = new ProductDetailsModel.ProductBreadcrumbModel {

                        Enabled = _catalogSettings.CategoryBreadcrumbEnabled,
                        ProductId = product.Id,
                        ProductName = product.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                        ProductSeName = product.GetSeName(_workContext.WorkingLanguage.Id)
                    };
                    var productCategories = product.ProductCategories;
                    if (productCategories.Any())
                    {
                        var category = await _categoryService.GetCategoryById(productCategories.FirstOrDefault().CategoryId);
                        if (category != null)
                        {
                            foreach (var catBr in await category.GetCategoryBreadCrumb(_categoryService, _aclService, _storeMappingService))
                            {
                                breadcrumbModel.CategoryBreadcrumb.Add(new CategorySimpleModel {
                                    Id = catBr.Id,
                                    Name = catBr.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                                    SeName = catBr.GetSeName(_workContext.WorkingLanguage.Id),
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
                model.ProductTags = await _cacheManager.GetAsync(productTagsCacheKey, async () =>
                {
                    List<ProductTagModel> tags = new List<ProductTagModel>();
                    foreach (var item in product.ProductTags)
                    {
                        var tag = await _productTagService.GetProductTagByName(item);
                        if (tag != null)
                        {
                            tags.Add(new ProductTagModel() {
                                Id = tag.Id,
                                Name = tag.GetLocalized(y => y.Name, _workContext.WorkingLanguage.Id),
                                SeName = tag.SeName,
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
            var cachedPictures = await _cacheManager.GetAsync(productPicturesCacheKey, async () =>
            {
                var defaultPicture = product.ProductPictures.OrderBy(x => x.DisplayOrder).FirstOrDefault();
                if (defaultPicture == null)
                    defaultPicture = new ProductPicture();

                var defaultPictureModel = new PictureModel {
                    Id = defaultPicture.PictureId,
                    ImageUrl = await _pictureService.GetPictureUrl(defaultPicture.PictureId, defaultPictureSize, !isAssociatedProduct),
                    FullSizeImageUrl = await _pictureService.GetPictureUrl(defaultPicture.PictureId, 0, !isAssociatedProduct),
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
                    var pictureModel = new PictureModel {
                        Id = picture.PictureId,
                        ThumbImageUrl = await _pictureService.GetPictureUrl(picture.PictureId, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage),
                        ImageUrl = await _pictureService.GetPictureUrl(picture.PictureId, _mediaSettings.ProductDetailsPictureSize),
                        FullSizeImageUrl = await _pictureService.GetPictureUrl(picture.PictureId),
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
            var displayPrices = await _permissionService.Authorize(StandardPermissionProvider.DisplayPrices);

            model.ProductPrice.ProductId = product.Id;
            if (displayPrices)
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
                        var productprice = await _taxService.GetProductPrice(product, product.OldPrice);
                        decimal taxRate = productprice.taxRate;
                        decimal oldPriceBase = productprice.productprice;
                        decimal finalPriceWithoutDiscountBase = (await (_taxService.GetProductPrice(product, (await _priceCalculationService.GetFinalPrice(product, _workContext.CurrentCustomer, includeDiscounts: false)).finalPrice))).productprice;

                        var appliedPrice = (await _priceCalculationService.GetFinalPrice(product, _workContext.CurrentCustomer, includeDiscounts: true));
                        decimal finalPriceWithDiscountBase = (await _taxService.GetProductPrice(product, appliedPrice.finalPrice)).productprice;

                        decimal oldPrice = await _currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, _workContext.WorkingCurrency);
                        decimal finalPriceWithoutDiscount = await _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithoutDiscountBase, _workContext.WorkingCurrency);
                        decimal finalPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase, _workContext.WorkingCurrency);

                        if (finalPriceWithoutDiscountBase != oldPriceBase && oldPriceBase > decimal.Zero)
                            model.ProductPrice.OldPrice = _priceFormatter.FormatPrice(oldPrice);

                        model.ProductPrice.Price = _priceFormatter.FormatPrice(finalPriceWithoutDiscount);
                        if (appliedPrice.appliedDiscounts.Any())
                            model.ProductPrice.AppliedDiscounts = appliedPrice.appliedDiscounts;
                        if (appliedPrice.preferredTierPrice != null)
                            model.ProductPrice.PreferredTierPrice = appliedPrice.preferredTierPrice;

                        if (product.CatalogPrice > 0)
                        {
                            var catalogPrice = await _currencyService.ConvertFromPrimaryStoreCurrency(product.CatalogPrice, _workContext.WorkingCurrency);
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
                        model.ProductPrice.BasePricePAngV = await product.FormatBasePrice(finalPriceWithDiscountBase,
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
                            decimal highestBid = await _currencyService.ConvertFromPrimaryStoreCurrency(product.HighestBid, _workContext.WorkingCurrency);
                            model.ProductPrice.HighestBid = _priceFormatter.FormatPrice(highestBid);
                            model.ProductPrice.HighestBidValue = highestBid;
                            model.ProductPrice.DisableBuyButton = product.DisableBuyButton;
                            decimal startPrice = await _currencyService.ConvertFromPrimaryStoreCurrency(product.StartPrice, _workContext.WorkingCurrency);
                            model.ProductPrice.StartPrice = _priceFormatter.FormatPrice(startPrice);
                            model.ProductPrice.StartPriceValue = startPrice;
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
            model.AddToCart.MeasureUnit = !String.IsNullOrEmpty(product.UnitId) ? (await _measureService.GetMeasureUnitById(product.UnitId)).Name : string.Empty;

            //allowed quantities
            var allowedQuantities = product.ParseAllowedQuantities();
            foreach (var qty in allowedQuantities)
            {
                model.AddToCart.AllowedQuantities.Add(new SelectListItem {
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
            model.AddToCart.DisableBuyButton = product.DisableBuyButton || !await _permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart);
            model.AddToCart.DisableWishlistButton = product.DisableWishlistButton || !await _permissionService.Authorize(StandardPermissionProvider.EnableWishlist);
            if (!await _permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
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
                decimal minimumCustomerEnteredPrice = await _currencyService.ConvertFromPrimaryStoreCurrency(product.MinimumCustomerEnteredPrice, _workContext.WorkingCurrency);
                decimal maximumCustomerEnteredPrice = await _currencyService.ConvertFromPrimaryStoreCurrency(product.MaximumCustomerEnteredPrice, _workContext.WorkingCurrency);

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

            foreach (var attribute in productAttributeMapping.OrderBy(x => x.DisplayOrder))
            {
                var productAttribute = await _productAttributeService.GetProductAttributeById(attribute.ProductAttributeId);
                var attributeModel = new ProductDetailsModel.ProductAttributeModel {
                    Id = attribute.Id,
                    ProductId = product.Id,
                    ProductAttributeId = attribute.ProductAttributeId,
                    Name = productAttribute.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                    Description = productAttribute.GetLocalized(x => x.Description, _workContext.WorkingLanguage.Id),
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
                        var valueModel = new ProductDetailsModel.ProductAttributeValueModel {
                            Id = attributeValue.Id,
                            Name = attributeValue.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                            ColorSquaresRgb = attributeValue.ColorSquaresRgb, //used with "Color squares" attribute type
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(valueModel);

                        //display price if allowed
                        if (displayPrices)
                        {
                            decimal attributeValuePriceAdjustment = await _priceCalculationService.GetProductAttributeValuePriceAdjustment(attributeValue);
                            var productprice = await _taxService.GetProductPrice(product, attributeValuePriceAdjustment);
                            decimal priceAdjustmentBase = productprice.productprice;
                            decimal taxRate = productprice.taxRate;
                            decimal priceAdjustment = await _currencyService.ConvertFromPrimaryStoreCurrency(priceAdjustmentBase, _workContext.WorkingCurrency);
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
                            valueModel.ImageSquaresPictureModel = await _cacheManager.GetAsync(productAttributeImageSquarePictureCacheKey, async () =>
                            {
                                var imageSquaresPicture = await _pictureService.GetPictureById(attributeValue.ImageSquaresPictureId);
                                if (imageSquaresPicture != null)
                                {
                                    return new PictureModel {
                                        Id = imageSquaresPicture?.Id,
                                        FullSizeImageUrl = await _pictureService.GetPictureUrl(imageSquaresPicture),
                                        ImageUrl = await _pictureService.GetPictureUrl(imageSquaresPicture, _mediaSettings.ImageSquarePictureSize)
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
                            valueModel.PictureModel = await _cacheManager.GetAsync(productAttributePictureCacheKey, async () =>
                            {
                                var valuePicture = await _pictureService.GetPictureById(attributeValue.PictureId);
                                if (valuePicture != null)
                                {
                                    return new PictureModel {
                                        Id = attributeValue.PictureId,
                                        FullSizeImageUrl = await _pictureService.GetPictureUrl(valuePicture),
                                        ImageUrl = await _pictureService.GetPictureUrl(valuePicture, defaultPictureSize)
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
                                    var download = await _downloadService.GetDownloadByGuid(downloadGuid);
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

            model.ProductSpecifications = await PrepareProductSpecificationModel(product);

            #endregion

            #region Product review overview

            model.ProductReviewOverview = await PrepareProductReviewOverviewModel(product);

            #endregion

            #region Tier prices
            if (product.TierPrices.Any() && await _permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
            {

                foreach (var tierPrice in product.TierPrices.OrderBy(x => x.Quantity)
                                    .FilterByStore(_storeContext.CurrentStore.Id)
                                    .FilterForCustomer(_workContext.CurrentCustomer)
                                    .FilterByDate()
                                    .RemoveDuplicatedQuantities())
                {
                    var tier = new ProductDetailsModel.TierPriceModel();
                    var priceBase = await _taxService.GetProductPrice(product, (await _priceCalculationService.GetFinalPrice(product,
                                           _workContext.CurrentCustomer, decimal.Zero, _catalogSettings.DisplayTierPricesWithDiscounts, tierPrice.Quantity)).finalPrice);
                    var price = await _currencyService.ConvertFromPrimaryStoreCurrency(priceBase.productprice, _workContext.WorkingCurrency);
                    tier.Quantity = tierPrice.Quantity;
                    tier.Price = _priceFormatter.FormatPrice(price, false, false);
                    model.TierPrices.Add(tier);
                }
            }

            #endregion

            #region Manufacturers

            string manufacturersCacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_MANUFACTURERS_MODEL_KEY,
                product.Id,
                _workContext.WorkingLanguage.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            model.ProductManufacturers = await _cacheManager.GetAsync(manufacturersCacheKey, async () =>
            {
                var listManuf = new List<ManufacturerModel>();
                foreach (var item in product.ProductManufacturers)
                {
                    var manuf = (await _manufacturerService.GetManufacturerById(item.ManufacturerId)).ToModel(_workContext.WorkingLanguage);
                    listManuf.Add(manuf);
                }
                return listManuf;
            });

            #endregion

            #region Associated products

            if (product.ProductType == ProductType.GroupedProduct)
            {
                //ensure no circular references
                if (!isAssociatedProduct)
                {
                    var associatedProducts = await _productService.GetAssociatedProducts(product.Id, _storeContext.CurrentStore.Id);
                    foreach (var associatedProduct in associatedProducts)
                        model.AssociatedProducts.Add(await PrepareProductDetailsPage(associatedProduct, null, true));
                }
            }

            #endregion

            #region Product reservations
            if (product.ProductType == ProductType.Reservation)
            {
                model.AddToCart.IsReservation = true;
                var reservations = await _productReservationService.GetProductReservationsByProductId(product.Id, true, null);
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

            if (product.ProductType == ProductType.BundledProduct)
            {
                foreach (var bundle in product.BundleProducts.OrderBy(x => x.DisplayOrder))
                {
                    var p1 = await _productService.GetProductById(bundle.ProductId);
                    if (p1 != null && p1.Published && _aclService.Authorize(p1) && _storeMappingService.Authorize(p1) && p1.IsAvailable())
                    {

                        var bundleProduct = new ProductDetailsModel.ProductBundleModel() {
                            ProductId = p1.Id,
                            Name = p1.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                            ShortDescription = p1.GetLocalized(x => x.ShortDescription, _workContext.WorkingLanguage.Id),
                            SeName = p1.GetSeName(_workContext.WorkingLanguage.Id),
                            Sku = p1.Sku,
                            Mpn = p1.ManufacturerPartNumber,
                            Gtin = p1.Gtin,
                            Quantity = bundle.Quantity
                        };
                        if (displayPrices)
                        {
                            var productprice = await _taxService.GetProductPrice(p1, (await _priceCalculationService.GetFinalPrice(p1, _workContext.CurrentCustomer, includeDiscounts: true)).finalPrice);
                            decimal taxRateBundle = productprice.taxRate;
                            decimal finalPriceWithDiscountBase = productprice.productprice;
                            decimal finalPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase, _workContext.WorkingCurrency);
                            bundleProduct.Price = _priceFormatter.FormatPrice(finalPriceWithDiscount);
                            bundleProduct.PriceValue = finalPriceWithDiscount;
                        }

                        //prepare picture model
                        var productbundlePicturesCacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_DETAILS_PICTURES_MODEL_KEY,
                            p1.Id, _mediaSettings.ProductBundlePictureSize, false, _workContext.WorkingLanguage.Id,
                            _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);

                        bundleProduct.DefaultPictureModel = await _cacheManager.GetAsync(productbundlePicturesCacheKey, async () =>
                        {
                            var picture = p1.ProductPictures.OrderBy(x => x.DisplayOrder).FirstOrDefault();
                            if (picture == null)
                                picture = new ProductPicture();

                            var pictureModel = new PictureModel {
                                Id = picture.PictureId,
                                ImageUrl = await _pictureService.GetPictureUrl(picture.PictureId, _mediaSettings.ProductBundlePictureSize),
                                FullSizeImageUrl = await _pictureService.GetPictureUrl(picture.PictureId)
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
            model.AddToCart.IsAuction = product.ProductType == ProductType.Auction;
            model.EndTime = product.AvailableEndDateTimeUtc;
            model.EndTimeLocalTime = product.AvailableEndDateTimeUtc.HasValue ? _dateTimeHelper.ConvertToUserTime(product.AvailableEndDateTimeUtc.Value, DateTimeKind.Utc) : new DateTime?();

            model.AuctionEnded = product.AuctionEnded;

            #endregion

            return model;
        }

        public virtual async Task PrepareProductReviewsModel(ProductReviewsModel model, Product product, int size = 0)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (model == null)
                throw new ArgumentNullException("model");

            model.ProductId = product.Id;
            model.ProductName = product.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id);
            model.ProductSeName = product.GetSeName(_workContext.WorkingLanguage.Id);
            var customerService = _serviceProvider.GetRequiredService<ICustomerService>();
            var productReviews = await _productService.GetAllProductReviews("", true, null, null, "", _catalogSettings.ShowProductReviewsPerStore ? _storeContext.CurrentStore.Id : "", product.Id, size);
            foreach (var pr in productReviews)
            {
                var customer = await customerService.GetCustomerById(pr.CustomerId);
                model.Items.Add(new ProductReviewModel {
                    Id = pr.Id,
                    CustomerId = pr?.CustomerId,
                    CustomerName = customer?.FormatUserName(_customerSettings.CustomerNameFormat),
                    AllowViewingProfiles = _customerSettings.AllowViewingProfiles && customer != null && !customer.IsGuest(),
                    Title = pr.Title,
                    ReviewText = pr.ReviewText,
                    ReplyText = pr.ReplyText,
                    Signature = pr.Signature,
                    Rating = pr.Rating,
                    Helpfulness = new ProductReviewHelpfulnessModel {
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

        public virtual async Task<ProductReview> InsertProductReview(Product product, ProductReviewsModel model)
        {
            //save review
            int rating = model.AddProductReview.Rating;
            if (rating < 1 || rating > 5)
                rating = _catalogSettings.DefaultProductRatingValue;
            bool isApproved = !_catalogSettings.ProductReviewsMustBeApproved;

            var productReview = new ProductReview {
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
            await _productService.InsertProductReview(productReview);

            if (!_workContext.CurrentCustomer.HasContributions)
            {
                await _serviceProvider.GetRequiredService<ICustomerService>().UpdateContributions(_workContext.CurrentCustomer);
            }

            //update product totals
            await _productService.UpdateProductReviewTotals(product);

            //notify store owner
            if (_catalogSettings.NotifyStoreOwnerAboutNewProductReviews)
                await _workflowMessageService.SendProductReviewNotificationMessage(product, productReview, _localizationSettings.DefaultAdminLanguageId);

            return productReview;
        }

        public virtual async Task SendProductEmailAFriendMessage(Product product, ProductEmailAFriendModel model)
        {
            await _workflowMessageService.SendProductEmailAFriendMessage(_workContext.CurrentCustomer,
                    _workContext.WorkingLanguage.Id, product,
                    model.YourEmailAddress, model.FriendEmail,
                    Core.Html.HtmlHelper.FormatText(model.PersonalMessage, false, true, false, false, false, false));
        }

        public virtual async Task SendProductAskQuestionMessage(Product product, ProductAskQuestionModel model)
        {
            await _workflowMessageService.SendProductQuestionMessage(_workContext.CurrentCustomer,
                    _workContext.WorkingLanguage.Id, product, model.Email, model.FullName, model.Phone,
                    Core.Html.HtmlHelper.FormatText(model.Message, false, true, false, false, false, false));

        }

        public virtual async Task<ProductDetailsAttributeChangeModel> PrepareProductDetailsAttributeChangeModel
            (Product product, bool validateAttributeConditions, bool loadPicture, IFormCollection form)
        {
            var model = new ProductDetailsAttributeChangeModel();

            var shoppingCartViewModelService = _serviceProvider.GetRequiredService<IShoppingCartViewModelService>();

            string attributeXml = await shoppingCartViewModelService.ParseProductAttributes(product, form);

            string warehouseId = _shoppingCartSettings.AllowToSelectWarehouse ? form["WarehouseId"].ToString() : _storeContext.CurrentStore.DefaultWarehouseId;

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

            if (await _permissionService.Authorize(StandardPermissionProvider.DisplayPrices) && !product.CustomerEntersPrice && product.ProductType != ProductType.Auction)
            {
                //we do not calculate price of "customer enters price" option is enabled
                var unitprice = await _priceCalculationService.GetUnitPrice(product,
                    _workContext.CurrentCustomer,
                    ShoppingCartType.ShoppingCart,
                    1, attributeXml, 0,
                    rentalStartDate, rentalEndDate,
                    true);

                decimal discountAmount = unitprice.discountAmount;
                List<AppliedDiscount> scDiscounts = unitprice.appliedDiscounts;
                decimal finalPrice = unitprice.unitprice;
                var productprice = await _taxService.GetProductPrice(product, finalPrice);
                decimal finalPriceWithDiscountBase = productprice.productprice;
                decimal taxRate = productprice.taxRate;
                decimal finalPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase, _workContext.WorkingCurrency);
                model.Price = _priceFormatter.FormatPrice(finalPriceWithDiscount);
            }
            //stock
            model.StockAvailability = product.FormatStockMessage(warehouseId, attributeXml, _localizationService, _productAttributeParser);

            //back in stock subscription
            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes &&
                product.BackorderMode == BackorderMode.NoBackorders &&
                product.AllowBackInStockSubscriptions)
            {
                var combination = _productAttributeParser.FindProductAttributeCombination(product, attributeXml);

                if (combination != null)
                    if (product.GetTotalStockQuantityForCombination(combination, warehouseId: _storeContext.CurrentStore.DefaultWarehouseId) <= 0)
                        model.DisplayBackInStockSubscription = true;

                var backInStockSubscriptionService = _serviceProvider.GetRequiredService<IBackInStockSubscriptionService>();
                var subscription = await backInStockSubscriptionService
                   .FindSubscription(_workContext.CurrentCustomer.Id,
                    product.Id, attributeXml, _storeContext.CurrentStore.Id, product.UseMultipleWarehouses ? _storeContext.CurrentStore.DefaultWarehouseId : "");

                if (subscription != null)
                    model.ButtonTextBackInStockSubscription = _localizationService.GetResource("BackInStockSubscriptions.DeleteNotifyWhenAvailable");
                else
                    model.ButtonTextBackInStockSubscription = _localizationService.GetResource("BackInStockSubscriptions.NotifyMeWhenAvailable");

            }

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
                    var pictureModel = await _cacheManager.GetAsync(productAttributePictureCacheKey, async () =>
                    {
                        var picture = await _pictureService.GetPictureById(pictureId);
                        return picture == null ? new PictureModel() : new PictureModel {
                            Id = pictureId,
                            FullSizeImageUrl = await _pictureService.GetPictureUrl(picture),
                            ImageUrl = await _pictureService.GetPictureUrl(picture, _mediaSettings.ProductDetailsPictureSize)
                        };
                    });
                    model.PictureFullSizeUrl = pictureModel.FullSizeImageUrl;
                    model.PictureDefaultSizeUrl = pictureModel.ImageUrl;
                }

            }

            return model;
        }


        public virtual async Task<ProductAskQuestionModel> PrepareProductAskQuestionModel(Product product)
        {
            var customer = _workContext.CurrentCustomer;

            var model = new ProductAskQuestionModel();
            model.Id = product.Id;
            model.ProductName = product.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id);
            model.ProductSeName = product.GetSeName(_workContext.WorkingLanguage.Id);
            model.Email = customer.Email;
            model.FullName = customer.GetFullName();
            model.Phone = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Phone);
            model.Message = "";
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnAskQuestionPage;

            return await Task.FromResult(model);
        }

        public virtual async Task<ProductAskQuestionSimpleModel> PrepareProductAskQuestionSimpleModel(Product product)
        {
            var customer = _workContext.CurrentCustomer;

            var model = new ProductAskQuestionSimpleModel();
            model.Id = product.Id;
            model.AskQuestionEmail = customer.Email;
            model.AskQuestionFullName = customer.GetFullName();
            model.AskQuestionPhone = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Phone);
            model.AskQuestionMessage = "";
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnAskQuestionPage;
            return await Task.FromResult(model);
        }
        public virtual async Task<IList<ProductOverviewModel>> PrepareNewProductsDisplayedOnHomePage(int? productThumbPictureSize)
        {
            if (!_catalogSettings.NewProductsOnHomePage)
                return new List<ProductOverviewModel>();

            var products = (await _productService.SearchProducts(
                storeId: _storeContext.CurrentStore.Id,
                visibleIndividuallyOnly: true,
                markedAsNewOnly: true,
                orderBy: ProductSortingEnum.CreatedOn,
                pageSize: _catalogSettings.NewProductsNumberOnHomePage)).products;

            if (!products.Any())
                return new List<ProductOverviewModel>();

            return (await PrepareProductOverviewModels(products, true, true, productThumbPictureSize, prepareSpecificationAttributes: _catalogSettings.ShowSpecAttributeOnCatalogPages)).ToList();
        }
        public virtual async Task<IList<ProductOverviewModel>> PrepareProductsDisplayedOnHomePage(int? productThumbPictureSize)
        {
            var products = await _productService.GetAllProductsDisplayedOnHomePage();

            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
            //availability dates
            products = products.Where(p => p.IsAvailable()).ToList();

            if (!products.Any())
                return new List<ProductOverviewModel>();

            return (await PrepareProductOverviewModels(products, true, true, productThumbPictureSize, prepareSpecificationAttributes: _catalogSettings.ShowSpecAttributeOnCatalogPages)).ToList();
        }
        public virtual async Task<IList<ProductOverviewModel>> PrepareProductsHomePageBestSellers(int? productThumbPictureSize)
        {
            //load and cache report
            var orderReportService = _serviceProvider.GetRequiredService<IOrderReportService>();
            var fromdate = DateTime.UtcNow.AddMonths(_catalogSettings.PeriodBestsellers > 0 ? -_catalogSettings.PeriodBestsellers : -12);
            var report = await _cacheManager.GetAsync(string.Format(ModelCacheEventConsumer.HOMEPAGE_BESTSELLERS_IDS_KEY, _storeContext.CurrentStore.Id),
                async () => await orderReportService.BestSellersReport(
                        storeId: _storeContext.CurrentStore.Id,
                        createdFromUtc: fromdate,
                        ps: Core.Domain.Payments.PaymentStatus.Paid,
                        pageSize: _catalogSettings.NumberOfBestsellersOnHomepage)
                        );

            //load products
            var products = await _productService.GetProductsByIds(report.Select(x => x.ProductId).ToArray());
            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
            //availability dates
            products = products.Where(p => p.IsAvailable()).ToList();

            if (!products.Any())
                return new List<ProductOverviewModel>();

            return (await PrepareProductOverviewModels(products, true, true, productThumbPictureSize, prepareSpecificationAttributes: _catalogSettings.ShowSpecAttributeOnCatalogPages)).ToList();
        }
        public virtual async Task<IList<ProductOverviewModel>> PrepareProductsRecommended(int? productThumbPictureSize)
        {
            var products = await _productService.GetRecommendedProducts(_workContext.CurrentCustomer.GetCustomerRoleIds());

            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();

            //availability dates
            products = products.Where(p => p.IsAvailable()).ToList();

            if (!products.Any())
                return new List<ProductOverviewModel>();

            //prepare model
            return (await PrepareProductOverviewModels(products, true, true, productThumbPictureSize, prepareSpecificationAttributes: _catalogSettings.ShowSpecAttributeOnCatalogPages)).ToList();
        }
        public virtual async Task<IList<ProductOverviewModel>> PrepareProductsPersonalized(int? productThumbPictureSize)
        {
            var customer = _workContext.CurrentCustomer;
            var products = await _productService.GetPersonalizedProducts(customer.Id);

            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p, customer) && _storeMappingService.Authorize(p)).ToList();

            //availability dates
            products = products.Where(p => p.IsAvailable()).ToList();

            if (!products.Any())
                return new List<ProductOverviewModel>();

            //prepare model
            return (await PrepareProductOverviewModels(products, true, true, productThumbPictureSize)).ToList();
        }
        public virtual async Task<IList<ProductOverviewModel>> PrepareProductsSuggested(int? productThumbPictureSize)
        {
            var products = await _productService.GetSuggestedProducts(_workContext.CurrentCustomer.CustomerTags.ToArray());

            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();

            //availability dates
            products = products.Where(p => p.IsAvailable()).ToList();

            if (!products.Any())
                return new List<ProductOverviewModel>();

            //prepare model
            return (await PrepareProductOverviewModels(products.Take(_catalogSettings.SuggestedProductsNumber), true, true, productThumbPictureSize, prepareSpecificationAttributes: _catalogSettings.ShowSpecAttributeOnCatalogPages)).ToList();
        }
        public virtual async Task<IList<ProductOverviewModel>> PrepareProductsCrossSell(int? productThumbPictureSize, int count)
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                .LimitPerStore(_serviceProvider.GetRequiredService<ShoppingCartSettings>().CartsSharedBetweenStores, _storeContext.CurrentStore.Id)
                .ToList();

            var products = await _productService.GetCrosssellProductsByShoppingCart(cart, count);
            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
            //availability dates
            products = products.Where(p => p.IsAvailable()).ToList();

            if (!products.Any())
                return new List<ProductOverviewModel>();

            return (await PrepareProductOverviewModels(products,
                productThumbPictureSize: productThumbPictureSize, forceRedirectionAfterAddingToCart: true, prepareSpecificationAttributes: _catalogSettings.ShowSpecAttributeOnCatalogPages)
                ).ToList();
        }
        public virtual async Task<IList<ProductOverviewModel>> PrepareProductsRelated(string productId, int? productThumbPictureSize)
        {
            var productIds = await _cacheManager.GetAsync(string.Format(ModelCacheEventConsumer.PRODUCTS_RELATED_IDS_KEY, productId, _storeContext.CurrentStore.Id),
               async () =>
                   (await _productService.GetProductById(productId)).RelatedProducts.OrderBy(x => x.DisplayOrder).Select(x => x.ProductId2).ToArray()
                   );

            //load products
            var products = await _productService.GetProductsByIds(productIds);
            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
            //availability dates
            products = products.Where(p => p.IsAvailable()).ToList();

            if (!products.Any())
                return new List<ProductOverviewModel>();

            return (await PrepareProductOverviewModels(products, true, true, productThumbPictureSize, prepareSpecificationAttributes: _catalogSettings.ShowSpecAttributeOnCatalogPages)).ToList();
        }
        public virtual async Task<IList<ProductOverviewModel>> PrepareProductsSimilar(string productId, int? productThumbPictureSize)
        {
            var productIds = await _cacheManager.GetAsync(string.Format(ModelCacheEventConsumer.PRODUCTS_SIMILAR_IDS_KEY, productId, _storeContext.CurrentStore.Id),
               async () =>
                   (await _productService.GetProductById(productId)).SimilarProducts.OrderBy(x => x.DisplayOrder).Select(x => x.ProductId2).ToArray()
                   );

            //load products
            var products = await _productService.GetProductsByIds(productIds);
            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
            //availability dates
            products = products.Where(p => p.IsAvailable()).ToList();

            if (!products.Any())
                return new List<ProductOverviewModel>();

            return (await PrepareProductOverviewModels(products, true, true, productThumbPictureSize, prepareSpecificationAttributes: _catalogSettings.ShowSpecAttributeOnCatalogPages)).ToList();
        }
        public virtual async Task<IList<ProductOverviewModel>> PrepareProductsRecentlyViewed(int? productThumbPictureSize, bool? preparePriceModel)
        {
            var preparePictureModel = productThumbPictureSize.HasValue;
            var recentlyViewedProductsService = _serviceProvider.GetRequiredService<IRecentlyViewedProductsService>();
            var products = await recentlyViewedProductsService.GetRecentlyViewedProducts(_workContext.CurrentCustomer.Id, _catalogSettings.RecentlyViewedProductsNumber);

            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
            //availability dates
            products = products.Where(p => p.IsAvailable()).ToList();

            if (!products.Any())
                return new List<ProductOverviewModel>();

            //prepare model
            var model = new List<ProductOverviewModel>();
            model.AddRange(await PrepareProductOverviewModels(products,
                preparePriceModel.GetValueOrDefault(),
                preparePictureModel,
                productThumbPictureSize));
            return model;
        }

        public virtual async Task<IList<ProductOverviewModel>> PrepareIdsProducts(string[] productIds, int? productThumbPictureSize)
        {
            //load products
            var products = await _productService.GetProductsByIds(productIds);
            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
            //availability dates
            products = products.Where(p => p.IsAvailable()).ToList();

            if (!products.Any())
                return new List<ProductOverviewModel>();

            return (await PrepareProductOverviewModels(products, true, true, productThumbPictureSize, prepareSpecificationAttributes: _catalogSettings.ShowSpecAttributeOnCatalogPages)).ToList();
        }
    }
}