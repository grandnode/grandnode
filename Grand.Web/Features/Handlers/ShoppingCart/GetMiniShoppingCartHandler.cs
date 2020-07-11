using Grand.Core.Caching;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Media;
using Grand.Domain.Orders;
using Grand.Domain.Tax;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Discounts;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Orders;
using Grand.Services.Seo;
using Grand.Services.Tax;
using Grand.Web.Features.Models.ShoppingCart;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Models.Media;
using Grand.Web.Models.ShoppingCart;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.ShoppingCart
{
    public class GetMiniShoppingCartHandler : IRequestHandler<GetMiniShoppingCart, MiniShoppingCartModel>
    {
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ITaxService _taxService;
        private readonly ICacheManager _cacheManager;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IProductService _productService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ILocalizationService _localizationService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPictureService _pictureService;

        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly OrderSettings _orderSettings;
        private readonly TaxSettings _taxSettings;
        private readonly MediaSettings _mediaSettings;

        public GetMiniShoppingCartHandler(
            IShoppingCartService shoppingCartService,
            IOrderTotalCalculationService orderTotalCalculationService,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            ITaxService taxService,
            ICacheManager cacheManager,
            ICheckoutAttributeService checkoutAttributeService,
            IOrderProcessingService orderProcessingService,
            IProductService productService,
            IProductAttributeFormatter productAttributeFormatter,
            IProductAttributeParser productAttributeParser,
            ILocalizationService localizationService,
            IPriceCalculationService priceCalculationService,
            IPictureService pictureService,
            ShoppingCartSettings shoppingCartSettings,
            OrderSettings orderSettings,
            TaxSettings taxSettings,
            MediaSettings mediaSettings)
        {
            _shoppingCartService = shoppingCartService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _taxService = taxService;
            _cacheManager = cacheManager;
            _checkoutAttributeService = checkoutAttributeService;
            _orderProcessingService = orderProcessingService;
            _productService = productService;
            _productAttributeFormatter = productAttributeFormatter;
            _productAttributeParser = productAttributeParser;
            _localizationService = localizationService;
            _priceCalculationService = priceCalculationService;
            _pictureService = pictureService;
            _shoppingCartSettings = shoppingCartSettings;
            _orderSettings = orderSettings;
            _taxSettings = taxSettings;
            _mediaSettings = mediaSettings;
        }

        public async Task<MiniShoppingCartModel> Handle(GetMiniShoppingCart request, CancellationToken cancellationToken)
        {
            var model = new MiniShoppingCartModel {
                ShowProductImages = _shoppingCartSettings.ShowProductImagesInMiniShoppingCart,
                DisplayShoppingCartButton = true,
                CurrentCustomerIsGuest = request.Customer.IsGuest(),
                AnonymousCheckoutAllowed = _orderSettings.AnonymousCheckoutAllowed,
            };

            if (request.Customer.ShoppingCartItems.Any())
            {
                var shoppingCartTypes = new List<ShoppingCartType>();
                shoppingCartTypes.Add(ShoppingCartType.ShoppingCart);
                shoppingCartTypes.Add(ShoppingCartType.Auctions);
                if (_shoppingCartSettings.AllowOnHoldCart)
                    shoppingCartTypes.Add(ShoppingCartType.OnHoldCart);

                var cart = _shoppingCartService.GetShoppingCart(request.Store.Id, shoppingCartTypes.ToArray());
                model.TotalProducts = cart.Sum(x => x.Quantity);
                if (cart.Any())
                {
                    //subtotal
                    var subTotalIncludingTax = request.TaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
                    var shoppingCartSubTotal = await _orderTotalCalculationService.GetShoppingCartSubTotal(cart, subTotalIncludingTax);
                    decimal orderSubTotalDiscountAmountBase = shoppingCartSubTotal.discountAmount;
                    List<AppliedDiscount> orderSubTotalAppliedDiscounts = shoppingCartSubTotal.appliedDiscounts;
                    decimal subTotalWithoutDiscountBase = shoppingCartSubTotal.subTotalWithoutDiscount;
                    decimal subTotalWithDiscountBase = shoppingCartSubTotal.subTotalWithDiscount;
                    decimal subtotalBase = subTotalWithoutDiscountBase;
                    decimal subtotal = await _currencyService.ConvertFromPrimaryStoreCurrency(subtotalBase, request.Currency);
                    model.SubTotal = _priceFormatter.FormatPrice(subtotal, false, request.Currency, request.Language, subTotalIncludingTax);

                    var requiresShipping = cart.RequiresShipping();
                    //a customer should visit the shopping cart page (hide checkout button) before going to checkout if:
                    //1. "terms of service" are enabled
                    //2. min order sub-total is OK
                    //3. we have at least one checkout attribute
                    var checkoutAttributesExistCacheKey = string.Format(ModelCacheEventConst.CHECKOUTATTRIBUTES_EXIST_KEY,
                        request.Store.Id, requiresShipping);
                    var checkoutAttributesExist = await _cacheManager.GetAsync(checkoutAttributesExistCacheKey, async () =>
                    {
                        var checkoutAttributes = await _checkoutAttributeService.GetAllCheckoutAttributes(request.Store.Id, !requiresShipping);
                        return checkoutAttributes.Any();
                    });

                    bool minOrderSubtotalAmountOk = await _orderProcessingService.ValidateMinOrderSubtotalAmount(cart.Where
                        (x => x.ShoppingCartType == ShoppingCartType.ShoppingCart || x.ShoppingCartType == ShoppingCartType.Auctions).ToList());
                    model.DisplayCheckoutButton = !_orderSettings.TermsOfServiceOnShoppingCartPage &&
                        minOrderSubtotalAmountOk &&
                        !checkoutAttributesExist;

                    //products. sort descending (recently added products)
                    foreach (var sci in cart
                        .OrderByDescending(x => x.Id)
                        .Take(_shoppingCartSettings.MiniShoppingCartProductNumber)
                        .ToList())
                    {
                        var product = await _productService.GetProductById(sci.ProductId);
                        if (product == null)
                            continue;

                        var cartItemModel = new MiniShoppingCartModel.ShoppingCartItemModel {
                            Id = sci.Id,
                            ProductId = product.Id,
                            ProductName = product.GetLocalized(x => x.Name, request.Language.Id),
                            ProductSeName = product.GetSeName(request.Language.Id),
                            Quantity = sci.Quantity,
                            AttributeInfo = await _productAttributeFormatter.FormatAttributes(product, sci.AttributesXml)
                        };
                        if (product.ProductType == ProductType.Reservation)
                        {
                            var reservation = "";
                            if (sci.RentalEndDateUtc == default(DateTime) || sci.RentalEndDateUtc == null)
                            {
                                reservation = string.Format(_localizationService.GetResource("ShoppingCart.Reservation.StartDate"), sci.RentalStartDateUtc?.ToString(_shoppingCartSettings.ReservationDateFormat));
                            }
                            else
                            {
                                reservation = string.Format(_localizationService.GetResource("ShoppingCart.Reservation.Date"), sci.RentalStartDateUtc?.ToString(_shoppingCartSettings.ReservationDateFormat), sci.RentalEndDateUtc?.ToString(_shoppingCartSettings.ReservationDateFormat));
                            }

                            if (!string.IsNullOrEmpty(sci.Parameter))
                            {
                                reservation += "<br>" + string.Format(_localizationService.GetResource("ShoppingCart.Reservation.Option"), sci.Parameter);
                            }
                            if (!string.IsNullOrEmpty(sci.Duration))
                            {
                                reservation += "<br>" + string.Format(_localizationService.GetResource("ShoppingCart.Reservation.Duration"), sci.Duration);
                            }
                            if (string.IsNullOrEmpty(cartItemModel.AttributeInfo))
                                cartItemModel.AttributeInfo = reservation;
                            else
                                cartItemModel.AttributeInfo += "<br>" + reservation;
                        }
                        //unit prices
                        if (product.CallForPrice)
                        {
                            cartItemModel.UnitPrice = _localizationService.GetResource("Products.CallForPrice");
                        }
                        else
                        {
                            var productprices = await _taxService.GetProductPrice(product, (await _priceCalculationService.GetUnitPrice(sci)).unitprice);
                            decimal taxRate = productprices.taxRate;
                            decimal shoppingCartUnitPriceWithDiscountBase = productprices.productprice;
                            decimal shoppingCartUnitPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartUnitPriceWithDiscountBase, request.Currency);
                            cartItemModel.UnitPrice = _priceFormatter.FormatPrice(shoppingCartUnitPriceWithDiscount);
                        }

                        //picture
                        if (_shoppingCartSettings.ShowProductImagesInMiniShoppingCart)
                        {
                            cartItemModel.Picture = await PrepareCartItemPicture(request, product, sci.AttributesXml);
                        }

                        model.Items.Add(cartItemModel);
                    }
                }
            }

            return model;
        }

        private async Task<PictureModel> PrepareCartItemPicture(GetMiniShoppingCart request, Product product, string attributesXml)
        {
            var pictureCacheKey = string.Format(ModelCacheEventConst.CART_PICTURE_MODEL_KEY,
                product.Id, _mediaSettings.MiniCartThumbPictureSize, true, request.Language.Id, request.Store.Id);
            var model = await _cacheManager.GetAsync(pictureCacheKey, async () =>
            {
                var sciPicture = await product.GetProductPicture(attributesXml, _productService, _pictureService, _productAttributeParser);
                return new PictureModel {
                    Id = sciPicture?.Id,
                    ImageUrl = await _pictureService.GetPictureUrl(sciPicture, _mediaSettings.MiniCartThumbPictureSize, true),
                    Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), product.Name),
                    AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), product.Name),
                };
            });

            return model;
        }
    }
}
