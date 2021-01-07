using Grand.Domain.Catalog;
using Grand.Domain.Shipping;
using Grand.Domain.Tax;
using Grand.Domain.Vendors;
using Grand.Services.Catalog;
using Grand.Services.Commands.Models.Messages;
using Grand.Services.Common;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Messages.DotLiquidDrops;
using Grand.Services.Orders;
using Grand.Services.Payments;
using Grand.Services.Vendors;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Commands.Handlers.Messages
{
    public class GetOrderTokensCommandHandler : IRequestHandler<GetOrderTokensCommand, LiquidOrder>
    {
        private readonly ILanguageService _languageService;
        private readonly ICurrencyService _currencyService;
        private readonly IProductService _productService;
        private readonly IDownloadService _downloadService;
        private readonly IVendorService _vendorService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IPaymentService _paymentService;
        private readonly ILocalizationService _localizationService;
        private readonly IGiftCardService _giftCardService;

        private readonly CatalogSettings _catalogSettings;
        private readonly TaxSettings _taxSettings;

        public GetOrderTokensCommandHandler(
            ILanguageService languageService, 
            ICurrencyService currencyService, 
            IProductService productService, 
            IDownloadService downloadService, 
            IVendorService vendorService, 
            IPriceFormatter priceFormatter, 
            IProductAttributeParser productAttributeParser, 
            ICountryService countryService, 
            IStateProvinceService stateProvinceService,
            IAddressAttributeParser addressAttributeParser, 
            IPaymentService paymentService, 
            ILocalizationService localizationService, 
            IGiftCardService giftCardService, 
            CatalogSettings catalogSettings, 
            TaxSettings taxSettings)
        {
            _languageService = languageService;
            _currencyService = currencyService;
            _productService = productService;
            _downloadService = downloadService;
            _vendorService = vendorService;
            _priceFormatter = priceFormatter;
            _productAttributeParser = productAttributeParser;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _addressAttributeParser = addressAttributeParser;
            _paymentService = paymentService;
            _localizationService = localizationService;
            _giftCardService = giftCardService;
            _catalogSettings = catalogSettings;
            _taxSettings = taxSettings;
        }

        public async Task<LiquidOrder> Handle(GetOrderTokensCommand request, CancellationToken cancellationToken)
        {
            var language = await _languageService.GetLanguageById(request.Order.CustomerLanguageId);
            var currency = await _currencyService.GetCurrencyByCode(request.Order.CustomerCurrencyCode);

            var liquidOrder = new LiquidOrder(request.Order, request.Customer, language, currency, request.Store, request.OrderNote, request.Vendor);
            foreach (var item in request.Order.OrderItems.Where(x => x.VendorId == request.Vendor?.Id || request.Vendor == null))
            {
                var product = await _productService.GetProductById(item.ProductId);
                Vendor vendorItem = string.IsNullOrEmpty(item.VendorId) ? null : await _vendorService.GetVendorById(item.VendorId);
                var liqitem = new LiquidOrderItem(item, product, request.Order, language, currency, request.Store, vendorItem);

                #region Download

                liqitem.IsDownloadAllowed = _downloadService.IsDownloadAllowed(request.Order, item, product);
                liqitem.IsLicenseDownloadAllowed = _downloadService.IsLicenseDownloadAllowed(request.Order, item, product);

                #endregion

                #region Unit price
                string unitPriceStr;
                if (request.Order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    unitPriceStr = _priceFormatter.FormatPrice(item.UnitPriceInclTax, true, currency, language, true);
                }
                else
                {
                    //excluding tax
                    unitPriceStr = _priceFormatter.FormatPrice(item.UnitPriceExclTax, true, currency, language, false);
                }
                liqitem.UnitPrice = unitPriceStr;

                #endregion

                #region total price
                string priceStr;
                if (request.Order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    priceStr = _priceFormatter.FormatPrice(item.PriceInclTax, true, currency, language, true);
                }
                else
                {
                    //excluding tax
                    priceStr = _priceFormatter.FormatPrice(item.PriceExclTax, true, currency, language, false);
                }
                liqitem.TotalPrice = priceStr;

                #endregion

                string sku = "";
                if (product != null)
                    sku = product.FormatSku(item.Attributes, _productAttributeParser);

                liqitem.ProductSku = WebUtility.HtmlEncode(sku);
                liqitem.ShowSkuOnProductDetailsPage = _catalogSettings.ShowSkuOnProductDetailsPage;
                liqitem.ProductOldPrice = _priceFormatter.FormatPrice(product.OldPrice, true, currency, language, true);

                liquidOrder.OrderItems.Add(liqitem);
            }

            liquidOrder.BillingCustomAttributes = await _addressAttributeParser.FormatAttributes(language, request.Order.BillingAddress?.Attributes);
            liquidOrder.BillingCountry = request.Order.BillingAddress != null && !string.IsNullOrEmpty(request.Order.BillingAddress.CountryId) ? (await _countryService.GetCountryById(request.Order.BillingAddress.CountryId))?.GetLocalized(x => x.Name, request.Order.CustomerLanguageId) : "";
            liquidOrder.BillingStateProvince = !string.IsNullOrEmpty(request.Order.BillingAddress.StateProvinceId) ? (await _stateProvinceService.GetStateProvinceById(request.Order.BillingAddress.StateProvinceId))?.GetLocalized(x => x.Name, request.Order.CustomerLanguageId) : "";

            liquidOrder.ShippingCountry = request.Order.ShippingAddress != null && !string.IsNullOrEmpty(request.Order.ShippingAddress.CountryId) ? (await _countryService.GetCountryById(request.Order.ShippingAddress.CountryId))?.GetLocalized(x => x.Name, request.Order.CustomerLanguageId) : "";
            liquidOrder.ShippingStateProvince = request.Order.ShippingAddress != null && !string.IsNullOrEmpty(request.Order.ShippingAddress.StateProvinceId) ? (await _stateProvinceService.GetStateProvinceById(request.Order.ShippingAddress.StateProvinceId)).GetLocalized(x => x.Name, request.Order.CustomerLanguageId) : "";
            liquidOrder.ShippingCustomAttributes = await _addressAttributeParser.FormatAttributes(language, request.Order.ShippingAddress != null ? request.Order.ShippingAddress.Attributes : null);

            var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(request.Order.PaymentMethodSystemName);
            liquidOrder.PaymentMethod = paymentMethod != null ? paymentMethod.GetLocalizedFriendlyName(_localizationService, language.Id) : request.Order.PaymentMethodSystemName;
            liquidOrder.AmountRefunded = _priceFormatter.FormatPrice(request.RefundedAmount, true, currency, language, false);

            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var item in request.Order.OrderTaxes)
            {
                string taxRate = string.Format(_localizationService.GetResource("Messages.Order.TaxRateLine"), _priceFormatter.FormatTaxRate(item.Percent));
                string taxValue = _priceFormatter.FormatPrice(item.Amount, true, currency, language, false);
                dict.Add(taxRate, taxValue);
            }
            liquidOrder.TaxRates = dict;

            Dictionary<string, string> cards = new Dictionary<string, string>();
            var gcuhC = await _giftCardService.GetAllGiftCardUsageHistory(request.Order.Id);
            foreach (var gcuh in gcuhC)
            {
                var giftCard = await _giftCardService.GetGiftCardById(gcuh.GiftCardId);
                string giftCardText = string.Format(_localizationService.GetResource("Messages.Order.GiftCardInfo", language.Id), WebUtility.HtmlEncode(giftCard.GiftCardCouponCode));
                string giftCardAmount = _priceFormatter.FormatPrice(-gcuh.UsedValue, true, currency, language, true, false);
                cards.Add(giftCardText, giftCardAmount);
            }
            liquidOrder.GiftCards = cards;
            if (request.Order.RedeemedRewardPointsEntry != null)
            {
                liquidOrder.RPTitle = string.Format(_localizationService.GetResource("Messages.Order.RewardPoints", language.Id), -request.Order.RedeemedRewardPointsEntry?.Points);
                liquidOrder.RPAmount = _priceFormatter.FormatPrice(-request.Order.RedeemedRewardPointsEntry.UsedAmount, true, currency, language, true, false);
            }
            void CalculateSubTotals()
            {
                string _cusSubTotal;
                bool _displaySubTotalDiscount;
                string _cusSubTotalDiscount;
                string _cusShipTotal;
                string _cusPaymentMethodAdditionalFee;
                bool _displayTax;
                string _cusTaxTotal;
                bool _displayTaxRates;
                bool _displayDiscount;
                string _cusDiscount;
                string _cusTotal;

                _displaySubTotalDiscount = false;
                _cusSubTotalDiscount = string.Empty;
                if (request.Order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
                {
                    //including tax

                    //subtotal
                    _cusSubTotal = _priceFormatter.FormatPrice(request.Order.OrderSubtotalInclTax, true, currency, language, true);
                    //discount (applied to order subtotal)
                    if (request.Order.OrderSubTotalDiscountInclTax > decimal.Zero)
                    {
                        _cusSubTotalDiscount = _priceFormatter.FormatPrice(-request.Order.OrderSubTotalDiscountInclTax, true, currency, language, true);
                        _displaySubTotalDiscount = true;
                    }
                }
                else
                {
                    //exсluding tax

                    //subtotal
                    _cusSubTotal = _priceFormatter.FormatPrice(request.Order.OrderSubtotalExclTax, true, currency, language, false);
                    //discount (applied to order subtotal)
                    if (request.Order.OrderSubTotalDiscountExclTax > decimal.Zero)
                    {
                        _cusSubTotalDiscount = _priceFormatter.FormatPrice(-request.Order.OrderSubTotalDiscountExclTax, true, currency, language, false);
                        _displaySubTotalDiscount = true;
                    }
                }

                //shipping, payment method fee
                _cusTaxTotal = string.Empty;
                _cusDiscount = string.Empty;

                if (request.Order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax

                    //shipping
                    _cusShipTotal = _priceFormatter.FormatShippingPrice(request.Order.OrderShippingInclTax, true, currency, language, true);
                    //payment method additional fee
                    _cusPaymentMethodAdditionalFee = _priceFormatter.FormatPaymentMethodAdditionalFee(request.Order.PaymentMethodAdditionalFeeInclTax, true, currency, language, true);
                }
                else
                {
                    //excluding tax

                    //shipping
                    _cusShipTotal = _priceFormatter.FormatShippingPrice(request.Order.OrderShippingExclTax, true, currency, language, false);
                    //payment method additional fee
                    _cusPaymentMethodAdditionalFee = _priceFormatter.FormatPaymentMethodAdditionalFee(request.Order.PaymentMethodAdditionalFeeExclTax, true, currency, language, false);
                }

                //shipping
                bool displayShipping = request.Order.ShippingStatus != ShippingStatus.ShippingNotRequired;

                //payment method fee
                bool displayPaymentMethodFee = request.Order.PaymentMethodAdditionalFeeExclTax > decimal.Zero;

                //tax
                _displayTax = true;
                _displayTaxRates = true;
                if (_taxSettings.HideTaxInOrderSummary && request.Order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    _displayTax = false;
                    _displayTaxRates = false;
                }
                else
                {
                    if (request.Order.OrderTax == 0 && _taxSettings.HideZeroTax)
                    {
                        _displayTax = false;
                        _displayTaxRates = false;
                    }
                    else
                    {
                        _displayTaxRates = _taxSettings.DisplayTaxRates && request.Order.OrderTaxes.Any();
                        _displayTax = !_displayTaxRates;

                        string taxStr = _priceFormatter.FormatPrice(request.Order.OrderTax, true, currency, language, request.Order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax, false);
                        _cusTaxTotal = taxStr;
                    }
                }

                //discount
                _displayDiscount = false;
                if (request.Order.OrderDiscount > decimal.Zero)
                {
                    _cusDiscount = _priceFormatter.FormatPrice(-request.Order.OrderDiscount, true, currency, language, request.Order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax, false);
                    _displayDiscount = true;
                }

                //total
                _cusTotal = _priceFormatter.FormatPrice(request.Order.OrderTotal, true, currency, language, request.Order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax, false);


                liquidOrder.SubTotal = _cusSubTotal;
                liquidOrder.DisplaySubTotalDiscount = _displaySubTotalDiscount;
                liquidOrder.SubTotalDiscount = _cusSubTotalDiscount;
                liquidOrder.Shipping = _cusShipTotal;
                liquidOrder.Discount = _cusDiscount;
                liquidOrder.PaymentMethodAdditionalFee = _cusPaymentMethodAdditionalFee;
                liquidOrder.Tax = _cusTaxTotal;
                liquidOrder.Total = _cusTotal;
                liquidOrder.DisplayTax = _displayTax;
                liquidOrder.DisplayDiscount = _displayDiscount;
                liquidOrder.DisplayTaxRates = _displayTaxRates;

            }

            CalculateSubTotals();

            return liquidOrder;
        }
    }
}
