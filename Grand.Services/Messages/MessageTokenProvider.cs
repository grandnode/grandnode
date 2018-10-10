using Grand.Core;
using Grand.Core.Domain;
using Grand.Core.Domain.Blogs;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Forums;
using Grand.Core.Domain.Knowledgebase;
using Grand.Core.Domain.Media;
using Grand.Core.Domain.Messages;
using Grand.Core.Domain.News;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Shipping;
using Grand.Core.Domain.Stores;
using Grand.Core.Domain.Tax;
using Grand.Core.Domain.Vendors;
using Grand.Core.Html;
using Grand.Core.Infrastructure;
using Grand.Services.Blogs;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Events;
using Grand.Services.Forums;
using Grand.Services.Helpers;
using Grand.Services.Knowledgebase;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.News;
using Grand.Services.Orders;
using Grand.Services.Payments;
using Grand.Services.Seo;
using Grand.Services.Shipping;
using Grand.Services.Stores;
using Grand.Services.Vendors;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

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
        private readonly IWorkContext _workContext;
        private readonly IDownloadService _downloadService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly IStoreService _storeService;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerAttributeFormatter _customerAttributeFormatter;

        private readonly MessageTemplatesSettings _templatesSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly TaxSettings _taxSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly MediaSettings _mediaSettings;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        public MessageTokenProvider(ILanguageService languageService,
            ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper,
            IPriceFormatter priceFormatter,
            ICurrencyService currencyService,
            IWorkContext workContext,
            IDownloadService downloadService,
            IOrderService orderService,
            IPaymentService paymentService,
            IStoreService storeService,
            IStoreContext storeContext,
            IProductAttributeParser productAttributeParser,
            IAddressAttributeFormatter addressAttributeFormatter,
            ICustomerAttributeFormatter customerAttributeFormatter,
            MessageTemplatesSettings templatesSettings,
            CatalogSettings catalogSettings,
            TaxSettings taxSettings,
            CurrencySettings currencySettings,
            ShippingSettings shippingSettings,
            StoreInformationSettings storeInformationSettings,
            MediaSettings _mediaSettings,
            IEventPublisher eventPublisher)
        {
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._dateTimeHelper = dateTimeHelper;
            this._priceFormatter = priceFormatter;
            this._currencyService = currencyService;
            this._workContext = workContext;
            this._downloadService = downloadService;
            this._orderService = orderService;
            this._paymentService = paymentService;
            this._productAttributeParser = productAttributeParser;
            this._addressAttributeFormatter = addressAttributeFormatter;
            this._customerAttributeFormatter = customerAttributeFormatter;
            this._storeService = storeService;
            this._storeContext = storeContext;
            this._shippingSettings = shippingSettings;
            this._templatesSettings = templatesSettings;
            this._catalogSettings = catalogSettings;
            this._taxSettings = taxSettings;
            this._currencySettings = currencySettings;
            this._storeInformationSettings = storeInformationSettings;
            this._mediaSettings = _mediaSettings;
            this._eventPublisher = eventPublisher;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Convert a collection to a HTML table
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="vendorId">Vendor identifier (used to limit products by vendor</param>
        /// <returns>HTML table of products</returns>
        protected virtual string ProductListToHtmlTable(Order order, string languageId, string vendorId)
        {
            string result;

            var language = _languageService.GetLanguageById(languageId);

            var sb = new StringBuilder();
            sb.AppendLine("<table border=\"0\" style=\"width:100%;\">");

            #region Products
            sb.AppendLine(string.Format("<tr style=\"background-color:{0};text-align:center;\">", _templatesSettings.Color1));
            sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Name", languageId)));
            sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Price", languageId)));
            sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Quantity", languageId)));
            sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Total", languageId)));
            sb.AppendLine("</tr>");

            var table = order.OrderItems.ToList();
            var productService = EngineContext.Current.Resolve<IProductService>();
            for (int i = 0; i <= table.Count - 1; i++)
            {
                var orderItem = table[i];
                var product = productService.GetProductByIdIncludeArch(orderItem.ProductId);
                if (product == null)
                    continue;

                if (!String.IsNullOrEmpty(vendorId) && product.VendorId != vendorId)
                    continue;

                sb.AppendLine(string.Format("<tr style=\"background-color: {0};text-align: center;\">", _templatesSettings.Color2));
                //product name
                string productName = product.GetLocalized(x => x.Name, languageId);

                sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + WebUtility.HtmlEncode(productName));
                //add download link
                if (_downloadService.IsDownloadAllowed(orderItem))
                {
                    //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
                    string downloadUrl = string.Format("{0}download/getdownload/{1}", GetStoreUrl(order.StoreId), orderItem.OrderItemGuid);
                    string downloadLink = string.Format("<a class=\"link\" href=\"{0}\">{1}</a>", downloadUrl, _localizationService.GetResource("Messages.Order.Product(s).Download", languageId));
                    sb.AppendLine("<br />");
                    sb.AppendLine(downloadLink);
                }
                //add download link
                if (_downloadService.IsLicenseDownloadAllowed(orderItem))
                {
                    //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
                    string licenseUrl = string.Format("{0}download/getlicense/{1}", GetStoreUrl(order.StoreId), orderItem.OrderItemGuid);
                    string licenseLink = string.Format("<a class=\"link\" href=\"{0}\">{1}</a>", licenseUrl, _localizationService.GetResource("Messages.Order.Product(s).License", languageId));
                    sb.AppendLine("<br />");
                    sb.AppendLine(licenseLink);
                }
                //attributes
                if (!String.IsNullOrEmpty(orderItem.AttributeDescription))
                {
                    sb.AppendLine("<br />");
                    sb.AppendLine(orderItem.AttributeDescription);
                }
                //sku
                if (_catalogSettings.ShowSkuOnProductDetailsPage)
                {
                    var sku = product.FormatSku(orderItem.AttributesXml, _productAttributeParser);
                    if (!String.IsNullOrEmpty(sku))
                    {
                        sb.AppendLine("<br />");
                        sb.AppendLine(string.Format(_localizationService.GetResource("Messages.Order.Product(s).SKU", languageId), WebUtility.HtmlEncode(sku)));
                    }
                }
                sb.AppendLine("</td>");

                string unitPriceStr;
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                    unitPriceStr = _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, true);
                }
                else
                {
                    //excluding tax
                    var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                    unitPriceStr = _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, false);
                }
                sb.AppendLine(string.Format("<td style=\"padding: 0.6em 0.4em;text-align: right;\">{0}</td>", unitPriceStr));

                sb.AppendLine(string.Format("<td style=\"padding: 0.6em 0.4em;text-align: center;\">{0}</td>", orderItem.Quantity));

                string priceStr;
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var priceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceInclTax, order.CurrencyRate);
                    priceStr = _priceFormatter.FormatPrice(priceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, true);
                }
                else
                {
                    //excluding tax
                    var priceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceExclTax, order.CurrencyRate);
                    priceStr = _priceFormatter.FormatPrice(priceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, false);
                }
                sb.AppendLine(string.Format("<td style=\"padding: 0.6em 0.4em;text-align: right;\">{0}</td>", priceStr));

                sb.AppendLine("</tr>");
            }
            #endregion

            if (String.IsNullOrEmpty(vendorId))
            {
                //we render checkout attributes and totals only for store owners (hide for vendors)

                #region Checkout Attributes

                if (!String.IsNullOrEmpty(order.CheckoutAttributeDescription))
                {
                    sb.AppendLine("<tr><td style=\"text-align:right;\" colspan=\"1\">&nbsp;</td><td colspan=\"3\" style=\"text-align:right\">");
                    sb.AppendLine(order.CheckoutAttributeDescription);
                    sb.AppendLine("</td></tr>");
                }

                #endregion

                #region Totals

                //subtotal
                string cusSubTotal;
                bool displaySubTotalDiscount = false;
                string cusSubTotalDiscount = string.Empty;
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
                {
                    //including tax

                    //subtotal
                    var orderSubtotalInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalInclTax, order.CurrencyRate);
                    cusSubTotal = _priceFormatter.FormatPrice(orderSubtotalInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, true);
                    //discount (applied to order subtotal)
                    var orderSubTotalDiscountInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountInclTax, order.CurrencyRate);
                    if (orderSubTotalDiscountInclTaxInCustomerCurrency > decimal.Zero)
                    {
                        cusSubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, true);
                        displaySubTotalDiscount = true;
                    }
                }
                else
                {
                    //exсluding tax

                    //subtotal
                    var orderSubtotalExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalExclTax, order.CurrencyRate);
                    cusSubTotal = _priceFormatter.FormatPrice(orderSubtotalExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, false);
                    //discount (applied to order subtotal)
                    var orderSubTotalDiscountExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountExclTax, order.CurrencyRate);
                    if (orderSubTotalDiscountExclTaxInCustomerCurrency > decimal.Zero)
                    {
                        cusSubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, false);
                        displaySubTotalDiscount = true;
                    }
                }

                //shipping, payment method fee
                string cusShipTotal;
                string cusPaymentMethodAdditionalFee;
                var taxRates = new SortedDictionary<decimal, decimal>();
                string cusTaxTotal = string.Empty;
                string cusDiscount = string.Empty;
                string cusTotal;
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax

                    //shipping
                    var orderShippingInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                    cusShipTotal = _priceFormatter.FormatShippingPrice(orderShippingInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, true);
                    //payment method additional fee
                    var paymentMethodAdditionalFeeInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate);
                    cusPaymentMethodAdditionalFee = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, true);
                }
                else
                {
                    //excluding tax

                    //shipping
                    var orderShippingExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate);
                    cusShipTotal = _priceFormatter.FormatShippingPrice(orderShippingExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, false);
                    //payment method additional fee
                    var paymentMethodAdditionalFeeExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate);
                    cusPaymentMethodAdditionalFee = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, false);
                }

                //shipping
                bool displayShipping = order.ShippingStatus != ShippingStatus.ShippingNotRequired;

                //payment method fee
                bool displayPaymentMethodFee = order.PaymentMethodAdditionalFeeExclTax > decimal.Zero;

                //tax
                bool displayTax = true;
                bool displayTaxRates = true;
                if (_taxSettings.HideTaxInOrderSummary && order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
                else
                {
                    if (order.OrderTax == 0 && _taxSettings.HideZeroTax)
                    {
                        displayTax = false;
                        displayTaxRates = false;
                    }
                    else
                    {
                        taxRates = new SortedDictionary<decimal, decimal>();
                        foreach (var tr in order.TaxRatesDictionary)
                            taxRates.Add(tr.Key, _currencyService.ConvertCurrency(tr.Value, order.CurrencyRate));

                        displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
                        displayTax = !displayTaxRates;

                        var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate);
                        string taxStr = _priceFormatter.FormatPrice(orderTaxInCustomerCurrency, true, order.CustomerCurrencyCode, false, language);
                        cusTaxTotal = taxStr;
                    }
                }

                //discount
                bool displayDiscount = false;
                if (order.OrderDiscount > decimal.Zero)
                {
                    var orderDiscountInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);
                    cusDiscount = _priceFormatter.FormatPrice(-orderDiscountInCustomerCurrency, true, order.CustomerCurrencyCode, false, language);
                    displayDiscount = true;
                }

                //total
                var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
                cusTotal = _priceFormatter.FormatPrice(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, language);




                //subtotal
                sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.SubTotal", languageId), cusSubTotal));

                //discount (applied to order subtotal)
                if (displaySubTotalDiscount)
                {
                    sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.SubTotalDiscount", languageId), cusSubTotalDiscount));
                }


                //shipping
                if (displayShipping)
                {
                    sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.Shipping", languageId), cusShipTotal));
                }

                //payment method fee
                if (displayPaymentMethodFee)
                {
                    string paymentMethodFeeTitle = _localizationService.GetResource("Messages.Order.PaymentMethodAdditionalFee", languageId);
                    sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, paymentMethodFeeTitle, cusPaymentMethodAdditionalFee));
                }

                //tax
                if (displayTax)
                {
                    sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.Tax", languageId), cusTaxTotal));
                }
                if (displayTaxRates)
                {
                    foreach (var item in taxRates)
                    {
                        string taxRate = String.Format(_localizationService.GetResource("Messages.Order.TaxRateLine"), _priceFormatter.FormatTaxRate(item.Key));
                        string taxValue = _priceFormatter.FormatPrice(item.Value, true, order.CustomerCurrencyCode, false, language);
                        sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, taxRate, taxValue));
                    }
                }

                //discount
                if (displayDiscount)
                {
                    sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.TotalDiscount", languageId), cusDiscount));
                }

                //gift cards
                var _servicegiftCard = EngineContext.Current.Resolve<IGiftCardService>();
                var gcuhC = _servicegiftCard.GetAllGiftCardUsageHistory(order.Id);
                foreach (var gcuh in gcuhC)
                {
                    var giftCard = EngineContext.Current.Resolve<IGiftCardService>().GetGiftCardById(gcuh.GiftCardId);
                    string giftCardText = String.Format(_localizationService.GetResource("Messages.Order.GiftCardInfo", languageId), WebUtility.HtmlEncode(giftCard.GiftCardCouponCode));

                    string giftCardAmount = _priceFormatter.FormatPrice(-(_currencyService.ConvertCurrency(gcuh.UsedValue, order.CurrencyRate)), true, order.CustomerCurrencyCode, false, language);
                    sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, giftCardText, giftCardAmount));
                }

                //reward points
                if (order.RedeemedRewardPointsEntry != null)
                {
                    string rpTitle = string.Format(_localizationService.GetResource("Messages.Order.RewardPoints", languageId), -order.RedeemedRewardPointsEntry.Points);
                    string rpAmount = _priceFormatter.FormatPrice(-(_currencyService.ConvertCurrency(order.RedeemedRewardPointsEntry.UsedAmount, order.CurrencyRate)), true, order.CustomerCurrencyCode, false, language);
                    sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, rpTitle, rpAmount));
                }

                //total
                sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.OrderTotal", languageId), cusTotal));
                #endregion

            }

            sb.AppendLine("</table>");
            result = sb.ToString();
            return result;
        }

        protected virtual string ProductListToHtmlTable(ReturnRequest returnRequest)
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

            var order = _orderService.GetOrderById(returnRequest.OrderId);
            IProductService _productService = EngineContext.Current.Resolve<IProductService>();

            foreach (var rrItem in returnRequest.ReturnRequestItems)
            {
                var orderItem = order.OrderItems.Where(x => x.Id == rrItem.OrderItemId).First();

                sb.AppendLine(string.Format("<tr style=\"background-color: {0};text-align: center;\">", _templatesSettings.Color2));
                string productName = _productService.GetProductById(orderItem.ProductId).GetLocalized(x => x.Name, order.CustomerLanguageId);

                sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + WebUtility.HtmlEncode(productName));

                sb.AppendLine("</td>");

                string unitPriceStr;
                var language = _languageService.GetLanguageById(order.CustomerLanguageId);
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                    unitPriceStr = _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, true);
                }
                else
                {
                    //excluding tax
                    var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                    unitPriceStr = _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, false);
                }
                sb.AppendLine(string.Format("<td style=\"padding: 0.6em 0.4em;text-align: right;\">{0}</td>", unitPriceStr));

                sb.AppendLine(string.Format("<td style=\"padding: 0.6em 0.4em;text-align: center;\">{0}</td>", orderItem.Quantity));

                sb.AppendLine(string.Format("<td style=\"padding: 0.6em 0.4em;text-align: center;\">{0}</td>", rrItem.ReasonForReturn));

                sb.AppendLine(string.Format("<td style=\"padding: 0.6em 0.4em;text-align: center;\">{0}</td>", rrItem.RequestedAction));
            }

            sb.AppendLine("</table>");
            return sb.ToString();
        }

        /// <summary>
        /// Convert a collection to a HTML table
        /// </summary>
        /// <param name="shipment">Shipment</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>HTML table of products</returns>
        protected virtual string ProductListToHtmlTable(Shipment shipment, string languageId)
        {
            string result;

            var sb = new StringBuilder();
            sb.AppendLine("<table border=\"0\" style=\"width:100%;\">");

            #region Products
            sb.AppendLine(string.Format("<tr style=\"background-color:{0};text-align:center;\">", _templatesSettings.Color1));
            sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Name", languageId)));
            sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Quantity", languageId)));
            sb.AppendLine("</tr>");

            var table = shipment.ShipmentItems.ToList();
            var order = _orderService.GetOrderById(shipment.OrderId);
            var productService = EngineContext.Current.Resolve<IProductService>();
            for (int i = 0; i <= table.Count - 1; i++)
            {
                var si = table[i];
                var orderItem = order.OrderItems.Where(x => x.Id == si.OrderItemId).FirstOrDefault();
                if (orderItem == null)
                    continue;

                var product = productService.GetProductByIdIncludeArch(orderItem.ProductId);
                if (product == null)
                    continue;

                sb.AppendLine(string.Format("<tr style=\"background-color: {0};text-align: center;\">", _templatesSettings.Color2));
                //product name
                string productName = product.GetLocalized(x => x.Name, languageId);

                sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + WebUtility.HtmlEncode(productName));
                //attributes
                if (!String.IsNullOrEmpty(orderItem.AttributeDescription))
                {
                    sb.AppendLine("<br />");
                    sb.AppendLine(orderItem.AttributeDescription);
                }
                //sku
                if (_catalogSettings.ShowSkuOnProductDetailsPage)
                {
                    var sku = product.FormatSku(orderItem.AttributesXml, _productAttributeParser);
                    if (!String.IsNullOrEmpty(sku))
                    {
                        sb.AppendLine("<br />");
                        sb.AppendLine(string.Format(_localizationService.GetResource("Messages.Order.Product(s).SKU", languageId), WebUtility.HtmlEncode(sku)));
                    }
                }
                sb.AppendLine("</td>");

                sb.AppendLine(string.Format("<td style=\"padding: 0.6em 0.4em;text-align: center;\">{0}</td>", si.Quantity));

                sb.AppendLine("</tr>");
            }
            #endregion

            sb.AppendLine("</table>");
            result = sb.ToString();
            return result;
        }


        protected virtual string ShoppingCartWishListProductListToHtmlTable(Customer customer, string languageId, bool cart, bool withPicture)
        {
            string result;

            var sb = new StringBuilder();
            sb.AppendLine("<table border=\"0\" style=\"width:100%;\">");

            #region Products
            sb.AppendLine(string.Format("<tr style=\"background-color:{0};text-align:center;\">", _templatesSettings.Color1));
            if (withPicture)
                sb.AppendLine(string.Format("<th>{0}</th>", cart ? _localizationService.GetResource("Messages.ShoppingCart.Product(s).Picture", languageId) : _localizationService.GetResource("Messages.Wishlist.Product(s).Picture", languageId)));
            sb.AppendLine(string.Format("<th>{0}</th>", cart ? _localizationService.GetResource("Messages.ShoppingCart.Product(s).Name", languageId) : _localizationService.GetResource("Messages.Wishlist.Product(s).Name", languageId)));
            sb.AppendLine(string.Format("<th>{0}</th>", cart ? _localizationService.GetResource("Messages.ShoppingCart.Product(s).Quantity", languageId) : _localizationService.GetResource("Messages.Wishlist.Product(s).Quantity", languageId)));
            sb.AppendLine("</tr>");
            var productService = EngineContext.Current.Resolve<IProductService>();
            var pictureService = EngineContext.Current.Resolve<IPictureService>();

            foreach (var item in cart ? customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart) :
                customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.Wishlist))
            {
                var product = productService.GetProductById(item.ProductId);
                sb.AppendLine(string.Format("<tr style=\"background-color: {0};text-align: center;\">", _templatesSettings.Color2));
                //product name
                string productName = product.GetLocalized(x => x.Name, languageId);
                if (withPicture)
                {
                    string pictureUrl = "";
                    if (product.ProductPictures.Any())
                    {
                        var picture = pictureService.GetPictureById(product.ProductPictures.OrderBy(x => x.DisplayOrder).FirstOrDefault().PictureId);
                        if (picture != null)
                        {
                            pictureUrl = pictureService.GetPictureUrl(picture, _templatesSettings.PictureSize);
                        }
                    }
                    sb.Append(string.Format("<td><img src=\"{0}\" alt=\"\"/></td>", pictureUrl));
                }
                sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + WebUtility.HtmlEncode(productName));
                //attributes
                if (!String.IsNullOrEmpty(item.AttributesXml))
                {
                    sb.AppendLine("<br />");
                    sb.AppendLine(item.AttributesXml);
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

        protected virtual string RecommendedProductsListToHtmlTable(Customer customer, string languageId, bool withPicture)
        {
            string result;

            var sb = new StringBuilder();
            sb.AppendLine("<table border=\"0\" style=\"width:100%;\">");

            #region Products
            sb.AppendLine(string.Format("<tr style=\"background-color:{0};text-align:center;\">", _templatesSettings.Color1));
            if (withPicture)
                sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.RecommendedProducts.Product(s).Picture", languageId)));
            sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.RecommendedProducts.Product(s).Name", languageId)));
            sb.AppendLine("</tr>");

            var productService = EngineContext.Current.Resolve<IProductService>();
            var pictureService = EngineContext.Current.Resolve<IPictureService>();
            var products = productService.GetRecommendedProducts(customer.GetCustomerRoleIds());

            foreach (var item in products)
            {

                sb.AppendLine(string.Format("<tr style=\"background-color: {0};text-align: center;\">", _templatesSettings.Color2));
                //product name
                string productName = item.GetLocalized(x => x.Name, languageId);
                if (withPicture)
                {
                    string pictureUrl = "";
                    if (item.ProductPictures.Any())
                    {
                        var picture = pictureService.GetPictureById(item.ProductPictures.OrderBy(x => x.DisplayOrder).FirstOrDefault().PictureId);
                        if (picture != null)
                        {
                            pictureUrl = pictureService.GetPictureUrl(picture, _templatesSettings.PictureSize);
                        }
                    }
                    sb.Append(string.Format("<td><img src=\"{0}\" alt=\"\"/></td>", pictureUrl));
                }
                sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + WebUtility.HtmlEncode(productName));

                sb.AppendLine("</td>");

                sb.AppendLine("</tr>");
            }
            #endregion

            sb.AppendLine("</table>");
            result = sb.ToString();
            return result;
        }

        protected virtual string RecentlyViewedProductsListToHtmlTable(Customer customer, string languageId, bool withPicture)
        {
            string result;

            var sb = new StringBuilder();
            sb.AppendLine("<table border=\"0\" style=\"width:100%;\">");

            #region Products
            sb.AppendLine(string.Format("<tr style=\"background-color:{0};text-align:center;\">", _templatesSettings.Color1));
            if (withPicture)
                sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.RecentlyViewedProducts.Product(s).Picture", languageId)));
            sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.RecentlyViewedProducts.Product(s).Name", languageId)));
            sb.AppendLine("</tr>");

            var recentlyViewedProductsService = EngineContext.Current.Resolve<IRecentlyViewedProductsService>();
            var pictureService = EngineContext.Current.Resolve<IPictureService>();
            var products = recentlyViewedProductsService.GetRecentlyViewedProducts(customer.Id, _catalogSettings.RecentlyViewedProductsNumber);
            foreach (var item in products)
            {
                sb.AppendLine(string.Format("<tr style=\"background-color: {0};text-align: center;\">", _templatesSettings.Color2));
                //product name
                string productName = item.GetLocalized(x => x.Name, languageId);
                if (withPicture)
                {
                    string pictureUrl = "";
                    if (item.ProductPictures.Any())
                    {
                        var picture = pictureService.GetPictureById(item.ProductPictures.OrderBy(x => x.DisplayOrder).FirstOrDefault().PictureId);
                        if (picture != null)
                        {
                            pictureUrl = pictureService.GetPictureUrl(picture, _templatesSettings.PictureSize);
                        }
                    }
                    sb.Append(string.Format("<td><img src=\"{0}\" alt=\"\"/></td>", pictureUrl));
                }
                sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + WebUtility.HtmlEncode(productName));
                sb.AppendLine("</td>");
                sb.AppendLine("</tr>");
            }
            #endregion

            sb.AppendLine("</table>");
            result = sb.ToString();
            return result;
        }


        /// <summary>
        /// Get store URL
        /// </summary>
        /// <param name="storeId">Store identifier; Pass 0 to load URL of the current store</param>
        /// <param name="useSsl">Use SSL</param>
        /// <returns></returns>
        protected virtual string GetStoreUrl(string storeId = "", bool useSsl = false)
        {
            var store = _storeService.GetStoreById(storeId) ?? _storeContext.CurrentStore;

            if (store == null)
                throw new Exception("No store could be loaded");

            return useSsl ? store.SecureUrl : store.Url;
        }

        #endregion

        #region Methods

        public virtual void AddStoreTokens(IList<Token> tokens, Store store, EmailAccount emailAccount)
        {
            if (emailAccount == null)
                throw new ArgumentNullException("emailAccount");

            tokens.Add(new Token("Store.Name", store.GetLocalized(x => x.Name)));
            tokens.Add(new Token("Store.URL", store.Url, true));
            tokens.Add(new Token("Store.Email", emailAccount.Email));
            tokens.Add(new Token("Store.CompanyName", store.CompanyName));
            tokens.Add(new Token("Store.CompanyAddress", store.CompanyAddress));
            tokens.Add(new Token("Store.CompanyPhoneNumber", store.CompanyPhoneNumber));
            tokens.Add(new Token("Store.CompanyVat", store.CompanyVat));
            tokens.Add(new Token("Twitter.URL", _storeInformationSettings.TwitterLink));
            tokens.Add(new Token("Facebook.URL", _storeInformationSettings.FacebookLink));
            tokens.Add(new Token("YouTube.URL", _storeInformationSettings.YoutubeLink));
            tokens.Add(new Token("GooglePlus.URL", _storeInformationSettings.GooglePlusLink));
            tokens.Add(new Token("Instagram.URL", _storeInformationSettings.InstagramLink));
            tokens.Add(new Token("LinkedIn.URL", _storeInformationSettings.LinkedInLink));
            tokens.Add(new Token("Pinterest.URL", _storeInformationSettings.PinterestLink));
            //event notification
            _eventPublisher.EntityTokensAdded(store, tokens);
        }

        public virtual void AddOrderTokens(IList<Token> tokens, Order order, string languageId, string vendorId = "")
        {
            tokens.Add(new Token("Order.OrderNumber", order.OrderNumber.ToString()));

            tokens.Add(new Token("Order.CustomerFullName", string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName)));
            tokens.Add(new Token("Order.CustomerEmail", order.BillingAddress.Email));


            tokens.Add(new Token("Order.BillingFirstName", order.BillingAddress.FirstName));
            tokens.Add(new Token("Order.BillingLastName", order.BillingAddress.LastName));
            tokens.Add(new Token("Order.BillingPhoneNumber", order.BillingAddress.PhoneNumber));
            tokens.Add(new Token("Order.BillingEmail", order.BillingAddress.Email));
            tokens.Add(new Token("Order.BillingFaxNumber", order.BillingAddress.FaxNumber));
            tokens.Add(new Token("Order.BillingCompany", order.BillingAddress.Company));
            tokens.Add(new Token("Order.BillingVatNumber", order.BillingAddress.VatNumber));
            tokens.Add(new Token("Order.BillingAddress1", order.BillingAddress.Address1));
            tokens.Add(new Token("Order.BillingAddress2", order.BillingAddress.Address2));
            tokens.Add(new Token("Order.BillingCity", order.BillingAddress.City));
            tokens.Add(new Token("Order.BillingStateProvince", !String.IsNullOrEmpty(order.BillingAddress.StateProvinceId) ? EngineContext.Current.Resolve<IStateProvinceService>().GetStateProvinceById(order.BillingAddress.StateProvinceId).GetLocalized(x => x.Name) : ""));
            tokens.Add(new Token("Order.BillingZipPostalCode", order.BillingAddress.ZipPostalCode));
            tokens.Add(new Token("Order.BillingCountry", !String.IsNullOrEmpty(order.BillingAddress.CountryId) ? EngineContext.Current.Resolve<ICountryService>().GetCountryById(order.BillingAddress.CountryId).GetLocalized(x => x.Name) : ""));
            tokens.Add(new Token("Order.BillingCustomAttributes", _addressAttributeFormatter.FormatAttributes(order.BillingAddress.CustomAttributes), true));

            tokens.Add(new Token("Order.ShippingMethod", order.ShippingMethod));
            tokens.Add(new Token("Order.ShippingAdditionDescription", order.ShippingOptionAttributeDescription, true));
            tokens.Add(new Token("Order.ShippingFirstName", order.ShippingAddress != null ? order.ShippingAddress.FirstName : ""));
            tokens.Add(new Token("Order.ShippingLastName", order.ShippingAddress != null ? order.ShippingAddress.LastName : ""));
            tokens.Add(new Token("Order.ShippingPhoneNumber", order.ShippingAddress != null ? order.ShippingAddress.PhoneNumber : ""));
            tokens.Add(new Token("Order.ShippingEmail", order.ShippingAddress != null ? order.ShippingAddress.Email : ""));
            tokens.Add(new Token("Order.ShippingFaxNumber", order.ShippingAddress != null ? order.ShippingAddress.FaxNumber : ""));
            tokens.Add(new Token("Order.ShippingCompany", order.ShippingAddress != null ? order.ShippingAddress.Company : ""));
            tokens.Add(new Token("Order.ShippingAddress1", order.ShippingAddress != null ? order.ShippingAddress.Address1 : ""));
            tokens.Add(new Token("Order.ShippingAddress2", order.ShippingAddress != null ? order.ShippingAddress.Address2 : ""));
            tokens.Add(new Token("Order.ShippingCity", order.ShippingAddress != null ? order.ShippingAddress.City : ""));
            tokens.Add(new Token("Order.ShippingStateProvince", order.ShippingAddress != null && !String.IsNullOrEmpty(order.ShippingAddress.StateProvinceId) ? EngineContext.Current.Resolve<IStateProvinceService>().GetStateProvinceById(order.ShippingAddress.StateProvinceId).GetLocalized(x => x.Name) : ""));
            tokens.Add(new Token("Order.ShippingZipPostalCode", order.ShippingAddress != null ? order.ShippingAddress.ZipPostalCode : ""));
            tokens.Add(new Token("Order.ShippingCountry", order.ShippingAddress != null && !String.IsNullOrEmpty(order.ShippingAddress.CountryId) ? EngineContext.Current.Resolve<ICountryService>().GetCountryById(order.ShippingAddress.CountryId).GetLocalized(x => x.Name) : ""));
            tokens.Add(new Token("Order.ShippingCustomAttributes", _addressAttributeFormatter.FormatAttributes(order.ShippingAddress != null ? order.ShippingAddress.CustomAttributes : ""), true));

            var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(order.PaymentMethodSystemName);
            var paymentMethodName = paymentMethod != null ? paymentMethod.GetLocalizedFriendlyName(_localizationService, _workContext.WorkingLanguage.Id) : order.PaymentMethodSystemName;
            tokens.Add(new Token("Order.PaymentMethod", paymentMethodName));
            tokens.Add(new Token("Order.VatNumber", order.VatNumber));
            var sbCustomValues = new StringBuilder();
            var customValues = order.DeserializeCustomValues();
            if (customValues != null)
            {
                foreach (var item in customValues)
                {
                    sbCustomValues.AppendFormat("{0}: {1}", WebUtility.HtmlEncode(item.Key), WebUtility.HtmlEncode(item.Value != null ? item.Value.ToString() : ""));
                    sbCustomValues.Append("<br />");
                }
            }
            tokens.Add(new Token("Order.CustomValues", sbCustomValues.ToString(), true));

            tokens.Add(new Token("Order.Product(s)", ProductListToHtmlTable(order, languageId, vendorId), true));

            var language = _languageService.GetLanguageById(languageId);
            if (language != null && !String.IsNullOrEmpty(language.LanguageCulture))
            {
                var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(order.CustomerId);
                if (customer != null)
                {
                    DateTime createdOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, TimeZoneInfo.Utc, _dateTimeHelper.GetCustomerTimeZone(customer));
                    tokens.Add(new Token("Order.CreatedOn", createdOn.ToString("D", new CultureInfo(language.LanguageCulture))));
                }
            }
            else
            {
                tokens.Add(new Token("Order.CreatedOn", order.CreatedOnUtc.ToString("D")));
            }

            //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
            tokens.Add(new Token("Order.OrderURLForCustomer", string.Format("{0}orderdetails/{1}", GetStoreUrl(order.StoreId), order.Id), true));

            //event notification
            _eventPublisher.EntityTokensAdded(order, tokens);
        }

        public virtual void AddOrderRefundedTokens(IList<Token> tokens, Order order, decimal refundedAmount)
        {
            //should we convert it to customer currency?
            //most probably, no. It can cause some rounding or legal issues
            //furthermore, exchange rate could be changed
            //so let's display it the primary store currency

            var primaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
            var refundedAmountStr = _priceFormatter.FormatPrice(refundedAmount, true, primaryStoreCurrencyCode, false, _workContext.WorkingLanguage);

            tokens.Add(new Token("Order.AmountRefunded", refundedAmountStr));

            //event notification
            _eventPublisher.EntityTokensAdded(order, tokens);
        }

        public virtual void AddShipmentTokens(IList<Token> tokens, Shipment shipment, string languageId)
        {
            tokens.Add(new Token("Shipment.ShipmentNumber", shipment.ShipmentNumber.ToString()));
            tokens.Add(new Token("Shipment.TrackingNumber", shipment.TrackingNumber));

            var trackingNumberUrl = "";
            if (!String.IsNullOrEmpty(shipment.TrackingNumber))
            {
                //we cannot inject IShippingService into constructor because it'll cause circular references.
                //that's why we resolve it here this way
                var shippingService = EngineContext.Current.Resolve<IShippingService>();
                var _order = EngineContext.Current.Resolve<IOrderService>().GetOrderById(shipment.OrderId);
                var srcm = shippingService.LoadShippingRateComputationMethodBySystemName(_order.ShippingRateComputationMethodSystemName);
                if (srcm != null &&
                    srcm.PluginDescriptor.Installed &&
                    srcm.IsShippingRateComputationMethodActive(_shippingSettings))
                {
                    var shipmentTracker = srcm.ShipmentTracker;
                    if (shipmentTracker != null)
                    {
                        trackingNumberUrl = shipmentTracker.GetUrl(shipment.TrackingNumber);
                    }
                }
            }
            tokens.Add(new Token("Shipment.TrackingNumberURL",trackingNumberUrl));
            tokens.Add(new Token("Shipment.Product(s)", ProductListToHtmlTable(shipment, languageId), true));
            var order = EngineContext.Current.Resolve<IOrderService>().GetOrderById(shipment.OrderId);
            tokens.Add(new Token("Shipment.URLForCustomer", string.Format("{0}orderdetails/shipment/{1}", GetStoreUrl(order.StoreId), shipment.Id), true));

            //event notification
            _eventPublisher.EntityTokensAdded(shipment, tokens);
        }

        public virtual void AddOrderNoteTokens(IList<Token> tokens, OrderNote orderNote)
        {
            tokens.Add(new Token("Order.NewNoteText", orderNote.FormatOrderNoteText(), true));
            var order = EngineContext.Current.Resolve<IOrderService>().GetOrderById(orderNote.OrderId);
            tokens.Add(new Token("Order.OrderNoteAttachmentUrl", string.Format("{0}download/ordernotefile/{1}", GetStoreUrl(order.StoreId), orderNote.Id), true));
            //event notification
            _eventPublisher.EntityTokensAdded(orderNote, tokens);
        }

        public virtual void AddRecurringPaymentTokens(IList<Token> tokens, RecurringPayment recurringPayment)
        {
            tokens.Add(new Token("RecurringPayment.ID", recurringPayment.Id.ToString()));

            //event notification
            _eventPublisher.EntityTokensAdded(recurringPayment, tokens);
        }

        public virtual void AddReturnRequestTokens(IList<Token> tokens, ReturnRequest returnRequest, Order order)
        {
            var productService = EngineContext.Current.Resolve<IProductService>();
            var countryService = EngineContext.Current.Resolve<ICountryService>();
            var orderService = EngineContext.Current.Resolve<IOrderService>();
            tokens.Add(new Token("ReturnRequest.ID", returnRequest.ReturnNumber.ToString()));
            tokens.Add(new Token("ReturnRequest.OrderId", order.OrderNumber.ToString()));
            tokens.Add(new Token("ReturnRequest.CustomerComment", HtmlHelper.FormatText(returnRequest.CustomerComments, false, true, false, false, false, false), true));
            tokens.Add(new Token("ReturnRequest.StaffNotes", HtmlHelper.FormatText(returnRequest.StaffNotes, false, true, false, false, false, false), true));
            tokens.Add(new Token("ReturnRequest.Status", returnRequest.ReturnRequestStatus.GetLocalizedEnum(_localizationService, _workContext)));
            tokens.Add(new Token("ReturnRequest.Products", ProductListToHtmlTable(returnRequest), true));

            if (returnRequest.PickupDate != default(DateTime))
                tokens.Add(new Token("ReturnRequest.PickupDate", returnRequest.PickupDate.ToShortDateString()));

            if (returnRequest.PickupAddress != null)
            {
                tokens.Add(new Token("PickupAddress.FirstName", returnRequest.PickupAddress.FirstName));
                tokens.Add(new Token("PickupAddress.LastName", returnRequest.PickupAddress.LastName));
                tokens.Add(new Token("PickupAddress.PhoneNumber", returnRequest.PickupAddress.PhoneNumber));
                tokens.Add(new Token("PickupAddress.Email", returnRequest.PickupAddress.Email));
                tokens.Add(new Token("PickupAddress.FaxNumber", returnRequest.PickupAddress.FaxNumber));
                tokens.Add(new Token("PickupAddress.Company", returnRequest.PickupAddress.Company));
                tokens.Add(new Token("PickupAddress.VatNumber", returnRequest.PickupAddress.VatNumber));
                tokens.Add(new Token("PickupAddress.Address1", returnRequest.PickupAddress.Address1));
                tokens.Add(new Token("PickupAddress.Address2", returnRequest.PickupAddress.Address2));
                tokens.Add(new Token("PickupAddress.City", returnRequest.PickupAddress.City));
                tokens.Add(new Token("PickupAddress.StateProvince", !String.IsNullOrEmpty(returnRequest.PickupAddress.StateProvinceId) ? EngineContext.Current.Resolve<IStateProvinceService>().GetStateProvinceById(returnRequest.PickupAddress.StateProvinceId).GetLocalized(x => x.Name) : ""));
                tokens.Add(new Token("PickupAddress.ZipPostalCode", returnRequest.PickupAddress.ZipPostalCode));
                tokens.Add(new Token("PickupAddress.Country", !String.IsNullOrEmpty(returnRequest.PickupAddress.CountryId) ? EngineContext.Current.Resolve<ICountryService>().GetCountryById(order.BillingAddress.CountryId).GetLocalized(x => x.Name) : ""));
                tokens.Add(new Token("PickupAddress.CustomAttributes", _addressAttributeFormatter.FormatAttributes(returnRequest.PickupAddress.CustomAttributes), true));
            }

            //event notification
            _eventPublisher.EntityTokensAdded(returnRequest, tokens);
        }

        public virtual void AddGiftCardTokens(IList<Token> tokens, GiftCard giftCard)
        {
            tokens.Add(new Token("GiftCard.SenderName", giftCard.SenderName));
            tokens.Add(new Token("GiftCard.SenderEmail", giftCard.SenderEmail));
            tokens.Add(new Token("GiftCard.RecipientName", giftCard.RecipientName));
            tokens.Add(new Token("GiftCard.RecipientEmail", giftCard.RecipientEmail));
            tokens.Add(new Token("GiftCard.Amount", _priceFormatter.FormatPrice(giftCard.Amount, true, false)));
            tokens.Add(new Token("GiftCard.CouponCode", giftCard.GiftCardCouponCode));

            var giftCardMesage = !String.IsNullOrWhiteSpace(giftCard.Message) ?
                HtmlHelper.FormatText(giftCard.Message, false, true, false, false, false, false) : "";

            tokens.Add(new Token("GiftCard.Message", giftCardMesage, true));

            //event notification
            _eventPublisher.EntityTokensAdded(giftCard, tokens);
        }

        public virtual void AddCustomerTokens(IList<Token> tokens, Customer customer)
        {
            tokens.Add(new Token("Customer.Email", customer.Email));
            tokens.Add(new Token("Customer.Username", customer.Username));
            tokens.Add(new Token("Customer.FullName", customer.GetFullName()));
            tokens.Add(new Token("Customer.FirstName", customer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName)));
            tokens.Add(new Token("Customer.LastName", customer.GetAttribute<string>(SystemCustomerAttributeNames.LastName)));
            tokens.Add(new Token("Customer.VatNumber", customer.GetAttribute<string>(SystemCustomerAttributeNames.VatNumber)));
            tokens.Add(new Token("Customer.VatNumberStatus", ((VatNumberStatus)customer.GetAttribute<int>(SystemCustomerAttributeNames.VatNumberStatusId)).ToString()));

            var customAttributesXml = customer.GetAttribute<string>(SystemCustomerAttributeNames.CustomCustomerAttributes);
            tokens.Add(new Token("Customer.CustomAttributes", _customerAttributeFormatter.FormatAttributes(customAttributesXml), true));

            //note: we do not use SEO friendly URLS because we can get errors caused by having .(dot) in the URL (from the email address)
            //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
            string passwordRecoveryUrl = string.Format("{0}passwordrecovery/confirm?token={1}&email={2}", GetStoreUrl(), customer.GetAttribute<string>(SystemCustomerAttributeNames.PasswordRecoveryToken), WebUtility.UrlEncode(customer.Email));
            string accountActivationUrl = string.Format("{0}customer/activation?token={1}&email={2}", GetStoreUrl(), customer.GetAttribute<string>(SystemCustomerAttributeNames.AccountActivationToken), WebUtility.UrlEncode(customer.Email));
            var wishlistUrl = string.Format("{0}wishlist/{1}", GetStoreUrl(), customer.CustomerGuid);
            tokens.Add(new Token("Customer.PasswordRecoveryURL", passwordRecoveryUrl, true));
            tokens.Add(new Token("Customer.AccountActivationURL", accountActivationUrl, true));
            tokens.Add(new Token("Wishlist.URLForCustomer", wishlistUrl, true));

            //event notification
            _eventPublisher.EntityTokensAdded(customer, tokens);
        }

        public virtual void AddCustomerNoteTokens(IList<Token> tokens, CustomerNote customerNote)
        {
            tokens.Add(new Token("Customer.NewNoteText", customerNote.FormatCustomerNoteText(), true));
            tokens.Add(new Token("Customer.NewTitleText", customerNote.Title, true));
            tokens.Add(new Token("Customer.CustomerNoteAttachmentUrl", string.Format("{0}download/customernotefile/{1}", GetStoreUrl(""), customerNote.Id), true));
            //event notification
            _eventPublisher.EntityTokensAdded(customerNote, tokens);
        }


        public virtual void AddShoppingCartTokens(IList<Token> tokens, Customer customer)
        {
            string languageId = _languageService.GetAllLanguages().FirstOrDefault().Id;
            if (customer.GenericAttributes.FirstOrDefault(x => x.Key == "LanguageId") != null)
            {
                languageId = customer.GenericAttributes.FirstOrDefault(x => x.Key == "LanguageId").Value;
            }
            tokens.Add(new Token("ShoppingCart.Products", ShoppingCartWishListProductListToHtmlTable(customer, languageId, true, false), true));
            tokens.Add(new Token("ShoppingCart.ProductsWithPictures", ShoppingCartWishListProductListToHtmlTable(customer, languageId, true, true), true));
            tokens.Add(new Token("Wishlist.Products", ShoppingCartWishListProductListToHtmlTable(customer, languageId, true, false), true));
            tokens.Add(new Token("Wishlist.ProductsWithPictures", ShoppingCartWishListProductListToHtmlTable(customer, languageId, true, true), true));

            //event notification
            _eventPublisher.EntityTokensAdded(customer, tokens);
        }
        public virtual void AddRecommendedProductsTokens(IList<Token> tokens, Customer customer)
        {
            string languageId = _languageService.GetAllLanguages().FirstOrDefault().Id;
            if (customer.GenericAttributes.FirstOrDefault(x => x.Key == "LanguageId") != null)
            {
                languageId = customer.GenericAttributes.FirstOrDefault(x => x.Key == "LanguageId").Value;
            }
            //RecommendedProducts

            tokens.Add(new Token("RecommendedProducts.Products", RecommendedProductsListToHtmlTable(customer, languageId, false), true));
            tokens.Add(new Token("RecommendedProducts.ProductsWithPictures", RecommendedProductsListToHtmlTable(customer, languageId, true), true));

            //event notification
            _eventPublisher.EntityTokensAdded(customer, tokens);
        }

        public virtual void AddRecentlyViewedProductsTokens(IList<Token> tokens, Customer customer)
        {
            string languageId = _languageService.GetAllLanguages().FirstOrDefault().Id;
            if (customer.GenericAttributes.FirstOrDefault(x => x.Key == "LanguageId") != null)
            {
                languageId = customer.GenericAttributes.FirstOrDefault(x => x.Key == "LanguageId").Value;
            }

            //RecentlyViewed products

            tokens.Add(new Token("RecentlyViewedProducts.Products", RecentlyViewedProductsListToHtmlTable(customer, languageId, false), true));
            tokens.Add(new Token("RecentlyViewedProducts.ProductsWithPictures", RecentlyViewedProductsListToHtmlTable(customer, languageId, true), true));

            //event notification
            _eventPublisher.EntityTokensAdded(customer, tokens);
        }

        public virtual void AddVendorTokens(IList<Token> tokens, Vendor vendor)
        {
            tokens.Add(new Token("Vendor.Name", vendor.Name));
            tokens.Add(new Token("Vendor.Email", vendor.Email));
            tokens.Add(new Token("Vendor.Description", vendor.Description));
            tokens.Add(new Token("Vendor.Address1", vendor.Address?.Address1));
            tokens.Add(new Token("Vendor.Address2", vendor.Address?.Address2));
            tokens.Add(new Token("Vendor.City", vendor.Address?.City));
            tokens.Add(new Token("Vendor.Company", vendor.Address?.Company));
            tokens.Add(new Token("Vendor.FaxNumber", vendor.Address?.FaxNumber));
            tokens.Add(new Token("Vendor.PhoneNumber", vendor.Address?.PhoneNumber));
            tokens.Add(new Token("Vendor.ZipPostalCode", vendor.Address?.ZipPostalCode));
            tokens.Add(new Token("Vendor.StateProvince", !String.IsNullOrEmpty(vendor.Address?.StateProvinceId) ? EngineContext.Current.Resolve<IStateProvinceService>().GetStateProvinceById(vendor.Address?.StateProvinceId).GetLocalized(x => x.Name) : ""));
            tokens.Add(new Token("Vendor.Country", !String.IsNullOrEmpty(vendor.Address?.CountryId) ? EngineContext.Current.Resolve<ICountryService>().GetCountryById(vendor.Address?.CountryId).GetLocalized(x => x.Name) : ""));

            //event notification
            _eventPublisher.EntityTokensAdded(vendor, tokens);
        }


        public virtual void AddNewsLetterSubscriptionTokens(IList<Token> tokens, NewsLetterSubscription subscription)
        {
            tokens.Add(new Token("NewsLetterSubscription.Email", subscription.Email));


            const string urlFormat = "{0}newsletter/subscriptionactivation/{1}/{2}";

            var activationUrl = String.Format(urlFormat, GetStoreUrl(), subscription.NewsLetterSubscriptionGuid, "true");
            tokens.Add(new Token("NewsLetterSubscription.ActivationUrl", activationUrl, true));

            var deActivationUrl = String.Format(urlFormat, GetStoreUrl(), subscription.NewsLetterSubscriptionGuid, "false");
            tokens.Add(new Token("NewsLetterSubscription.DeactivationUrl", deActivationUrl, true));

            //event notification
            _eventPublisher.EntityTokensAdded(subscription, tokens);
        }

        public virtual void AddProductReviewTokens(IList<Token> tokens, ProductReview productReview)
        {
            var product = EngineContext.Current.Resolve<IProductService>().GetProductById(productReview.ProductId);
            tokens.Add(new Token("ProductReview.ProductName", product.Name));

            //event notification
            _eventPublisher.EntityTokensAdded(productReview, tokens);
        }

        public virtual void AddVendorReviewTokens(IList<Token> tokens, VendorReview vendorReview)
        {
            var vendor = EngineContext.Current.Resolve<IVendorService>().GetVendorById(vendorReview.VendorId);
            tokens.Add(new Token("VendorReview.VendorName", vendor.Name));

            //event notification
            _eventPublisher.EntityTokensAdded(vendorReview, tokens);
        }

        public virtual void AddBlogCommentTokens(string storeId, IList<Token> tokens, BlogComment blogComment)
        {
            var blogPost = EngineContext.Current.Resolve<IBlogService>().GetBlogPostById(blogComment.BlogPostId);
            tokens.Add(new Token("BlogComment.BlogPostTitle", blogPost.Title));

            var blogUrl = $"{GetStoreUrl(storeId)}{blogPost.GetSeName()}";
            tokens.Add(new Token("BlogPost.URL", blogUrl, true));

            //event notification
            _eventPublisher.EntityTokensAdded(blogComment, tokens);
        }

        public virtual void AddArticleCommentTokens(string storeId, IList<Token> tokens, KnowledgebaseArticleComment articleComment)
        {
            var article = EngineContext.Current.Resolve<IKnowledgebaseService>().GetPublicKnowledgebaseArticle(articleComment.ArticleId);
            tokens.Add(new Token("Article.ArticleTitle", article.Name));

            var articleUrl = $"{GetStoreUrl(storeId)}{article.GetSeName()}";
            tokens.Add(new Token("Article.URL", articleUrl, true));

            //event notification
            _eventPublisher.EntityTokensAdded(articleComment, tokens);
        }

        public virtual void AddNewsCommentTokens(string storeId, IList<Token> tokens, NewsComment newsComment)
        {
            var newsitem = EngineContext.Current.Resolve<INewsService>().GetNewsById(newsComment.NewsItemId);
            tokens.Add(new Token("NewsComment.NewsTitle", newsitem.Title));
            tokens.Add(new Token("NewsComment.CommentText", newsComment.CommentText));
            tokens.Add(new Token("NewsComment.CommentTitle", newsComment.CommentTitle));

            var newsUrl = $"{GetStoreUrl(storeId)}{newsitem.GetSeName()}";
            tokens.Add(new Token("News.Url", newsUrl, true));

            //event notification
            _eventPublisher.EntityTokensAdded(newsComment, tokens);
        }

        public virtual void AddProductTokens(IList<Token> tokens, Product product, string languageId)
        {
            var defaultCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);

            tokens.Add(new Token("Product.ID", product.Id.ToString()));
            tokens.Add(new Token("Product.Name", product.GetLocalized(x => x.Name, languageId)));
            tokens.Add(new Token("Product.ShortDescription", product.GetLocalized(x => x.ShortDescription, languageId), true));
            tokens.Add(new Token("Product.SKU", product.Sku));
            tokens.Add(new Token("Product.StockQuantity", product.GetTotalStockQuantity().ToString()));
            tokens.Add(new Token("Product.Price", _priceFormatter.FormatPrice(product.Price, true, defaultCurrency)));

            //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
            var productUrl = string.Format("{0}{1}", GetStoreUrl(), product.GetSeName());
            tokens.Add(new Token("Product.ProductURLForCustomer", productUrl, true));

            //event notification
            _eventPublisher.EntityTokensAdded(product, tokens);
        }

        public virtual void AddAttributeCombinationTokens(IList<Token> tokens, ProductAttributeCombination combination, string languageId)
        {
            //attributes
            //we cannot inject IProductAttributeFormatter into constructor because it'll cause circular references.
            //that's why we resolve it here this way
            var product = EngineContext.Current.Resolve<IProductService>().GetProductById(combination.ProductId);
            var productAttributeFormatter = EngineContext.Current.Resolve<IProductAttributeFormatter>();
            string attributes = productAttributeFormatter.FormatAttributes(product,
                combination.AttributesXml,
                _workContext.CurrentCustomer,
                renderPrices: false);



            tokens.Add(new Token("AttributeCombination.Formatted", attributes, true));
            tokens.Add(new Token("AttributeCombination.SKU", product.FormatSku(combination.AttributesXml, _productAttributeParser)));
            tokens.Add(new Token("AttributeCombination.StockQuantity", combination.StockQuantity.ToString()));

            //event notification
            _eventPublisher.EntityTokensAdded(combination, tokens);
        }

        public virtual void AddForumTopicTokens(IList<Token> tokens, ForumTopic forumTopic,
            int? friendlyForumTopicPageIndex = null, string appendedPostIdentifierAnchor = "")
        {
            //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
            string topicUrl;
            if (friendlyForumTopicPageIndex.HasValue && friendlyForumTopicPageIndex.Value > 1)
                topicUrl = string.Format("{0}boards/topic/{1}/{2}/page/{3}", GetStoreUrl(), forumTopic.Id, forumTopic.GetSeName(), friendlyForumTopicPageIndex.Value);
            else
                topicUrl = string.Format("{0}boards/topic/{1}/{2}", GetStoreUrl(), forumTopic.Id, forumTopic.GetSeName());
            if (!String.IsNullOrEmpty(appendedPostIdentifierAnchor))
                topicUrl = string.Format("{0}#{1}", topicUrl, appendedPostIdentifierAnchor);
            tokens.Add(new Token("Forums.TopicURL", topicUrl, true));
            tokens.Add(new Token("Forums.TopicName", forumTopic.Subject));

            //event notification
            _eventPublisher.EntityTokensAdded(forumTopic, tokens);
        }

        public virtual void AddForumPostTokens(IList<Token> tokens, ForumPost forumPost)
        {
            var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(forumPost.CustomerId);
            tokens.Add(new Token("Forums.PostAuthor", customer.FormatUserName()));
            tokens.Add(new Token("Forums.PostBody", forumPost.FormatPostText(), true));

            //event notification
            _eventPublisher.EntityTokensAdded(forumPost, tokens);
        }

        public virtual void AddForumTokens(IList<Token> tokens, Forum forum)
        {
            //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
            var forumUrl = string.Format("{0}boards/forum/{1}/{2}", GetStoreUrl(), forum.Id, forum.GetSeName());
            tokens.Add(new Token("Forums.ForumURL", forumUrl, true));
            tokens.Add(new Token("Forums.ForumName", forum.Name));

            //event notification
            _eventPublisher.EntityTokensAdded(forum, tokens);
        }

        public virtual void AddPrivateMessageTokens(IList<Token> tokens, PrivateMessage privateMessage)
        {
            tokens.Add(new Token("PrivateMessage.Subject", privateMessage.Subject));
            tokens.Add(new Token("PrivateMessage.Text", privateMessage.FormatPrivateMessageText(), true));

            //event notification
            _eventPublisher.EntityTokensAdded(privateMessage, tokens);
        }

        public virtual void AddBackInStockTokens(IList<Token> tokens, BackInStockSubscription subscription)
        {
            var product = EngineContext.Current.Resolve<IProductService>().GetProductById(subscription.ProductId);
            tokens.Add(new Token("BackInStockSubscription.ProductName", product.Name));
            //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
            var productUrl = string.Format("{0}{1}", GetStoreUrl(subscription.StoreId), product.GetSeName());
            tokens.Add(new Token("BackInStockSubscription.ProductUrl", productUrl, true));

            //event notification
            _eventPublisher.EntityTokensAdded(subscription, tokens);
        }

        public virtual void AddAuctionTokens(IList<Token> tokens, Product product, Bid bid)
        {
            var defaultCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
            tokens.Add(new Token("Auctions.ProductName", product.Name));
            tokens.Add(new Token("Auctions.Price", _priceFormatter.FormatPrice(bid.Amount, true, defaultCurrency)));
            tokens.Add(new Token("Auctions.EndTime", product.AvailableEndDateTimeUtc.ToString()));
            tokens.Add(new Token("Auctions.ProductSeName", product.SeName));

            _eventPublisher.EntityTokensAdded(bid, tokens);
        }

        /// <summary>
        /// Gets list of allowed (supported) message tokens for campaigns
        /// </summary>
        /// <returns>List of allowed (supported) message tokens for campaigns</returns>
        public virtual string[] GetListOfCampaignAllowedTokens()
        {
            var allowedTokens = new List<string>
            {
                "%Store.Name%",
                "%Store.URL%",
                "%Store.Email%",
                "%Store.CompanyName%",
                "%Store.CompanyAddress%",
                "%Store.CompanyPhoneNumber%",
                "%Store.CompanyVat%",
                "%NewsLetterSubscription.Email%",
                "%NewsLetterSubscription.ActivationUrl%",
                "%NewsLetterSubscription.DeactivationUrl%",
                "%ShoppingCart.Products%",
                "%ShoppingCart.ProductsWithPictures%",
                "%Wishlist.Products%",
                "%Wishlist.ProductsWithPictures%",
                "%RecommendedProducts.Products%",
                "%RecommendedProducts.ProductsWithPictures%",
                "%RecentlyViewedProducts.Products%",
                "%RecentlyViewedProducts.ProductsWithPictures%",
            };
            return allowedTokens.ToArray();
        }

        public virtual string[] GetListOfAllowedTokens()
        {
            var allowedTokens = new List<string>
            {
                "%Store.Name%",
                "%Store.URL%",
                "%Store.Email%",
                "%Store.CompanyName%",
                "%Store.CompanyAddress%",
                "%Store.CompanyPhoneNumber%",
                "%Store.CompanyVat%",
                "%Twitter.URL%",
                "%Facebook.URL%",
                "%YouTube.URL%",
                "%GooglePlus.URL%",
                "%Instagram.URL%",
                "%LinkedIn.URL%",
                "%Pinterest.URL%",
                "%Order.OrderNumber%",
                "%Order.CustomerFullName%",
                "%Order.CustomerEmail%",
                "%Order.BillingFirstName%",
                "%Order.BillingLastName%",
                "%Order.BillingPhoneNumber%",
                "%Order.BillingEmail%",
                "%Order.BillingFaxNumber%",
                "%Order.BillingCompany%",
                "%Order.BillingAddress1%",
                "%Order.BillingAddress2%",
                "%Order.BillingCity%",
                "%Order.BillingStateProvince%",
                "%Order.BillingZipPostalCode%",
                "%Order.BillingCountry%",
                "%Order.BillingCustomAttributes%",
                "%Order.ShippingMethod%",
                "%Order.ShippingAdditionDescription%",
                "%Order.ShippingFirstName%",
                "%Order.ShippingLastName%",
                "%Order.ShippingPhoneNumber%",
                "%Order.ShippingEmail%",
                "%Order.ShippingFaxNumber%",
                "%Order.ShippingCompany%",
                "%Order.ShippingAddress1%",
                "%Order.ShippingAddress2%",
                "%Order.ShippingCity%",
                "%Order.ShippingStateProvince%",
                "%Order.ShippingZipPostalCode%",
                "%Order.ShippingCountry%",
                "%Order.ShippingCustomAttributes%",
                "%Order.PaymentMethod%",
                "%Order.VatNumber%",
                "%Order.CustomValues%",
                "%Order.Product(s)%",
                "%Order.CreatedOn%",
                "%Order.OrderURLForCustomer%",
                "%Order.NewNoteText%",
                "%Order.OrderNoteAttachmentUrl%",
                "%Order.AmountRefunded%",
                "%RecurringPayment.ID%",
                "%Shipment.ShipmentNumber%",
                "%Shipment.TrackingNumber%",
                "%Shipment.TrackingNumberURL%",
                "%Shipment.Product(s)%",
                "%Shipment.URLForCustomer%",
                "%ReturnRequest.ID%",
                "%ReturnRequest.OrderId%",
                "%ReturnRequest.Product.Quantity%",
                "%ReturnRequest.Product.Name%",
                "%ReturnRequest.Reason%",
                "%ReturnRequest.RequestedAction%",
                "%ReturnRequest.CustomerComment%",
                "%ReturnRequest.StaffNotes%",
                "%ReturnRequest.Status%",
                "%GiftCard.SenderName%",
                "%GiftCard.SenderEmail%",
                "%GiftCard.RecipientName%",
                "%GiftCard.RecipientEmail%",
                "%GiftCard.Amount%",
                "%GiftCard.CouponCode%",
                "%GiftCard.Message%",
                "%Customer.Email%",
                "%Customer.Username%",
                "%Customer.FullName%",
                "%Customer.FirstName%",
                "%Customer.LastName%",
                "%Customer.VatNumber%",
                "%Customer.CustomAttributes%",
                "%Customer.PasswordRecoveryURL%",
                "%Customer.AccountActivationURL%",
                "%Customer.NewNoteText%",
                "%Customer.NewTitleText%",
                "%Customer.CustomerNoteAttachmentUrl%",
                "%ContactUs.SenderEmail%",
                "%ContactUs.SenderName%",
                "%ContactUs.Body%",
                "%ContactUs.AttributeDescription%",
                "%Vendor.Address1%",
                "%Vendor.Address2%",
                "%Vendor.City%",
                "%Vendor.Company%",
                "%Vendor.Country%",
                "%Vendor.Description%",
                "%Vendor.Email%",
                "%Vendor.FaxNumber%",
                "%Vendor.Name%",
                "%Vendor.PhoneNumber%",
                "%Vendor.StateProvince%",
                "%Vendor.ZipPostalCode%",
                "%Wishlist.URLForCustomer%",
                "%NewsLetterSubscription.Email%",
                "%NewsLetterSubscription.ActivationUrl%",
                "%NewsLetterSubscription.DeactivationUrl%",
                "%ProductReview.ProductName%",
                "%BlogComment.BlogPostTitle%",
                "%BlogPost.URL%",
                "%NewsComment.NewsTitle%",
                "%NewsComment.CommentText%",
                "%NewsComment.CommentTitle%",
                "%News.Url%",
                "%Product.ID%",
                "%Product.Name%",
                "%Product.ShortDescription%",
                "%Product.ProductURLForCustomer%",
                "%Product.SKU%",
                "%Product.StockQuantity%",
                "%Forums.TopicURL%",
                "%Forums.TopicName%",
                "%Forums.PostAuthor%",
                "%Forums.PostBody%",
                "%Forums.ForumURL%",
                "%Forums.ForumName%",
                "%AttributeCombination.Formatted%",
                "%AttributeCombination.SKU%",
                "%AttributeCombination.StockQuantity%",
                "%PrivateMessage.Subject%",
                "%PrivateMessage.Text%",
                "%BackInStockSubscription.ProductName%",
                "%BackInStockSubscription.ProductUrl%",
                "%Auctions.ProductName%",
                "%Auctions.Price%",
                "%Auctions.EndTime%",
                "%Auctions.ProductSeName%"
            };
            return allowedTokens.ToArray();
        }

        public virtual string[] GetListOfCustomerReminderAllowedTokens(CustomerReminderRuleEnum rule)
        {
            var allowedTokens = new List<string>();
            allowedTokens.AddRange(
                new List<string>{ "%Store.Name%",
                "%Store.URL%",
                "%Store.Email%",
                "%Store.CompanyName%",
                "%Store.CompanyAddress%",
                "%Store.CompanyPhoneNumber%",
                "%Store.CompanyVat%",
                "%Twitter.URL%",
                "%Facebook.URL%",
                "%YouTube.URL%",
                "%GooglePlus.URL%",
                "%Instagram.URL%",
                "%LinkedIn.URL%",
                "%Pinterest.URL%"});

            if (rule == CustomerReminderRuleEnum.AbandonedCart)
            {
                allowedTokens.Add("%ShoppingCart.Products%");
                allowedTokens.Add("%ShoppingCart.ProductsWithPictures%");
                allowedTokens.Add("%Wishlist.Products%");
                allowedTokens.Add("%Wishlist.ProductsWithPictures%");
            }
            if (rule == CustomerReminderRuleEnum.CompletedOrder || rule == CustomerReminderRuleEnum.UnpaidOrder)
            {
                allowedTokens.AddRange(
                new List<string>{
                "%Order.OrderNumber%",
                "%Order.CustomerFullName%",
                "%Order.CustomerEmail%",
                "%Order.BillingFirstName%",
                "%Order.BillingLastName%",
                "%Order.BillingPhoneNumber%",
                "%Order.BillingEmail%",
                "%Order.BillingFaxNumber%",
                "%Order.BillingCompany%",
                "%Order.BillingAddress1%",
                "%Order.BillingAddress2%",
                "%Order.BillingCity%",
                "%Order.BillingStateProvince%",
                "%Order.BillingZipPostalCode%",
                "%Order.BillingCountry%",
                "%Order.BillingCustomAttributes%",
                "%Order.ShippingMethod%",
                "%Order.ShippingAdditionDescription%",
                "%Order.ShippingFirstName%",
                "%Order.ShippingLastName%",
                "%Order.ShippingPhoneNumber%",
                "%Order.ShippingEmail%",
                "%Order.ShippingFaxNumber%",
                "%Order.ShippingCompany%",
                "%Order.ShippingAddress1%",
                "%Order.ShippingAddress2%",
                "%Order.ShippingCity%",
                "%Order.ShippingStateProvince%",
                "%Order.ShippingZipPostalCode%",
                "%Order.ShippingCountry%",
                "%Order.ShippingCustomAttributes%",
                "%Order.PaymentMethod%",
                "%Order.VatNumber%",
                "%Order.CustomValues%",
                "%Order.Product(s)%",
                "%Order.CreatedOn%",
                "%Order.OrderURLForCustomer%",
                "%Order.NewNoteText%",
                "%Order.OrderNoteAttachmentUrl%",
                "%Order.AmountRefunded%"
                });
            }
            allowedTokens.AddRange(
                new List<string>{
                "%Customer.Email%",
                "%Customer.Username%",
                "%Customer.FullName%",
                "%Customer.FirstName%",
                "%Customer.LastName%",
                "%RecommendedProducts.Products%",
                "%RecommendedProducts.ProductsWithPictures%",
                "%RecentlyViewedProducts.Products%",
                "%RecentlyViewedProducts.ProductsWithPictures%",
                });
            return allowedTokens.ToArray();
        }

        #endregion
    }
}
