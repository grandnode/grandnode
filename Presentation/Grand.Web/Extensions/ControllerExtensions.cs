using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Media;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Tax;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Models.Catalog;
using Grand.Web.Models.Media;

namespace Grand.Web.Extensions
{
    //here we have some methods shared between controllers
    public static class ControllerExtensions
    {
        public static IList<ProductSpecificationModel> PrepareProductSpecificationModel(this Controller controller,
            IWorkContext workContext,
            ISpecificationAttributeService specificationAttributeService,
            ICacheManager cacheManager,
            Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            string cacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_SPECS_MODEL_KEY, product.Id, workContext.WorkingLanguage.Id);
            return cacheManager.Get(cacheKey, () =>
                //specificationAttributeService.GetProductSpecificationAttributes(product.Id, 0, null, true)
                product.ProductSpecificationAttributes.Where(x=>x.ShowOnProductPage)
                .Select(psa =>
                {
                    var specificationAttribute = specificationAttributeService.GetSpecificationAttributeById(psa.SpecificationAttributeId);
                    var m = new ProductSpecificationModel
                    {
                        SpecificationAttributeId = psa.SpecificationAttributeId,
                        SpecificationAttributeName = specificationAttribute.GetLocalized(x => x.Name),
                        ColorSquaresRgb = specificationAttribute.SpecificationAttributeOptions.Where(x => x.Id == psa.SpecificationAttributeOptionId).FirstOrDefault() != null ? specificationAttribute.SpecificationAttributeOptions.Where(x => x.Id == psa.SpecificationAttributeOptionId).FirstOrDefault().ColorSquaresRgb : "",
                    };

                    switch (psa.AttributeType)
                    {
                        case SpecificationAttributeType.Option:
                            m.ValueRaw = HttpUtility.HtmlEncode(specificationAttribute.SpecificationAttributeOptions.Where(x=>x.Id== psa.SpecificationAttributeOptionId).FirstOrDefault().GetLocalized(x => x.Name));
                            break;
                        case SpecificationAttributeType.CustomText:
                            m.ValueRaw = HttpUtility.HtmlEncode(psa.CustomValue);
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

        public static IEnumerable<ProductOverviewModel> PrepareProductOverviewModels(this Controller controller,
            IWorkContext workContext,
            IStoreContext storeContext,
            ICategoryService categoryService,
            IProductService productService,
            ISpecificationAttributeService specificationAttributeService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IPermissionService permissionService,
            ILocalizationService localizationService,
            ITaxService taxService,
            ICurrencyService currencyService,
            IPictureService pictureService,
            IMeasureService measureService,
            IWebHelper webHelper,
            ICacheManager cacheManager,
            CatalogSettings catalogSettings,
            MediaSettings mediaSettings,
            IEnumerable<Product> products,
            bool preparePriceModel = true, bool preparePictureModel = true,
            int? productThumbPictureSize = null, bool prepareSpecificationAttributes = false,
            bool forceRedirectionAfterAddingToCart = false)
        {
            if (products == null)
                throw new ArgumentNullException("products");

            var displayPrices = permissionService.Authorize(StandardPermissionProvider.DisplayPrices);
            var enableShoppingCart = permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart);
            var enableWishlist = permissionService.Authorize(StandardPermissionProvider.EnableWishlist);
            var currentCustomer = workContext.CurrentCustomer;
            var currentCurrency = workContext.WorkingCurrency;
            var currentStoreId = storeContext.CurrentStore.Id;
            var currentLanguageId = workContext.WorkingLanguage;
            int pictureSize = productThumbPictureSize.HasValue ? productThumbPictureSize.Value : mediaSettings.ProductThumbPictureSize;

            var res = new Dictionary<string, string>
            {
                { "Products.CallForPrice", localizationService.GetResource("Products.CallForPrice") },
                { "Products.PriceRangeFrom", localizationService.GetResource("Products.PriceRangeFrom")},
                { "Media.Product.ImageLinkTitleFormat", localizationService.GetResource("Media.Product.ImageLinkTitleFormat") },
                { "Media.Product.ImageAlternateTextFormat", localizationService.GetResource("Media.Product.ImageAlternateTextFormat") }
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
                    SeName = product.GetSeName(),
                    ProductType = product.ProductType,
                    Sku = product.Sku,
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

                                var associatedProducts = productService.GetAssociatedProducts(product.Id, currentStoreId);

                                //add to cart button (ignore "DisableBuyButton" property for grouped products)
                                priceModel.DisableBuyButton = !enableShoppingCart || !displayPrices;

                                //add to wishlist button (ignore "DisableWishlistButton" property for grouped products)
                                priceModel.DisableWishlistButton = !enableWishlist || !displayPrices;

                                //compare products
                                priceModel.DisableAddToCompareListButton = !catalogSettings.CompareProductsEnabled;

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
                                            priceModel.DisableAddToCompareListButton = !catalogSettings.CompareProductsEnabled;
                                            //priceModel.AvailableForPreOrder = false;

                                            if (displayPrices)
                                            {
                                                //find a minimum possible price
                                                decimal? minPossiblePrice = null;
                                                Product minPriceProduct = null;
                                                foreach (var associatedProduct in associatedProducts)
                                                {
                                                    //calculate for the maximum quantity (in case if we have tier prices)
                                                    var tmpPrice = priceCalculationService.GetFinalPrice(associatedProduct,
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
                                                        decimal finalPriceBase = taxService.GetProductPrice(minPriceProduct, minPossiblePrice.Value, out taxRate);
                                                        decimal finalPrice = currencyService.ConvertFromPrimaryStoreCurrency(finalPriceBase, currentCurrency);

                                                        priceModel.OldPrice = null;
                                                        priceModel.Price = String.Format(res["Products.PriceRangeFrom"], priceFormatter.FormatPrice(finalPrice));
                                                        priceModel.PriceValue = finalPrice;

                                                        //PAngV baseprice (used in Germany)
                                                        priceModel.BasePricePAngV = product.FormatBasePrice(finalPrice,
                                                            localizationService, measureService, currencyService, workContext, priceFormatter);
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
                        default:
                            {
                                #region Simple product

                                //add to cart button
                                priceModel.DisableBuyButton = product.DisableBuyButton || !enableShoppingCart || !displayPrices;

                                //add to wishlist button
                                priceModel.DisableWishlistButton = product.DisableWishlistButton || !enableWishlist || !displayPrices;
                                //compare products
                                priceModel.DisableAddToCompareListButton = !catalogSettings.CompareProductsEnabled;

                                //rental
                                priceModel.IsRental = product.IsRental;

                                //pre-order
                                if (product.AvailableForPreOrder)
                                {
                                    priceModel.AvailableForPreOrder = !product.PreOrderAvailabilityStartDateTimeUtc.HasValue ||
                                        product.PreOrderAvailabilityStartDateTimeUtc.Value >= DateTime.UtcNow;
                                    priceModel.PreOrderAvailabilityStartDateTimeUtc = product.PreOrderAvailabilityStartDateTimeUtc;
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
                                            decimal minPossiblePrice = priceCalculationService.GetFinalPrice(product,
                                                currentCustomer, decimal.Zero, true, int.MaxValue);

                                            decimal taxRate;
                                            decimal oldPriceBase = taxService.GetProductPrice(product, product.OldPrice, out taxRate);
                                            decimal finalPriceBase = taxService.GetProductPrice(product, minPossiblePrice, out taxRate);

                                            decimal oldPrice = currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, currentCurrency);
                                            decimal finalPrice = currencyService.ConvertFromPrimaryStoreCurrency(finalPriceBase, currentCurrency);

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
                                                priceModel.Price = String.Format(res["Products.PriceRangeFrom"], priceFormatter.FormatPrice(finalPrice));
                                                priceModel.PriceValue = finalPrice;
                                            }
                                            else
                                            {
                                                if (finalPriceBase != oldPriceBase && oldPriceBase != decimal.Zero)
                                                {
                                                    priceModel.OldPrice = priceFormatter.FormatPrice(oldPrice);
                                                    priceModel.Price = priceFormatter.FormatPrice(finalPrice);
                                                    priceModel.PriceValue = finalPrice;
                                                }
                                                else
                                                {
                                                    priceModel.OldPrice = null;
                                                    priceModel.Price = priceFormatter.FormatPrice(finalPrice);
                                                    priceModel.PriceValue = finalPrice;
                                                }
                                            }
                                            if (product.IsRental)
                                            {
                                                //rental product
                                                priceModel.OldPrice = priceFormatter.FormatRentalProductPeriod(product, priceModel.OldPrice);
                                                priceModel.Price = priceFormatter.FormatRentalProductPeriod(product, priceModel.Price);
                                            }


                                            //property for German market
                                            //we display tax/shipping info only with "shipping enabled" for this product
                                            //we also ensure this it's not free shipping
                                            priceModel.DisplayTaxShippingInfo = catalogSettings.DisplayTaxShippingInfoProductBoxes && product.IsShipEnabled && !product.IsFreeShipping;

                                            //PAngV baseprice (used in Germany)
                                            if(product.BasepriceEnabled)
                                                priceModel.BasePricePAngV = product.FormatBasePrice(finalPrice, localizationService, measureService, currencyService, workContext, priceFormatter);

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
                    var defaultProductPictureCacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_DEFAULTPICTURE_MODEL_KEY, product.Id, pictureSize, true, currentLanguageId, webHelper.IsCurrentConnectionSecured(), currentStoreId);
                    model.DefaultPictureModel = cacheManager.Get(defaultProductPictureCacheKey, () =>
                    {
                        var picture = product.ProductPictures.FirstOrDefault(); 
                        if (picture == null)
                            picture = new ProductPicture();

                        var pictureModel = new PictureModel
                        {
                            ImageUrl = pictureService.GetPictureUrl(picture.PictureId, mediaSettings.ApplyWatermarkForProduct ,pictureSize),
                            FullSizeImageUrl = pictureService.GetPictureUrl(picture.PictureId, mediaSettings.ApplyWatermarkForProduct)
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
                    model.SpecificationAttributeModels = PrepareProductSpecificationModel(controller, workContext,
                         specificationAttributeService, cacheManager, product);
                }

                //reviews
                model.ReviewOverviewModel = controller.PrepareProductReviewOverviewModel(productService, storeContext, catalogSettings, cacheManager, product);

                models.Add(model);
            }
            return models;
        }

        public static ProductReviewOverviewModel PrepareProductReviewOverviewModel(this Controller controller,
           IProductService productService,
           IStoreContext storeContext,
           CatalogSettings catalogSettings,
           ICacheManager cacheManager,
           Product product)
        {
            ProductReviewOverviewModel productReview = null;

            if (catalogSettings.ShowProductReviewsPerStore)
            {
                string cacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_REVIEWS_MODEL_KEY, product.Id, storeContext.CurrentStore.Id);

                productReview = cacheManager.Get(cacheKey, () =>
                {
                    return new ProductReviewOverviewModel
                    {
                        RatingSum = productService.RatingSumProduct(product.Id, catalogSettings.ShowProductReviewsPerStore ? storeContext.CurrentStore.Id: ""),
                        TotalReviews = productService.TotalReviewsProduct(product.Id, catalogSettings.ShowProductReviewsPerStore ? storeContext.CurrentStore.Id: ""),
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

    }
}