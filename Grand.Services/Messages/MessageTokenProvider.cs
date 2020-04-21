using Grand.Core.Domain;
using Grand.Core.Domain.Blogs;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Forums;
using Grand.Core.Domain.Knowledgebase;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Messages;
using Grand.Core.Domain.News;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Shipping;
using Grand.Core.Domain.Stores;
using Grand.Core.Domain.Tax;
using Grand.Core.Domain.Vendors;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Messages.DotLiquidDrops;
using Grand.Services.Orders;
using Grand.Services.Payments;
using Grand.Services.Vendors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Services.Messages
{
    public partial class MessageTokenProvider : IMessageTokenProvider
    {
        #region Fields

        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICurrencyService _currencyService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICountryService _countryService;
        private readonly MessageTemplatesSettings _templatesSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly TaxSettings _taxSettings;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly IMediator _mediator;
        private readonly IServiceProvider _serviceProvider;
        #endregion

        #region Ctor

        public MessageTokenProvider(ILanguageService languageService,
            ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper,
            IPriceFormatter priceFormatter,
            ICurrencyService currencyService,
            IProductAttributeParser productAttributeParser,
            IAddressAttributeFormatter addressAttributeFormatter,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            MessageTemplatesSettings templatesSettings,
            CatalogSettings catalogSettings,
            TaxSettings taxSettings,
            StoreInformationSettings storeInformationSettings,
            IMediator mediator,
            IServiceProvider serviceProvider)
        {
            _languageService = languageService;
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;
            _priceFormatter = priceFormatter;
            _currencyService = currencyService;
            _productAttributeParser = productAttributeParser;
            _addressAttributeFormatter = addressAttributeFormatter;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _templatesSettings = templatesSettings;
            _catalogSettings = catalogSettings;
            _taxSettings = taxSettings;
            _storeInformationSettings = storeInformationSettings;
            _mediator = mediator;
            _serviceProvider = serviceProvider;
        }

        #endregion


        #region Methods

        /// <summary>
        /// Gets list of allowed (supported) message tokens for campaigns
        /// </summary>
        /// <returns>List of allowed (supported) message tokens for campaigns</returns>
        public virtual string[] GetListOfCampaignAllowedTokens()
        {
            var allowedTokens = LiquidExtensions.GetTokens(typeof(LiquidStore),
                typeof(LiquidNewsLetterSubscription),
                typeof(LiquidShoppingCart),
                typeof(LiquidCustomer));

            return allowedTokens.ToArray();
        }

        public virtual string[] GetListOfAllowedTokens()
        {
            var allowedTokens = LiquidExtensions.GetTokens(
                typeof(LiquidAskQuestion),
                typeof(LiquidAttributeCombination),
                typeof(LiquidAuctions),
                typeof(LiquidBackInStockSubscription),
                typeof(LiquidBlogComment),
                typeof(LiquidContactUs),
                typeof(LiquidCustomer),
                typeof(LiquidEmailAFriend),
                typeof(LiquidForums),
                typeof(LiquidGiftCard),
                typeof(LiquidKnowledgebase),
                typeof(LiquidNewsComment),
                typeof(LiquidNewsLetterSubscription),
                typeof(LiquidOrder),
                typeof(LiquidOrderItem),
                typeof(LiquidPrivateMessage),
                typeof(LiquidProduct),
                typeof(LiquidProductReview),
                typeof(RecurringPayment),
                typeof(LiquidReturnRequest),
                typeof(LiquidShipment),
                typeof(LiquidShipmentItem),
                typeof(LiquidShoppingCart),
                typeof(LiquidStore),
                typeof(LiquidVatValidationResult),
                typeof(LiquidVendor),
                typeof(LiquidVendorReview));

            return allowedTokens.ToArray();
        }

        public virtual string[] GetListOfCustomerReminderAllowedTokens(CustomerReminderRuleEnum rule)
        {
            var allowedTokens = LiquidExtensions.GetTokens(typeof(LiquidStore));

            if (rule == CustomerReminderRuleEnum.AbandonedCart)
            {
                allowedTokens.AddRange(LiquidExtensions.GetTokens(typeof(LiquidShoppingCart)));
            }

            if (rule == CustomerReminderRuleEnum.CompletedOrder || rule == CustomerReminderRuleEnum.UnpaidOrder)
            {
                allowedTokens.AddRange(LiquidExtensions.GetTokens(typeof(LiquidOrder)));
            }

            allowedTokens.AddRange(LiquidExtensions.GetTokens(typeof(LiquidCustomer)));

            return allowedTokens.ToArray();
        }

        public async Task AddStoreTokens(LiquidObject liquidObject, Store store, Language language, EmailAccount emailAccount)
        {
            var liquidStore = new LiquidStore(store, language, emailAccount);
            liquidStore.TwitterLink = _storeInformationSettings.TwitterLink;
            liquidStore.FacebookLink = _storeInformationSettings.FacebookLink;
            liquidStore.YoutubeLink = _storeInformationSettings.YoutubeLink;
            liquidStore.InstagramLink = _storeInformationSettings.InstagramLink;
            liquidStore.LinkedInLink = _storeInformationSettings.LinkedInLink;
            liquidStore.PinterestLink = _storeInformationSettings.PinterestLink;

            liquidObject.Store = liquidStore;

            await _mediator.EntityTokensAdded(store, liquidStore, liquidObject);
        }

        public async Task AddOrderTokens(LiquidObject liquidObject, Order order, Customer customer, Store store, OrderNote orderNote = null, Vendor vendor = null, decimal refundedAmount = 0)
        {
            var language = await _languageService.GetLanguageById(order.CustomerLanguageId);
            var currency = await _currencyService.GetCurrencyByCode(order.CustomerCurrencyCode);
            var productService = _serviceProvider.GetRequiredService<IProductService>();
            var downloadService = _serviceProvider.GetRequiredService<IDownloadService>();
            var vendorService = _serviceProvider.GetRequiredService<IVendorService>();

            var liquidOrder = new LiquidOrder(order, customer, language, currency, store, orderNote, vendor);
            foreach (var item in order.OrderItems.Where(x => x.VendorId == vendor?.Id || vendor == null))
            {
                var product = await productService.GetProductById(item.ProductId);
                Vendor vendorItem = string.IsNullOrEmpty(item.VendorId) ? null : await vendorService.GetVendorById(item.VendorId);
                var liqitem = new LiquidOrderItem(item, product, order, language, currency, store, vendorItem);

                #region Download

                liqitem.IsDownloadAllowed = downloadService.IsDownloadAllowed(order, item, product);
                liqitem.IsLicenseDownloadAllowed = downloadService.IsLicenseDownloadAllowed(order, item, product);

                #endregion

                #region Unit price
                string unitPriceStr;
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(item.UnitPriceInclTax, order.CurrencyRate);
                    unitPriceStr = _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency, true, currency, language, true);
                }
                else
                {
                    //excluding tax
                    var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(item.UnitPriceExclTax, order.CurrencyRate);
                    unitPriceStr = _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency, true, currency, language, false);
                }
                liqitem.UnitPrice = unitPriceStr;

                #endregion

                #region total price
                string priceStr;
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var priceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(item.PriceInclTax, order.CurrencyRate);
                    priceStr = _priceFormatter.FormatPrice(priceInclTaxInCustomerCurrency, true, currency, language, true);
                }
                else
                {
                    //excluding tax
                    var priceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(item.PriceExclTax, order.CurrencyRate);
                    priceStr = _priceFormatter.FormatPrice(priceExclTaxInCustomerCurrency, true, currency, language, false);
                }
                liqitem.TotalPrice = priceStr;

                #endregion

                string sku = "";
                if (product != null)
                    sku = product.FormatSku(item.AttributesXml, _productAttributeParser);

                liqitem.ProductSku = WebUtility.HtmlEncode(sku);
                liqitem.ShowSkuOnProductDetailsPage = _catalogSettings.ShowSkuOnProductDetailsPage;
                liqitem.ProductOldPrice = _priceFormatter.FormatPrice(product.OldPrice, true, currency, language, true);

                liquidOrder.OrderItems.Add(liqitem);
            }

            liquidOrder.BillingCustomAttributes = await _addressAttributeFormatter.FormatAttributes(order.BillingAddress?.CustomAttributes);
            liquidOrder.BillingCountry = order.BillingAddress != null && !string.IsNullOrEmpty(order.BillingAddress.CountryId) ? (await _countryService.GetCountryById(order.BillingAddress.CountryId))?.GetLocalized(x => x.Name, order.CustomerLanguageId) : "";
            liquidOrder.BillingStateProvince = !string.IsNullOrEmpty(order.BillingAddress.StateProvinceId) ? (await _stateProvinceService.GetStateProvinceById(order.BillingAddress.StateProvinceId))?.GetLocalized(x => x.Name, order.CustomerLanguageId) : "";

            liquidOrder.ShippingCountry = order.ShippingAddress != null && !string.IsNullOrEmpty(order.ShippingAddress.CountryId) ? (await _countryService.GetCountryById(order.ShippingAddress.CountryId))?.GetLocalized(x => x.Name, order.CustomerLanguageId) : "";
            liquidOrder.ShippingStateProvince = order.ShippingAddress != null && !string.IsNullOrEmpty(order.ShippingAddress.StateProvinceId) ? (await _stateProvinceService.GetStateProvinceById(order.ShippingAddress.StateProvinceId)).GetLocalized(x => x.Name, order.CustomerLanguageId) : "";
            liquidOrder.ShippingCustomAttributes = await _addressAttributeFormatter.FormatAttributes(order.ShippingAddress != null ? order.ShippingAddress.CustomAttributes : "");

            var paymentMethod = _serviceProvider.GetRequiredService<IPaymentService>().LoadPaymentMethodBySystemName(order.PaymentMethodSystemName);
            liquidOrder.PaymentMethod = paymentMethod != null ? paymentMethod.GetLocalizedFriendlyName(_localizationService, language.Id) : order.PaymentMethodSystemName;
            liquidOrder.AmountRefunded = _priceFormatter.FormatPrice(refundedAmount, true, currency, language, false);

            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var item in order.TaxRatesDictionary)
            {
                string taxRate = string.Format(_localizationService.GetResource("Messages.Order.TaxRateLine"), _priceFormatter.FormatTaxRate(item.Key));
                string taxValue = _priceFormatter.FormatPrice(item.Value, true, currency, language, false);
                dict.Add(taxRate, taxValue);
            }
            liquidOrder.TaxRates = dict;

            Dictionary<string, string> cards = new Dictionary<string, string>();
            var servicegiftCard = _serviceProvider.GetRequiredService<IGiftCardService>();
            var gcuhC = await servicegiftCard.GetAllGiftCardUsageHistory(order.Id);
            foreach (var gcuh in gcuhC)
            {
                var giftCard = await servicegiftCard.GetGiftCardById(gcuh.GiftCardId);
                string giftCardText = string.Format(_localizationService.GetResource("Messages.Order.GiftCardInfo", language.Id), WebUtility.HtmlEncode(giftCard.GiftCardCouponCode));
                string giftCardAmount = _priceFormatter.FormatPrice(-(_currencyService.ConvertCurrency(gcuh.UsedValue, order.CurrencyRate)), true, currency, language, false);
                cards.Add(giftCardText, giftCardAmount);
            }
            liquidOrder.GiftCards = cards;
            if (order.RedeemedRewardPointsEntry != null)
            {
                liquidOrder.RPTitle = string.Format(_localizationService.GetResource("Messages.Order.RewardPoints", language.Id), -order.RedeemedRewardPointsEntry?.Points);
                liquidOrder.RPAmount = _priceFormatter.FormatPrice(-(_currencyService.ConvertCurrency(order.RedeemedRewardPointsEntry.UsedAmount, order.CurrencyRate)), true, currency, language, false);
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
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
                {
                    //including tax

                    //subtotal
                    var orderSubtotalInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalInclTax, order.CurrencyRate);
                    _cusSubTotal = _priceFormatter.FormatPrice(orderSubtotalInclTaxInCustomerCurrency, true, currency, language, true);
                    //discount (applied to order subtotal)
                    var orderSubTotalDiscountInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountInclTax, order.CurrencyRate);
                    if (orderSubTotalDiscountInclTaxInCustomerCurrency > decimal.Zero)
                    {
                        _cusSubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountInclTaxInCustomerCurrency, true, currency, language, true);
                        _displaySubTotalDiscount = true;
                    }
                }
                else
                {
                    //exсluding tax

                    //subtotal
                    var orderSubtotalExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalExclTax, order.CurrencyRate);
                    _cusSubTotal = _priceFormatter.FormatPrice(orderSubtotalExclTaxInCustomerCurrency, true, currency, language, false);
                    //discount (applied to order subtotal)
                    var orderSubTotalDiscountExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountExclTax, order.CurrencyRate);
                    if (orderSubTotalDiscountExclTaxInCustomerCurrency > decimal.Zero)
                    {
                        _cusSubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountExclTaxInCustomerCurrency, true, currency, language, false);
                        _displaySubTotalDiscount = true;
                    }
                }

                //shipping, payment method fee
                _cusTaxTotal = string.Empty;
                _cusDiscount = string.Empty;

                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax

                    //shipping
                    var orderShippingInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                    _cusShipTotal = _priceFormatter.FormatShippingPrice(orderShippingInclTaxInCustomerCurrency, true, currency, language, true);
                    //payment method additional fee
                    var paymentMethodAdditionalFeeInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate);
                    _cusPaymentMethodAdditionalFee = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeInclTaxInCustomerCurrency, true, currency, language, true);
                }
                else
                {
                    //excluding tax

                    //shipping
                    var orderShippingExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate);
                    _cusShipTotal = _priceFormatter.FormatShippingPrice(orderShippingExclTaxInCustomerCurrency, true, currency, language, false);
                    //payment method additional fee
                    var paymentMethodAdditionalFeeExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate);
                    _cusPaymentMethodAdditionalFee = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeExclTaxInCustomerCurrency, true, currency, language, false);
                }

                //shipping
                bool displayShipping = order.ShippingStatus != ShippingStatus.ShippingNotRequired;

                //payment method fee
                bool displayPaymentMethodFee = order.PaymentMethodAdditionalFeeExclTax > decimal.Zero;

                //tax
                _displayTax = true;
                _displayTaxRates = true;
                if (_taxSettings.HideTaxInOrderSummary && order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    _displayTax = false;
                    _displayTaxRates = false;
                }
                else
                {
                    if (order.OrderTax == 0 && _taxSettings.HideZeroTax)
                    {
                        _displayTax = false;
                        _displayTaxRates = false;
                    }
                    else
                    {
                        var _taxRates = new SortedDictionary<decimal, decimal>();
                        foreach (var tr in order.TaxRatesDictionary)
                            _taxRates.Add(tr.Key, _currencyService.ConvertCurrency(tr.Value, order.CurrencyRate));

                        _displayTaxRates = _taxSettings.DisplayTaxRates && _taxRates.Any();
                        _displayTax = !_displayTaxRates;

                        var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate);
                        string taxStr = _priceFormatter.FormatPrice(orderTaxInCustomerCurrency, true, currency, language, order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax, false);
                        _cusTaxTotal = taxStr;
                    }
                }

                //discount
                _displayDiscount = false;
                if (order.OrderDiscount > decimal.Zero)
                {
                    var orderDiscountInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);
                    _cusDiscount = _priceFormatter.FormatPrice(-orderDiscountInCustomerCurrency, true, currency, language, order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax, false);
                    _displayDiscount = true;
                }

                //total
                var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
                _cusTotal = _priceFormatter.FormatPrice(orderTotalInCustomerCurrency, true, currency, language, order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax, false);


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

            liquidObject.Order = liquidOrder;

            await _mediator.EntityTokensAdded(order, liquidOrder, liquidObject);
        }

        public async Task AddShipmentTokens(LiquidObject liquidObject, Shipment shipment, Order order, Store store, Language language)
        {
            var liquidShipment = new LiquidShipment(shipment, order, store, language);
            var productService = _serviceProvider.GetRequiredService<IProductService>();
            foreach (var shipmentItem in shipment.ShipmentItems)
            {
                var orderitem = order.OrderItems.FirstOrDefault(x => x.Id == shipmentItem.OrderItemId);
                var product = await productService.GetProductById(shipmentItem.ProductId);
                var liquidshipmentItems = new LiquidShipmentItem(shipmentItem, shipment, order, orderitem, product, language);
                liquidshipmentItems.ShowSkuOnProductDetailsPage = _catalogSettings.ShowSkuOnProductDetailsPage;
                string sku = "";
                if (product != null)
                    sku = product.FormatSku(orderitem.AttributesXml, _productAttributeParser);

                liquidshipmentItems.ProductSku = WebUtility.HtmlEncode(sku);

                liquidShipment.ShipmentItems.Add(liquidshipmentItems);
            }
            liquidObject.Shipment = liquidShipment;
            await _mediator.EntityTokensAdded(shipment, liquidShipment, liquidObject);
        }

        public async Task AddRecurringPaymentTokens(LiquidObject liquidObject, RecurringPayment recurringPayment)
        {
            var liquidRecurringPayment = new LiquidRecurringPayment(recurringPayment);
            liquidObject.RecurringPayment = liquidRecurringPayment;

            await _mediator.EntityTokensAdded(recurringPayment, liquidRecurringPayment, liquidObject);
        }

        public async Task AddReturnRequestTokens(LiquidObject liquidObject, ReturnRequest returnRequest, Store store, Order order, Language language, ReturnRequestNote returnRequestNote = null)
        {
            var liquidReturnRequest = new LiquidReturnRequest(returnRequest, store, order, returnRequestNote);

            liquidReturnRequest.Status = returnRequest.ReturnRequestStatus.GetLocalizedEnum(_localizationService, language.Id);
            liquidReturnRequest.Products = await ProductListToHtmlTable();
            liquidReturnRequest.PickupAddressStateProvince =
                            !string.IsNullOrEmpty(returnRequest.PickupAddress.StateProvinceId) ?
                            (await _stateProvinceService.GetStateProvinceById(returnRequest.PickupAddress.StateProvinceId))?.GetLocalized(x => x.Name, language.Id) : "";

            liquidReturnRequest.PickupAddressCountry =
                            !string.IsNullOrEmpty(returnRequest.PickupAddress.CountryId) ?
                            (await _countryService.GetCountryById(returnRequest.PickupAddress.CountryId))?.GetLocalized(x => x.Name, language.Id) : "";

            async Task<string> ProductListToHtmlTable()
            {
                var sb = new StringBuilder();
                sb.AppendLine("<table border=\"0\" style=\"width:100%;\">");

                sb.AppendLine(string.Format("<tr style=\"text-align:center;\">"));
                sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Name")));
                sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Price")));
                sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Quantity")));
                sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).ReturnReason")));
                sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).ReturnAction")));
                sb.AppendLine("</tr>");

                IProductService _productService = _serviceProvider.GetRequiredService<IProductService>();
                var currency = await _currencyService.GetCurrencyByCode(order.CustomerCurrencyCode);
                foreach (var rrItem in returnRequest.ReturnRequestItems)
                {
                    var orderItem = order.OrderItems.Where(x => x.Id == rrItem.OrderItemId).First();

                    sb.AppendLine(string.Format("<tr style=\"background-color: {0};text-align: center;\">", _templatesSettings.Color2));
                    string productName = (await _productService.GetProductById(orderItem.ProductId))?.GetLocalized(x => x.Name, order.CustomerLanguageId);

                    sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + WebUtility.HtmlEncode(productName));

                    sb.AppendLine("</td>");

                    string unitPriceStr;
                    if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                    {
                        //including tax
                        var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                        unitPriceStr = _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency, true, currency, language, true);
                    }
                    else
                    {
                        //excluding tax
                        var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                        unitPriceStr = _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency, true, currency, language, false);
                    }
                    sb.AppendLine(string.Format("<td style=\"padding: 0.6em 0.4em;text-align: right;\">{0}</td>", unitPriceStr));
                    sb.AppendLine(string.Format("<td style=\"padding: 0.6em 0.4em;text-align: center;\">{0}</td>", orderItem.Quantity));
                    sb.AppendLine(string.Format("<td style=\"padding: 0.6em 0.4em;text-align: center;\">{0}</td>", rrItem.ReasonForReturn));
                    sb.AppendLine(string.Format("<td style=\"padding: 0.6em 0.4em;text-align: center;\">{0}</td>", rrItem.RequestedAction));
                }

                sb.AppendLine("</table>");
                return sb.ToString();
            }

            liquidObject.ReturnRequest = liquidReturnRequest;

            await _mediator.EntityTokensAdded(returnRequest, liquidReturnRequest, liquidObject);
        }

        public async Task AddGiftCardTokens(LiquidObject liquidObject, GiftCard giftCard)
        {
            var liquidGiftCart = new LiquidGiftCard(giftCard);
            liquidGiftCart.Amount = _priceFormatter.FormatPrice(giftCard.Amount, true, false);
            liquidObject.GiftCard = liquidGiftCart;

            await _mediator.EntityTokensAdded(giftCard, liquidGiftCart, liquidObject);
        }

        public async Task AddCustomerTokens(LiquidObject liquidObject, Customer customer, Store store, Language language, CustomerNote customerNote = null)
        {
            var liquidCustomer = new LiquidCustomer(customer, store, language, customerNote);
            liquidObject.Customer = liquidCustomer;

            await _mediator.EntityTokensAdded(customer, liquidCustomer, liquidObject);
            await _mediator.EntityTokensAdded(customerNote, liquidCustomer, liquidObject);
        }

        public async Task AddShoppingCartTokens(LiquidObject liquidObject, Customer customer, Store store, Language language,
            string personalMessage = "", string customerEmail = "")
        {
            var liquidShoppingCart = new LiquidShoppingCart(customer, store, language, personalMessage, customerEmail);
            liquidShoppingCart.ShoppingCartProducts = await ShoppingCartWishListProductListToHtmlTable(true, false);
            liquidShoppingCart.ShoppingCartProductsWithPictures = await ShoppingCartWishListProductListToHtmlTable(true, true);
            liquidShoppingCart.WishlistProducts = await ShoppingCartWishListProductListToHtmlTable(false, false);
            liquidShoppingCart.WishlistProductsWithPictures = await ShoppingCartWishListProductListToHtmlTable(false, true);

            async Task<string> ShoppingCartWishListProductListToHtmlTable(bool cart, bool withPicture)
            {
                string result;

                var sb = new StringBuilder();
                sb.AppendLine("<table border=\"0\" style=\"width:100%;\">");

                #region Products
                sb.AppendLine(string.Format("<tr style=\"background-color:{0};text-align:center;\">", _templatesSettings.Color1));
                if (withPicture)
                    sb.AppendLine(string.Format("<th>{0}</th>", cart ? _localizationService.GetResource("Messages.ShoppingCart.Product(s).Picture", language.Id) : _localizationService.GetResource("Messages.Wishlist.Product(s).Picture", language.Id)));
                sb.AppendLine(string.Format("<th>{0}</th>", cart ? _localizationService.GetResource("Messages.ShoppingCart.Product(s).Name", language.Id) : _localizationService.GetResource("Messages.Wishlist.Product(s).Name", language.Id)));
                sb.AppendLine(string.Format("<th>{0}</th>", cart ? _localizationService.GetResource("Messages.ShoppingCart.Product(s).Quantity", language.Id) : _localizationService.GetResource("Messages.Wishlist.Product(s).Quantity", language.Id)));
                sb.AppendLine("</tr>");
                var productService = _serviceProvider.GetRequiredService<IProductService>();
                var productAttributeFormatter = _serviceProvider.GetRequiredService<IProductAttributeFormatter>();
                var pictureService = _serviceProvider.GetRequiredService<IPictureService>();

                foreach (var item in cart ? customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart) :
                    customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.Wishlist))
                {
                    var product = await productService.GetProductById(item.ProductId);
                    sb.AppendLine(string.Format("<tr style=\"background-color: {0};text-align: center;\">", _templatesSettings.Color2));
                    //product name
                    string productName = product.GetLocalized(x => x.Name, language.Id);
                    if (withPicture)
                    {
                        string pictureUrl = "";
                        if (product.ProductPictures.Any())
                        {
                            pictureUrl = await pictureService.GetPictureUrl(product.ProductPictures.OrderBy(x => x.DisplayOrder).FirstOrDefault().PictureId, _templatesSettings.PictureSize, storeLocation: store.SslEnabled ? store.SecureUrl : store.Url);
                        }
                        sb.Append(string.Format("<td><img src=\"{0}\" alt=\"\"/></td>", pictureUrl));
                    }
                    sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + WebUtility.HtmlEncode(productName));
                    //attributes
                    if (!string.IsNullOrEmpty(item.AttributesXml))
                    {
                        sb.AppendLine("<br />");
                        string attributeDescription = await productAttributeFormatter.FormatAttributes(product, item.AttributesXml, customer);
                        sb.AppendLine(attributeDescription);
                    }
                    sb.AppendLine("</td>");

                    sb.AppendLine(string.Format("<td style=\"padding: 0.6em 0.4em;text-align: center;\">{0}</td>", item.Quantity));

                    sb.AppendLine("</tr>");
                }
                #endregion

                sb.AppendLine("</table>");
                result = sb.ToString();
                return result;
            }

            liquidObject.ShoppingCart = liquidShoppingCart;

            await _mediator.EntityTokensAdded(customer, liquidShoppingCart, liquidObject);
        }

        public async Task AddVendorTokens(LiquidObject liquidObject, Vendor vendor, Language language)
        {
            var liquidVendor = new LiquidVendor(vendor);
            liquidVendor.StateProvince = !string.IsNullOrEmpty(vendor.Address?.StateProvinceId) ?
                (await _stateProvinceService.GetStateProvinceById(vendor.Address.StateProvinceId))?
                .GetLocalized(x => x.Name, language.Id) : "";
            liquidVendor.Country = !string.IsNullOrEmpty(vendor.Address?.CountryId) ?
                (await _countryService.GetCountryById(vendor.Address.CountryId))?
                .GetLocalized(x => x.Name, language.Id) : "";

            liquidObject.Vendor = liquidVendor;

            await _mediator.EntityTokensAdded(vendor, liquidVendor, liquidObject);
        }

        public async Task AddNewsLetterSubscriptionTokens(LiquidObject liquidObject, NewsLetterSubscription subscription, Store store)
        {
            var liquidNewsletterSubscription = new LiquidNewsLetterSubscription(subscription, store);
            liquidObject.NewsLetterSubscription = liquidNewsletterSubscription;

            await _mediator.EntityTokensAdded(subscription, liquidNewsletterSubscription, liquidObject);
        }

        public async Task AddProductReviewTokens(LiquidObject liquidObject, Product product, ProductReview productReview)
        {
            var liquidProductReview = new LiquidProductReview(product, productReview);
            liquidObject.ProductReview = liquidProductReview;

            await _mediator.EntityTokensAdded(productReview, liquidProductReview, liquidObject);
        }

        public async Task AddVendorReviewTokens(LiquidObject liquidObject, Vendor vendor, VendorReview vendorReview)
        {
            var liquidVendorReview = new LiquidVendorReview(vendor, vendorReview);
            liquidObject.VendorReview = liquidVendorReview;

            await _mediator.EntityTokensAdded(vendorReview, liquidVendorReview, liquidObject);
        }

        public async Task AddBlogCommentTokens(LiquidObject liquidObject, BlogPost blogPost, BlogComment blogComment, Store store, Language language)
        {
            var liquidBlogComment = new LiquidBlogComment(blogComment, blogPost, store, language);
            liquidObject.BlogComment = liquidBlogComment;

            await _mediator.EntityTokensAdded(blogComment, liquidBlogComment, liquidObject);
        }

        public async Task AddArticleCommentTokens(LiquidObject liquidObject, KnowledgebaseArticle article, KnowledgebaseArticleComment articleComment, Store store, Language language)
        {
            var liquidKnowledgebase = new LiquidKnowledgebase(article, articleComment, store, language);
            liquidObject.Knowledgebase = liquidKnowledgebase;

            await _mediator.EntityTokensAdded(articleComment, liquidKnowledgebase, liquidObject);
        }

        public async Task AddNewsCommentTokens(LiquidObject liquidObject, NewsItem newsItem, NewsComment newsComment, Store store, Language language)
        {
            var liquidNewsComment = new LiquidNewsComment(newsItem, newsComment, store, language);
            liquidObject.NewsComment = liquidNewsComment;

            await _mediator.EntityTokensAdded(newsComment, liquidNewsComment, liquidObject);
        }

        public async Task AddProductTokens(LiquidObject liquidObject, Product product, Language language, Store store)
        {
            var liquidProduct = new LiquidProduct(product, language, store);
            liquidObject.Product = liquidProduct;

            await _mediator.EntityTokensAdded(product, liquidProduct, liquidObject);
        }

        public async Task AddAttributeCombinationTokens(LiquidObject liquidObject, Product product, ProductAttributeCombination combination)
        {
            var liquidAttributeCombination = new LiquidAttributeCombination(combination);
            var productAttributeFormatter = _serviceProvider.GetRequiredService<IProductAttributeFormatter>();
            liquidAttributeCombination.Formatted = await productAttributeFormatter.FormatAttributes(product, combination.AttributesXml, null, renderPrices: false);
            liquidAttributeCombination.SKU = product.FormatSku(combination.AttributesXml, _productAttributeParser);
            liquidObject.AttributeCombination = liquidAttributeCombination;

            await _mediator.EntityTokensAdded(combination, liquidAttributeCombination, liquidObject);
        }

        public async Task AddForumTokens(LiquidObject liquidObject, Customer customer, Store store, Forum forum, ForumTopic forumTopic = null, ForumPost forumPost = null,
            int? friendlyForumTopicPageIndex = null, string appendedPostIdentifierAnchor = "")
        {
            var liquidForum = new LiquidForums(forum, forumTopic, forumPost, customer, store, friendlyForumTopicPageIndex, appendedPostIdentifierAnchor);
            liquidObject.Forums = liquidForum;

            await _mediator.EntityTokensAdded(forum, liquidForum, liquidObject);
            await _mediator.EntityTokensAdded(forumTopic, liquidForum, liquidObject);
            await _mediator.EntityTokensAdded(forumPost, liquidForum, liquidObject);
        }

        public async Task AddPrivateMessageTokens(LiquidObject liquidObject, PrivateMessage privateMessage)
        {
            var liquidPrivateMessage = new LiquidPrivateMessage(privateMessage);
            liquidObject.PrivateMessage = liquidPrivateMessage;

            await _mediator.EntityTokensAdded(privateMessage, liquidPrivateMessage, liquidObject);
        }

        public async Task AddBackInStockTokens(LiquidObject liquidObject, Product product, BackInStockSubscription subscription, Store store, Language language)
        {
            var liquidBackInStockSubscription = new LiquidBackInStockSubscription(product, subscription, store, language);
            liquidObject.BackInStockSubscription = liquidBackInStockSubscription;

            await _mediator.EntityTokensAdded(subscription, liquidBackInStockSubscription, liquidObject);
        }

        public async Task AddAuctionTokens(LiquidObject liquidObject, Product product, Bid bid)
        {
            var liquidAuctions = new LiquidAuctions(product, bid);
            var defaultCurrency = await _currencyService.GetPrimaryStoreCurrency();
            liquidAuctions.Price = _priceFormatter.FormatPrice(bid.Amount, true, defaultCurrency);
            liquidAuctions.EndTime = _dateTimeHelper.ConvertToUserTime(product.AvailableEndDateTimeUtc.Value, DateTimeKind.Utc).ToString();

            liquidObject.Auctions = liquidAuctions;

            await _mediator.EntityTokensAdded(bid, liquidAuctions, liquidObject);
        }

        #endregion
    }
}