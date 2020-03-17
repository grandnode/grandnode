using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
using Grand.Services.Directory;
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
using Grand.Web.Features.Models.Products;
using Grand.Web.Models.Catalog;
using Grand.Web.Models.Media;
using MediatR;

namespace Grand.Web.Features.Handlers.Products
{
    public class GetProductOverviewHandler : IRequestHandler<GetProductOverview, IEnumerable<ProductOverviewModel>>
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
        private readonly IPictureService _pictureService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IMediator _mediator;

        private readonly MediaSettings _mediaSettings;
        private readonly CatalogSettings _catalogSettings;

        public GetProductOverviewHandler(
            IPermissionService permissionService, 
            IWorkContext workContext, 
            IStoreContext storeContext, 
            ILocalizationService localizationService, 
            IProductService productService, 
            IPriceCalculationService priceCalculationService, 
            ITaxService taxService, 
            ICurrencyService currencyService, 
            IPriceFormatter priceFormatter, 
            IMeasureService measureService, 
            IPictureService pictureService, 
            IDateTimeHelper dateTimeHelper, 
            IMediator mediator, 
            MediaSettings mediaSettings, 
            CatalogSettings catalogSettings)
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
            _pictureService = pictureService;
            _dateTimeHelper = dateTimeHelper;
            _mediator = mediator;
            _mediaSettings = mediaSettings;
            _catalogSettings = catalogSettings;
        }

        public async Task<IEnumerable<ProductOverviewModel>> Handle(GetProductOverview request, CancellationToken cancellationToken)
        {
            if (request.Products == null)
                throw new ArgumentNullException("products");

            var displayPrices = await _permissionService.Authorize(StandardPermissionProvider.DisplayPrices, _workContext.CurrentCustomer);
            var enableShoppingCart = await _permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart, _workContext.CurrentCustomer);
            var enableWishlist = await _permissionService.Authorize(StandardPermissionProvider.EnableWishlist, _workContext.CurrentCustomer);
            int pictureSize = request.ProductThumbPictureSize.HasValue ? request.ProductThumbPictureSize.Value : _mediaSettings.ProductThumbPictureSize;
            bool priceIncludesTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax;

            var res = new Dictionary<string, string>
            {
                { "Products.CallForPrice", _localizationService.GetResource("Products.CallForPrice", _workContext.WorkingLanguage.Id) },
                { "Products.PriceRangeFrom", _localizationService.GetResource("Products.PriceRangeFrom", _workContext.WorkingLanguage.Id)},
                { "Media.Product.ImageLinkTitleFormat", _localizationService.GetResource("Media.Product.ImageLinkTitleFormat", _workContext.WorkingLanguage.Id) },
                { "Media.Product.ImageAlternateTextFormat", _localizationService.GetResource("Media.Product.ImageAlternateTextFormat", _workContext.WorkingLanguage.Id) }
            };

            var models = new List<ProductOverviewModel>();

            foreach (var product in request.Products)
            {
                var model = PrepareProductOverviewModel(product);
                //price
                if (request.PreparePriceModel)
                {
                    await PreparePriceModel(model, product, res, request.ForceRedirectionAfterAddingToCart,
                        enableShoppingCart, displayPrices, enableWishlist, priceIncludesTax);
                }

                //picture
                if (request.PreparePictureModel)
                {
                    await PreparePictureModel(model, product, res, pictureSize);
                }

                //specs
                if (request.PrepareSpecificationAttributes && product.ProductSpecificationAttributes.Any())
                {
                    model.SpecificationAttributeModels = await _mediator.Send(new GetProductSpecification() { Language = _workContext.WorkingLanguage, Product = product });
                }

                //reviews
                model.ReviewOverviewModel = await _mediator.Send(new GetProductReviewOverview() { Product = product, Language = _workContext.WorkingLanguage, Store = _storeContext.CurrentStore });

                models.Add(model);
            }
            return models;
        }

        private ProductOverviewModel PrepareProductOverviewModel(Product product)
        {
            var model = new ProductOverviewModel {
                Id = product.Id,
                Name = product.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                ShortDescription = product.GetLocalized(x => x.ShortDescription, _workContext.WorkingLanguage.Id),
                FullDescription = product.GetLocalized(x => x.FullDescription, _workContext.WorkingLanguage.Id),
                SeName = product.GetSeName(_workContext.WorkingLanguage.Id),
                ProductType = product.ProductType,
                Sku = product.Sku,
                Gtin = product.Gtin,
                Flag = product.Flag,
                ManufacturerPartNumber = product.ManufacturerPartNumber,
                IsFreeShipping = product.IsFreeShipping,
                ShowSku = _catalogSettings.ShowSkuOnCatalogPages,
                TaxDisplayType = _workContext.TaxDisplayType,
                EndTime = product.AvailableEndDateTimeUtc,
                EndTimeLocalTime = product.AvailableEndDateTimeUtc.HasValue ? _dateTimeHelper.ConvertToUserTime(product.AvailableEndDateTimeUtc.Value, DateTimeKind.Utc) : new DateTime?(),
                ShowQty = _catalogSettings.DisplayQuantityOnCatalogPages,
                GenericAttributes = product.GenericAttributes,
                MarkAsNew = product.MarkAsNew &&
                        (!product.MarkAsNewStartDateTimeUtc.HasValue || product.MarkAsNewStartDateTimeUtc.Value < DateTime.UtcNow) &&
                        (!product.MarkAsNewEndDateTimeUtc.HasValue || product.MarkAsNewEndDateTimeUtc.Value > DateTime.UtcNow)
            };
            return model;
        }

        private async Task PreparePriceModel(ProductOverviewModel model, Product product, Dictionary<string, string> res,
            bool forceRedirectionAfterAddingToCart, bool enableShoppingCart, bool displayPrices, bool enableWishlist,
            bool priceIncludesTax)
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

                        var associatedProducts = await _productService.GetAssociatedProducts(product.Id, _storeContext.CurrentStore.Id);

                        //add to cart button (ignore "DisableBuyButton" property for grouped products)
                        priceModel.DisableBuyButton = !enableShoppingCart || !displayPrices;

                        //add to wishlist button (ignore "DisableWishlistButton" property for grouped products)
                        priceModel.DisableWishlistButton = !enableWishlist || !displayPrices;

                        //compare products
                        priceModel.DisableAddToCompareListButton = !_catalogSettings.CompareProductsEnabled;

                        //catalog price, not used in views, but it's for front developer
                        if (product.CatalogPrice > 0)
                        {
                            decimal catalogPrice = await _currencyService.ConvertFromPrimaryStoreCurrency(product.CatalogPrice, _workContext.WorkingCurrency);
                            priceModel.CatalogPrice = _priceFormatter.FormatPrice(catalogPrice, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, priceIncludesTax);
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
                                                _workContext.CurrentCustomer, decimal.Zero, true, int.MaxValue)).finalPrice;
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
                                                decimal finalPriceBase = (await _taxService.GetProductPrice(minPriceProduct, minPossiblePrice.Value, priceIncludesTax, _workContext.CurrentCustomer)).productprice;
                                                decimal finalPrice = await _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceBase, _workContext.WorkingCurrency);

                                                priceModel.OldPrice = null;
                                                priceModel.Price = String.Format(res["Products.PriceRangeFrom"], _priceFormatter.FormatPrice(finalPrice, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, priceIncludesTax));
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
                            decimal catalogPrice = await _currencyService.ConvertFromPrimaryStoreCurrency(product.CatalogPrice, _workContext.WorkingCurrency);
                            priceModel.CatalogPrice = _priceFormatter.FormatPrice(catalogPrice, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, priceIncludesTax);
                        }

                        //start price for product auction
                        if (product.StartPrice > 0)
                        {
                            decimal startPrice = await _currencyService.ConvertFromPrimaryStoreCurrency(product.StartPrice, _workContext.WorkingCurrency);
                            priceModel.StartPrice = _priceFormatter.FormatPrice(startPrice, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, priceIncludesTax);
                            priceModel.StartPriceValue = startPrice;
                        }

                        //highest bid for product auction
                        if (product.HighestBid > 0)
                        {
                            decimal highestBid = await _currencyService.ConvertFromPrimaryStoreCurrency(product.HighestBid, _workContext.WorkingCurrency);
                            priceModel.HighestBid = _priceFormatter.FormatPrice(highestBid, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, priceIncludesTax);
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
                                        _workContext.CurrentCustomer, decimal.Zero, true, int.MaxValue));

                                    priceModel.AppliedDiscounts = infoprice.appliedDiscounts;
                                    priceModel.PreferredTierPrice = infoprice.preferredTierPrice;

                                    decimal minPossiblePrice = infoprice.finalPrice;

                                    decimal oldPriceBase = (await _taxService.GetProductPrice(product, product.OldPrice, priceIncludesTax, _workContext.CurrentCustomer)).productprice;
                                    decimal finalPriceBase = (await _taxService.GetProductPrice(product, minPossiblePrice, priceIncludesTax, _workContext.CurrentCustomer)).productprice;

                                    decimal oldPrice = await _currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, _workContext.WorkingCurrency);
                                    decimal finalPrice = await _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceBase, _workContext.WorkingCurrency);

                                    //do we have tier prices configured?
                                    var tierPrices = new List<TierPrice>();
                                    if (product.TierPrices.Any())
                                    {
                                        tierPrices.AddRange(product.TierPrices.OrderBy(tp => tp.Quantity)
                                            .FilterByStore(_storeContext.CurrentStore.Id)
                                            .FilterForCustomer(_workContext.CurrentCustomer)
                                            .FilterByDate()
                                            .RemoveDuplicatedQuantities());
                                    }
                                    //When there is just one tier (with  qty 1), 
                                    //there are no actual savings in the list.
                                    bool displayFromMessage = tierPrices.Any() && !(tierPrices.Count == 1 && tierPrices[0].Quantity <= 1);
                                    if (displayFromMessage)
                                    {
                                        priceModel.OldPrice = null;
                                        priceModel.Price = String.Format(res["Products.PriceRangeFrom"], _priceFormatter.FormatPrice(finalPrice, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, priceIncludesTax));
                                        priceModel.PriceValue = finalPrice;
                                    }
                                    else
                                    {
                                        if (finalPriceBase != oldPriceBase && oldPriceBase != decimal.Zero)
                                        {
                                            priceModel.OldPrice = _priceFormatter.FormatPrice(oldPrice, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, priceIncludesTax);
                                            priceModel.OldPriceValue = oldPrice;
                                            priceModel.Price = _priceFormatter.FormatPrice(finalPrice, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, priceIncludesTax);
                                            priceModel.PriceValue = finalPrice;
                                        }
                                        else
                                        {
                                            priceModel.OldPrice = null;
                                            priceModel.Price = _priceFormatter.FormatPrice(finalPrice, true, _workContext.WorkingCurrency, _workContext.WorkingLanguage, priceIncludesTax);
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

        private async Task PreparePictureModel(ProductOverviewModel model, Product product, Dictionary<string, string> res, int pictureSize)
        {
            #region Prepare product picture

            async Task<PictureModel> PreparePictureModel(ProductPicture picture)
            {
                if (picture == null)
                    picture = new ProductPicture();

                var pictureModel = new PictureModel {
                    Id = picture.PictureId,
                    ImageUrl = await _pictureService.GetPictureUrl(picture.PictureId, pictureSize),
                    FullSizeImageUrl = await _pictureService.GetPictureUrl(picture.PictureId)
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
            };

            //prepare picture model
            model.DefaultPictureModel = await PreparePictureModel(product.ProductPictures.OrderBy(x => x.DisplayOrder).FirstOrDefault());

            //prepare second picture model
            if (_catalogSettings.SecondPictureOnCatalogPages)
            {
                model.SecondPictureModel = await PreparePictureModel(product.ProductPictures.OrderBy(x => x.DisplayOrder).Skip(1).Take(1).FirstOrDefault());
            }

            #endregion

        }
    }

}