using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grand.Core.Caching;
using Grand.Domain.Catalog;
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

namespace Grand.Web.Features.Handlers.ShoppingCart
{
    public class GetAddToCartHandler : IRequestHandler<GetAddToCart, AddToCartModel>
    {
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly ILocalizationService _localizationService;
        private readonly ITaxService _taxService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICacheManager _cacheManager;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IAuctionService _auctionService;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly TaxSettings _taxSettings;
        private readonly MediaSettings _mediaSettings;

        public GetAddToCartHandler(
            IProductAttributeFormatter productAttributeFormatter, 
            ILocalizationService localizationService, 
            ITaxService taxService, 
            IPriceCalculationService priceCalculationService, 
            ICurrencyService currencyService, 
            IPriceFormatter priceFormatter, 
            IShoppingCartService shoppingCartService, 
            ICacheManager cacheManager, 
            IOrderTotalCalculationService orderTotalCalculationService, 
            IPictureService pictureService, 
            IProductService productService, 
            IProductAttributeParser productAttributeParser,
            IAuctionService auctionService,
            ShoppingCartSettings shoppingCartSettings, 
            TaxSettings taxSettings, 
            MediaSettings mediaSettings)
        {
            _productAttributeFormatter = productAttributeFormatter;
            _localizationService = localizationService;
            _taxService = taxService;
            _priceCalculationService = priceCalculationService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _shoppingCartService = shoppingCartService;
            _cacheManager = cacheManager;
            _orderTotalCalculationService = orderTotalCalculationService;
            _pictureService = pictureService;
            _productService = productService;
            _productAttributeParser = productAttributeParser;
            _auctionService = auctionService;
            _shoppingCartSettings = shoppingCartSettings;
            _taxSettings = taxSettings;
            _mediaSettings = mediaSettings;
        }

        public async Task<AddToCartModel> Handle(GetAddToCart request, CancellationToken cancellationToken)
        {
            var model = new AddToCartModel();
            model.AttributeDescription = await _productAttributeFormatter.FormatAttributes(request.Product, request.AttributesXml);
            model.ProductSeName = request.Product.GetSeName(request.Language.Id);
            model.CartType = request.CartType;
            model.ProductId = request.Product.Id;
            model.ProductName = request.Product.GetLocalized(x => x.Name, request.Language.Id);
            model.Quantity = request.Quantity;

            //reservation info
            if (request.Product.ProductType == ProductType.Reservation)
            {
                if (request.EndDate == default(DateTime) || request.EndDate == null)
                {
                    model.ReservationInfo = string.Format(_localizationService.GetResource("ShoppingCart.Reservation.StartDate"), request.StartDate?.ToString(_shoppingCartSettings.ReservationDateFormat));
                }
                else
                {
                    model.ReservationInfo = string.Format(_localizationService.GetResource("ShoppingCart.Reservation.Date"), request.StartDate?.ToString(_shoppingCartSettings.ReservationDateFormat), request.EndDate?.ToString(_shoppingCartSettings.ReservationDateFormat));
                }

                if (!string.IsNullOrEmpty(request.Parameter))
                {
                    model.ReservationInfo += "<br>" + string.Format(_localizationService.GetResource("ShoppingCart.Reservation.Option"), request.Parameter);
                }
                if (!string.IsNullOrEmpty(request.Duration))
                {
                    model.ReservationInfo += "<br>" + string.Format(_localizationService.GetResource("ShoppingCart.Reservation.Duration"), request.Duration);
                }
            }

            if (request.CartType != ShoppingCartType.Auctions)
            {
                var sci = request.Customer.ShoppingCartItems.FirstOrDefault(x => x.ProductId == request.Product.Id 
                && (string.IsNullOrEmpty(x.AttributesXml) ? "" : x.AttributesXml) == request.AttributesXml);
                model.ItemQuantity = sci.Quantity;

                //unit prices
                if (request.Product.CallForPrice)
                {
                    model.Price = _localizationService.GetResource("Products.CallForPrice");
                }
                else
                {
                    var productprices = await _taxService.GetProductPrice(request.Product, (await _priceCalculationService.GetUnitPrice(sci)).unitprice);
                    decimal taxRate = productprices.taxRate;
                    decimal shoppingCartUnitPriceWithDiscountBase = productprices.productprice;
                    decimal shoppingCartUnitPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartUnitPriceWithDiscountBase, request.Currency);
                    model.Price = request.CustomerEnteredPrice == 0 ? _priceFormatter.FormatPrice(shoppingCartUnitPriceWithDiscount) : _priceFormatter.FormatPrice(request.CustomerEnteredPrice);
                    model.DecimalPrice = request.CustomerEnteredPrice == 0 ? shoppingCartUnitPriceWithDiscount : request.CustomerEnteredPrice;
                    model.TotalPrice = _priceFormatter.FormatPrice(shoppingCartUnitPriceWithDiscount * sci.Quantity);
                }

                //picture
                model.Picture = await PrepareCartItemPicture(request);
            }
            else
            {
                model.Picture = await PrepareCartItemPicture(request);
            }

