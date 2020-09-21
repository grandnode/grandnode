using Grand.Core;
using Grand.Core.Caching;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Media;
using Grand.Domain.Orders;
using Grand.Domain.Seo;
using Grand.Domain.Stores;
using Grand.Domain.Vendors;
using Grand.Framework.Security.Captcha;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Messages;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Shipping;
using Grand.Services.Tax;
using Grand.Services.Vendors;
using Grand.Web.Extensions;
using Grand.Web.Features.Models.Catalog;
using Grand.Web.Features.Models.Products;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Models.Catalog;
using Grand.Web.Models.Media;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Products
{
    public class GetProductDetailsPageHandler : IRequestHandler<GetProductDetailsPage, ProductDetailsModel>
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
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IShippingService _shippingService;
        private readonly IVendorService _vendorService;
        private readonly ICategoryService _categoryService;
        private readonly IProductTagService _productTagService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IDownloadService _downloadService;
        private readonly IProductReservationService _productReservationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMediator _mediator;

        private readonly MediaSettings _mediaSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly SeoSettings _seoSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;

        public GetProductDetailsPageHandler(
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
            ICacheManager cacheManager, 
            IPictureService pictureService, 
            IProductAttributeParser productAttributeParser, 
            IShippingService shippingService, 
            IVendorService vendorService, 
            ICategoryService categoryService, 
            IProductTagService productTagService, 
            IProductAttributeService productAttributeService, 
            IManufacturerService manufacturerService, 
            IDateTimeHelper dateTimeHelper, 
            IDownloadService downloadService, 
            IProductReservationService productReservationService,
            IHttpContextAccessor httpContextAccessor,
            IMediator mediator, 
            MediaSettings mediaSettings, 
            CatalogSettings catalogSettings, 
            SeoSettings seoSettings, 
            VendorSettings vendorSettings, 
            CaptchaSettings captchaSettings, 
            ShoppingCartSettings shoppingCartSettings)
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
            _productAttributeParser = productAttributeParser;
            _shippingService = shippingService;
            _vendorService = vendorService;
            _categoryService = categoryService;
            _productTagService = productTagService;
            _productAttributeService = productAttributeService;
            _manufacturerService = manufacturerService;
            _dateTimeHelper = dateTimeHelper;
            _downloadService = downloadService;
            _productReservationService = productReservationService;
            _httpContextAccessor = httpContextAccessor;
            _mediator = mediator;
            _mediaSettings = mediaSettings;
            _catalogSettings = catalogSettings;
            _seoSettings = seoSettings;
            _vendorSettings = vendorSettings;
            _captchaSettings = captchaSettings;
            _shoppingCartSettings = shoppingCartSettings;
        }

        public async Task<ProductDetailsModel> Handle(GetProductDetailsPage request, CancellationToken cancellationToken)
        {
            return await PrepareProductDetailsModel(request.Store, request.Product, request.UpdateCartItem, request.IsAssociatedProduct);
        }

        private async Task<ProductDetailsModel> PrepareProductDetailsModel(Store store, Product product, ShoppingCartItem updateCartItem, bool isAssociatedProduct)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            var model = await PrepareStandardProperties(product, updateCartItem);

            #region Vendor details

            if (_vendorSettings.ShowVendorOnProductDetailsPage)
            {
                model.VendorModel = await PrepareVendorBriefInfoModel(product);
                if (model.VendorModel != null)
                    model.ShowVendor = true;
            }

            #endregion

            #region Page sharing

            if (_catalogSettings.ShowShareButton && !string.IsNullOrEmpty(_catalogSettings.PageShareCode))
            {
                var shareCode = _catalogSettings.PageShareCode;
                if (store.SslEnabled)
                {
                    //need to change the addthis link to be https linked when the page is, so that the page doesnt ask about mixed mode when viewed in https...
                    shareCode = shareCode.Replace("http://", "https://");
                }
                model.PageShareCode = shareCode;
            }

            #endregion

            #region Back in stock subscriptions

            if ((product.ManageInventoryMethod == ManageInventoryMethod.ManageStock
                || product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes) &&
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
                model.Breadcrumb = await PrepareProductBreadcrumbModel(product);
            }

            #endregion

            #region Product tags

            //do not prepare this model for the associated products. anyway it's not used
            if (!isAssociatedProduct)
            {
                model.ProductTags = await PrepareProductTagModel(product);
            }

            #endregion

            #region Pictures

            model.DefaultPictureZoomEnabled = _mediaSettings.DefaultPictureZoomEnabled;
            //default picture
            var defaultPictureSize = isAssociatedProduct ?
                _mediaSettings.AssociatedProductPictureSize :
                _mediaSettings.ProductDetailsPictureSize;
            //prepare picture models
            var cachedPictures = await PrepareProductPictureModel(product, defaultPictureSize, isAssociatedProduct, model.Name);
            model.DefaultPictureModel = cachedPictures.defaultPictureModel;
            model.PictureModels = cachedPictures.pictureModels;

            #endregion

            #region Product price

            model.ProductPrice = await PrepareProductPriceModel(product);

            #endregion

            #region 'Add to cart' model

            model.AddToCart = await PrepareAddToCartModel(product, updateCartItem);

            #endregion

            #region Gift card

            model.GiftCard = PrepareGiftCardModel(product, updateCartItem);

            #endregion

            #region Product attributes

            model.ProductAttributes = await PrepareProductAttributeModel(product, defaultPictureSize, updateCartItem);

            #endregion 

            #region Product specifications

            model.ProductSpecifications = await _mediator.Send(new GetProductSpecification() { 
                Language = _workContext.WorkingLanguage,
                Product = product
            });

            #endregion

            #region Product review overview

            model.ProductReviewOverview = await _mediator.Send(new GetProductReviewOverview() { 
                Product = product,
                Language = _workContext.WorkingLanguage,
                Store = _storeContext.CurrentStore
            });

            #endregion

            #region Tier prices

            if (product.TierPrices.Any() && await _permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
            {
                model.TierPrices = await PrepareProductTierPriceModel(product);
            }

            #endregion

            #region Manufacturers

            string manufacturersCacheKey = string.Format(ModelCacheEventConst.PRODUCT_MANUFACTURERS_MODEL_KEY,
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
                        model.AssociatedProducts.Add(await PrepareProductDetailsModel(store, associatedProduct, null, true));
                }
            }

            #endregion

            #region Product reservations

            await PrepareProductReservation(model, product);

            #endregion Product reservations

            #region Product Bundle

            if (product.ProductType == ProductType.BundledProduct)
            {
                model.ProductBundleModels = await PrepareProductBundleModel(product);
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


        private async Task<ProductDetailsModel> PrepareStandardProperties(Product product, ShoppingCartItem updateCartItem)
        {
            #region Standard properties

            var warehouseId = updateCartItem != null ? updateCartItem.WarehouseId : _storeContext.CurrentStore.DefaultWarehouseId;

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
                StockAvailability = product.FormatStockMessage(warehouseId, "", _localizationService, _productAttributeParser),
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
                        Selected = updateCartItem != null && updateCartItem?.WarehouseId == warehouse.Id
                    });
                }
            }
            //shipping info
            model.IsShipEnabled = product.IsShipEnabled;
            if (product.IsShipEnabled)
            {
                model.IsFreeShipping = product.IsFreeShipping;
                //delivery date
                if (!string.IsNullOrEmpty(product.DeliveryDateId))
                {
                    var deliveryDate = await _shippingService.GetDeliveryDateById(product.DeliveryDateId);
                    if (deliveryDate != null)
                    {
                        model.DeliveryDate = deliveryDate.GetLocalized(dd => dd.Name, _workContext.WorkingLanguage.Id);
                        model.DeliveryColorSquaresRgb = deliveryDate.ColorSquaresRgb;
                    }
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

            return model;

            #endregion
        }

        private async Task<ProductAskQuestionSimpleModel> PrepareProductAskQuestionSimpleModel(Product product)
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

        private async Task<VendorBriefInfoModel> PrepareVendorBriefInfoModel(Product product)
        {
            if (!string.IsNullOrEmpty(product.VendorId))
            {
                var vendor = await _vendorService.GetVendorById(product.VendorId);
                if (vendor != null && !vendor.Deleted && vendor.Active)
                {
                    return new VendorBriefInfoModel {
                        Id = vendor.Id,
                        Name = vendor.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                        SeName = vendor.GetSeName(_workContext.WorkingLanguage.Id),
                    };
                }
            }
            return null;
        }

        private async Task<ProductDetailsModel.ProductBreadcrumbModel> PrepareProductBreadcrumbModel(Product product)
        {
            var breadcrumbCacheKey = string.Format(ModelCacheEventConst.PRODUCT_BREADCRUMB_MODEL_KEY,
                product.Id,
                _workContext.WorkingLanguage.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            return await _cacheManager.GetAsync(breadcrumbCacheKey, async () =>
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
                    var category = await _categoryService.GetCategoryById(productCategories.OrderBy(x => x.DisplayOrder).FirstOrDefault().CategoryId);
                    if (category != null)
                    {
                        foreach (var catBr in await _categoryService.GetCategoryBreadCrumb(category))
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

        private async Task<IList<ProductTagModel>> PrepareProductTagModel(Product product)
        {
            var productTagsCacheKey = string.Format(ModelCacheEventConst.PRODUCTTAG_BY_PRODUCT_MODEL_KEY, product.Id, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);
            return await _cacheManager.GetAsync(productTagsCacheKey, async () =>
            {
                var tags = new List<ProductTagModel>();
                foreach (var item in product.ProductTags)
                {
                    var tag = await _productTagService.GetProductTagByName(item);
                    if (tag != null)
                    {
                        tags.Add(new ProductTagModel() {
                            Id = tag.Id,
                            Name = tag.GetLocalized(y => y.Name, _workContext.WorkingLanguage.Id),
                            SeName = tag.SeName,
                            ProductCount = await _productTagService.GetProductCount(tag.Id, _storeContext.CurrentStore.Id)
                        });
                    }
                }
                return tags;
            });
        }

        private async Task<(PictureModel defaultPictureModel, List<PictureModel> pictureModels)> PrepareProductPictureModel(Product product, int defaultPictureSize, bool isAssociatedProduct, string name)
        {
            var productPicturesCacheKey = string.Format(ModelCacheEventConst.PRODUCT_DETAILS_PICTURES_MODEL_KEY, product.Id, defaultPictureSize,
                isAssociatedProduct, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);
            return await _cacheManager.GetAsync(productPicturesCacheKey, async () =>
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
                    string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat.Details"), name);
                //"alt" attribute
                defaultPictureModel.AlternateText = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.AltAttribute)) ?
                    defaultPicture.AltAttribute :
                    string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat.Details"), name);

                //all pictures
                var pictureModels = new List<PictureModel>();
                foreach (var picture in product.ProductPictures.OrderBy(x => x.DisplayOrder))
                {
                    var pictureModel = new PictureModel {
                        Id = picture.PictureId,
                        ThumbImageUrl = await _pictureService.GetPictureUrl(picture.PictureId, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage),
                        ImageUrl = await _pictureService.GetPictureUrl(picture.PictureId, _mediaSettings.ProductDetailsPictureSize),
                        FullSizeImageUrl = await _pictureService.GetPictureUrl(picture.PictureId),
                        Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat.Details"), name),
                        AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat.Details"), name),
                    };
                    //"title" attribute
                    pictureModel.Title = !string.IsNullOrEmpty(picture.TitleAttribute) ?
                        picture.TitleAttribute :
                        string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat.Details"), name);
                    //"alt" attribute
                    pictureModel.AlternateText = !string.IsNullOrEmpty(picture.AltAttribute) ?
                       picture.AltAttribute :
                       string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat.Details"), name);

                    pictureModels.Add(pictureModel);
                }
                return (defaultPictureModel, pictureModels);
            });
        }

        private async Task<ProductDetailsModel.ProductPriceModel> PrepareProductPriceModel(Product product)
        {
            var displayPrices = await _permissionService.Authorize(StandardPermissionProvider.DisplayPrices);
            var model = new ProductDetailsModel.ProductPriceModel();
            model.ProductId = product.Id;
            if (displayPrices)
            {
                model.HidePrices = false;
                if (product.CustomerEntersPrice)
                {
                    model.CustomerEntersPrice = true;
                }
                else
                {
                    if (product.CallForPrice)
                    {
                        model.CallForPrice = true;
                    }
                    else
                    {
                        var oldproductprice = await _taxService.GetProductPrice(product, product.OldPrice);
                        decimal oldPriceBase = oldproductprice.productprice;
                        decimal finalPriceWithoutDiscountBase = (await (_taxService.GetProductPrice(product, (await _priceCalculationService.GetFinalPrice(product, _workContext.CurrentCustomer, includeDiscounts: false)).finalPrice))).productprice;

                        var appliedPrice = (await _priceCalculationService.GetFinalPrice(product, _workContext.CurrentCustomer, includeDiscounts: true));
                        decimal finalPriceWithDiscountBase = (await _taxService.GetProductPrice(product, appliedPrice.finalPrice)).productprice;

                        decimal oldPrice = await _currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, _workContext.WorkingCurrency);
                        decimal finalPriceWithoutDiscount = await _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithoutDiscountBase, _workContext.WorkingCurrency);
                        decimal finalPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase, _workContext.WorkingCurrency);

                        if (finalPriceWithoutDiscountBase != oldPriceBase && oldPriceBase > decimal.Zero)
                            model.OldPrice = _priceFormatter.FormatPrice(oldPrice);

                        model.Price = _priceFormatter.FormatPrice(finalPriceWithoutDiscount);
                        if (appliedPrice.appliedDiscounts.Any())
                            model.AppliedDiscounts = appliedPrice.appliedDiscounts;
                        if (appliedPrice.preferredTierPrice != null)
                            model.PreferredTierPrice = appliedPrice.preferredTierPrice;

                        if (product.CatalogPrice > 0)
                        {
                            var catalogPrice = await _currencyService.ConvertFromPrimaryStoreCurrency(product.CatalogPrice, _workContext.WorkingCurrency);
                            model.CatalogPrice = _priceFormatter.FormatPrice(catalogPrice);
                        }

                        if (finalPriceWithoutDiscountBase != finalPriceWithDiscountBase)
                            model.PriceWithDiscount = _priceFormatter.FormatPrice(finalPriceWithDiscount);

                        model.PriceValue = finalPriceWithDiscount;

                        //property for German market
                        //we display tax/shipping info only with "shipping enabled" for this product
                        //we also ensure this it's not free shipping
                        model.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoProductDetailsPage
                            && product.IsShipEnabled &&
                            !product.IsFreeShipping;

                        //PAngV baseprice (used in Germany)
                        if (product.BasepriceEnabled)
                            model.BasePricePAngV = await _mediator.Send(new GetFormatBasePrice() 
                            { Currency = _workContext.WorkingCurrency, Product = product, ProductPrice = finalPriceWithDiscountBase });

                        //currency code
                        model.CurrencyCode = _workContext.WorkingCurrency.CurrencyCode;

                        //reservation
                        if (product.ProductType == ProductType.Reservation)
                        {
                            model.IsReservation = true;
                            var priceStr = _priceFormatter.FormatPrice(finalPriceWithDiscount);
                            model.ReservationPrice = _priceFormatter.FormatReservationProductPeriod(product, priceStr);
                        }
                        //auction
                        if (product.ProductType == ProductType.Auction)
                        {
                            model.IsAuction = true;
                            decimal highestBid = await _currencyService.ConvertFromPrimaryStoreCurrency(product.HighestBid, _workContext.WorkingCurrency);
                            model.HighestBid = _priceFormatter.FormatPrice(highestBid);
                            model.HighestBidValue = highestBid;
                            model.DisableBuyButton = product.DisableBuyButton;
                            decimal startPrice = await _currencyService.ConvertFromPrimaryStoreCurrency(product.StartPrice, _workContext.WorkingCurrency);
                            model.StartPrice = _priceFormatter.FormatPrice(startPrice);
                            model.StartPriceValue = startPrice;
                        }
                    }
                }
            }
            else
            {
                model.HidePrices = true;
                model.OldPrice = null;
                model.CatalogPrice = null;
                model.Price = null;
            }
            return model;
        }

        private async Task<ProductDetailsModel.AddToCartModel> PrepareAddToCartModel(Product product, ShoppingCartItem updatecartitem = null)
        {
            var model = new ProductDetailsModel.AddToCartModel();
            model.ProductId = product.Id;
            if (updatecartitem != null)
            {
                model.UpdatedShoppingCartItemId = updatecartitem.Id;
                model.UpdateShoppingCartItemType = updatecartitem.ShoppingCartType;
            }

            //quantity
            model.EnteredQuantity = updatecartitem != null ? updatecartitem.Quantity : product.OrderMinimumQuantity;
            model.MeasureUnit = !string.IsNullOrEmpty(product.UnitId) ? (await _measureService.GetMeasureUnitById(product.UnitId)).Name : string.Empty;

            //allowed quantities
            var allowedQuantities = product.ParseAllowedQuantities();
            foreach (var qty in allowedQuantities)
            {
                model.AllowedQuantities.Add(new SelectListItem {
                    Text = qty.ToString(),
                    Value = qty.ToString(),
                    Selected = updatecartitem != null && updatecartitem.Quantity == qty
                });
            }
            //minimum quantity notification
            if (product.OrderMinimumQuantity > 1)
            {
                model.MinimumQuantityNotification = string.Format(_localizationService.GetResource("Products.MinimumQuantityNotification"), product.OrderMinimumQuantity);
            }
            //'add to cart', 'add to wishlist' buttons
            model.DisableBuyButton = product.DisableBuyButton || !await _permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart);
            model.DisableWishlistButton = product.DisableWishlistButton || !await _permissionService.Authorize(StandardPermissionProvider.EnableWishlist);
            if (!await _permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
            {
                model.DisableBuyButton = true;
                model.DisableWishlistButton = true;
            }
            //pre-order
            if (product.AvailableForPreOrder)
            {
                model.AvailableForPreOrder = !product.PreOrderAvailabilityStartDateTimeUtc.HasValue ||
                    product.PreOrderAvailabilityStartDateTimeUtc.Value >= DateTime.UtcNow;
                model.PreOrderAvailabilityStartDateTimeUtc = product.PreOrderAvailabilityStartDateTimeUtc;
            }

            //customer entered price
            model.CustomerEntersPrice = product.CustomerEntersPrice;
            if (model.CustomerEntersPrice)
            {
                decimal minimumCustomerEnteredPrice = await _currencyService.ConvertFromPrimaryStoreCurrency(product.MinimumCustomerEnteredPrice, _workContext.WorkingCurrency);
                decimal maximumCustomerEnteredPrice = await _currencyService.ConvertFromPrimaryStoreCurrency(product.MaximumCustomerEnteredPrice, _workContext.WorkingCurrency);

                model.CustomerEnteredPrice = updatecartitem != null ? updatecartitem.CustomerEnteredPrice : minimumCustomerEnteredPrice;
                model.CustomerEnteredPriceRange = string.Format(_localizationService.GetResource("Products.EnterProductPrice.Range"),
                    _priceFormatter.FormatPrice(minimumCustomerEnteredPrice, false, false),
                    _priceFormatter.FormatPrice(maximumCustomerEnteredPrice, false, false));
            }
            return model;
        }

        private ProductDetailsModel.GiftCardModel PrepareGiftCardModel(Product product, ShoppingCartItem updatecartitem = null)
        {
            var model = new ProductDetailsModel.GiftCardModel();
            model.IsGiftCard = product.IsGiftCard;
            if (model.IsGiftCard)
            {
                model.GiftCardType = product.GiftCardType;

                if (updatecartitem == null)
                {
                    model.SenderName = _workContext.CurrentCustomer.GetFullName();
                    model.SenderEmail = _workContext.CurrentCustomer.Email;
                }
                else
                {
                    string giftCardRecipientName, giftCardRecipientEmail, giftCardSenderName, giftCardSenderEmail, giftCardMessage;
                    _productAttributeParser.GetGiftCardAttribute(updatecartitem.AttributesXml,
                        out giftCardRecipientName, out giftCardRecipientEmail,
                        out giftCardSenderName, out giftCardSenderEmail, out giftCardMessage);

                    model.RecipientName = giftCardRecipientName;
                    model.RecipientEmail = giftCardRecipientEmail;
                    model.SenderName = giftCardSenderName;
                    model.SenderEmail = giftCardSenderEmail;
                    model.Message = giftCardMessage;
                }
            }
            return model;
        }

        private async Task<IList<ProductDetailsModel.ProductAttributeModel>> PrepareProductAttributeModel(Product product, int defaultPictureSize, ShoppingCartItem updatecartitem = null)
        {
            var model = new List<ProductDetailsModel.ProductAttributeModel>();

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
                if (!string.IsNullOrEmpty(attribute.ValidationFileAllowedExtensions))
                {
                    attributeModel.AllowedFileExtensions = attribute.ValidationFileAllowedExtensions
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }

                var urlselectedValues = !string.IsNullOrEmpty(productAttribute.SeName) ? _httpContextAccessor.HttpContext.Request.Query[productAttribute.SeName].ToList() : new List<string>();
                
                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = attribute.ProductAttributeValues;
                    foreach (var attributeValue in attributeValues.OrderBy(x=>x.DisplayOrder))
                    {
                        var preselected = attributeValue.IsPreSelected;
                        if (urlselectedValues.Any())
                            preselected = urlselectedValues.Contains(attributeValue.Name);

                        //Product Attribute Value - stock availability - support only for some conditions to show 
                        var stockAvailability = string.Empty;
                        if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes 
                            && product.ProductAttributeCombinations.Any() 
                            && product.ProductAttributeMappings.Count == 1)
                        {
                            var attributesXml = _productAttributeParser.AddProductAttribute(string.Empty, attribute, attributeValue.Id);
                            stockAvailability = product.FormatStockMessage(string.Empty, attributesXml, _localizationService, _productAttributeParser);
                        }
                        var valueModel = new ProductDetailsModel.ProductAttributeValueModel {
                            Id = attributeValue.Id,
                            Name = attributeValue.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                            ColorSquaresRgb = attributeValue.ColorSquaresRgb, //used with "Color squares" attribute type
                            IsPreSelected = preselected,
                            StockAvailability = stockAvailability,
                        };
                        attributeModel.Values.Add(valueModel);

                        //display price if allowed
                        var displayPrices = await _permissionService.Authorize(StandardPermissionProvider.DisplayPrices);
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
                        if (!string.IsNullOrEmpty(attributeValue.ImageSquaresPictureId))
                        {
                            var pm = new PictureModel();
                            if (attributeValue.ImageSquaresPictureId != null)
                            {
                                pm = new PictureModel {
                                    Id = attributeValue.ImageSquaresPictureId,
                                    FullSizeImageUrl = await _pictureService.GetPictureUrl(attributeValue.ImageSquaresPictureId),
                                    ImageUrl = await _pictureService.GetPictureUrl(attributeValue.ImageSquaresPictureId, _mediaSettings.ImageSquarePictureSize)
                                };
                            }
                            valueModel.ImageSquaresPictureModel = pm;
                        }

                        //picture of a product attribute value
                        if (!string.IsNullOrEmpty(attributeValue.PictureId))
                        {
                            var pm = new PictureModel();
                            if (attributeValue.PictureId != null)
                            {
                                pm = new PictureModel {
                                    Id = attributeValue.PictureId,
                                    FullSizeImageUrl = await _pictureService.GetPictureUrl(attributeValue.PictureId),
                                    ImageUrl = await _pictureService.GetPictureUrl(attributeValue.PictureId, defaultPictureSize)
                                };
                            }
                            valueModel.PictureModel = pm;
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

                model.Add(attributeModel);
            }
            return model;
        }
        
        private async Task<IList<ProductDetailsModel.TierPriceModel>> PrepareProductTierPriceModel(Product product)
        {
            var model = new List<ProductDetailsModel.TierPriceModel>();
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
                model.Add(tier);
            }
            return model;
        }
        
        private async Task PrepareProductReservation(ProductDetailsModel model, Product product)
        {
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
        }
        
        private async Task<IList<ProductDetailsModel.ProductBundleModel>> PrepareProductBundleModel(Product product)
        {
            var model = new List<ProductDetailsModel.ProductBundleModel>();
            var displayPrices = await _permissionService.Authorize(StandardPermissionProvider.DisplayPrices);

            foreach (var bundle in product.BundleProducts.OrderBy(x => x.DisplayOrder))
            {
                var p1 = await _productService.GetProductById(bundle.ProductId);
                if (p1 != null && p1.Published && p1.IsAvailable())
                {
                    var bundleProduct = new ProductDetailsModel.ProductBundleModel() {
                        ProductId = p1.Id,
                        Name = p1.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                        ShortDescription = p1.GetLocalized(x => x.ShortDescription, _workContext.WorkingLanguage.Id),
                        SeName = p1.GetSeName(_workContext.WorkingLanguage.Id),
                        Sku = p1.Sku,
                        Mpn = p1.ManufacturerPartNumber,
                        Gtin = p1.Gtin,
                        Quantity = bundle.Quantity,
                        GenericAttributes = p1.GenericAttributes
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
                    var productbundlePicturesCacheKey = string.Format(ModelCacheEventConst.PRODUCT_DETAILS_PICTURES_MODEL_KEY,
                        p1.Id, _mediaSettings.ProductBundlePictureSize, false, _workContext.WorkingLanguage.Id,
                        _storeContext.CurrentStore.Id);

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
                    bundleProduct.ProductAttributes = await PrepareProductAttributeModel(p1, _mediaSettings.ProductBundlePictureSize);

                    model.Add(bundleProduct);
                }
            }
            return model;
        }
    }
}