            var cart = _shoppingCartService.GetShoppingCart(request.Store.Id, request.CartType);

            if (request.CartType != ShoppingCartType.Auctions)
            {
                model.TotalItems = cart.Sum(x => x.Quantity);
            }
            else
            {
                model.TotalItems = 0;
                var grouped = (await _auctionService.GetBidsByCustomerId(request.Customer.Id)).GroupBy(x => x.ProductId);
                foreach (var item in grouped)
                {
                    var p = await _productService.GetProductById(item.Key);
                    if (p != null && p.AvailableEndDateTimeUtc > DateTime.UtcNow)
                    {
                        model.TotalItems++;
                    }
                }
            }


            if (request.CartType == ShoppingCartType.ShoppingCart)
            {
                var subTotalIncludingTax = request.TaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
                var shoppingCartSubTotal = await _orderTotalCalculationService.GetShoppingCartSubTotal(cart, subTotalIncludingTax);
                decimal orderSubTotalDiscountAmountBase = shoppingCartSubTotal.discountAmount;
                List<AppliedDiscount> orderSubTotalAppliedDiscounts = shoppingCartSubTotal.appliedDiscounts;
                decimal subTotalWithoutDiscountBase = shoppingCartSubTotal.subTotalWithoutDiscount;
                decimal subTotalWithDiscountBase = shoppingCartSubTotal.subTotalWithDiscount;
                decimal subtotalBase = subTotalWithoutDiscountBase;
                decimal subtotal = await _currencyService.ConvertFromPrimaryStoreCurrency(subtotalBase, request.Currency);
                model.SubTotal = _priceFormatter.FormatPrice(subtotal, true, request.Currency, request.Language, subTotalIncludingTax);
                model.DecimalSubTotal = subtotal;
                if (orderSubTotalDiscountAmountBase > decimal.Zero)
                {
                    decimal orderSubTotalDiscountAmount = await _currencyService.ConvertFromPrimaryStoreCurrency(orderSubTotalDiscountAmountBase, request.Currency);
                    model.SubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountAmount, true, request.Currency, request.Language, subTotalIncludingTax);
                }
            }
            else if (request.CartType == ShoppingCartType.Auctions)
            {
                model.IsAuction = true;
                model.HighestBidValue = request.Product.HighestBid;
                model.HighestBid = _priceFormatter.FormatPrice(request.Product.HighestBid);
                model.EndTime = request.Product.AvailableEndDateTimeUtc;
            }

            return model;

        }

        private async Task<PictureModel> PrepareCartItemPicture(GetAddToCart request)
        {
            var pictureCacheKey = string.Format(ModelCacheEventConst.CART_PICTURE_MODEL_KEY, 
                request.Product.Id, _mediaSettings.AddToCartThumbPictureSize, true, request.Language.Id, request.Store.Id);
            var model = await _cacheManager.GetAsync(pictureCacheKey, async () =>
            {
                var sciPicture = await request.Product.GetProductPicture(request.AttributesXml, _productService, _pictureService, _productAttributeParser);
                return new PictureModel {
                    Id = sciPicture?.Id,
                    ImageUrl = await _pictureService.GetPictureUrl(sciPicture, _mediaSettings.AddToCartThumbPictureSize, true),
                    Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), request.Product.Name),
                    AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), request.Product.Name),
                };
            });

            return model;
        }
    }
}
