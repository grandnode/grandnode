using DotLiquid;
using Grand.Core;
using Grand.Core.Domain.Messages;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Tax;
using Grand.Core.Html;
using Grand.Core.Infrastructure;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidReturnRequest : Drop
    {
        private ReturnRequest _returnRequest;
        private Order _order;

        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IOrderService _orderService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ILanguageService _languageService;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly MessageTemplatesSettings _templatesSettings;

        public LiquidReturnRequest(ReturnRequest returnRequest, Order order)
        {
            this._localizationService = EngineContext.Current.Resolve<ILocalizationService>();
            this._workContext = EngineContext.Current.Resolve<IWorkContext>();
            this._orderService = EngineContext.Current.Resolve<IOrderService>();
            this._currencyService = EngineContext.Current.Resolve<ICurrencyService>();
            this._priceFormatter = EngineContext.Current.Resolve<IPriceFormatter>();
            this._languageService = EngineContext.Current.Resolve<ILanguageService>();
            this._addressAttributeFormatter = EngineContext.Current.Resolve<IAddressAttributeFormatter>();
            this._templatesSettings = EngineContext.Current.Resolve<MessageTemplatesSettings>();

            this._returnRequest = returnRequest;
            this._order = order;
                       
            AdditionalTokens = new Dictionary<string, string>();
        }

        public string Id
        {
            get { return _returnRequest.ReturnNumber.ToString(); }
        }

        public string OrderId
        {
            get { return _order.OrderNumber.ToString(); }
        }

        public string CustomerComment
        {
            get { return HtmlHelper.FormatText(_returnRequest.CustomerComments, false, true, false, false, false, false); }
        }

        public string StaffNotes
        {
            get { return HtmlHelper.FormatText(_returnRequest.StaffNotes, false, true, false, false, false, false); }
        }

        public string Status
        {
            get { return _returnRequest.ReturnRequestStatus.GetLocalizedEnum(_localizationService, _workContext); }
        }

        public string Products
        {
            get { return ProductListToHtmlTable(_returnRequest); }
        }

        public string PickupDate
        {
            get { return _returnRequest.PickupDate.ToShortDateString(); }
        }

        public string PickupAddressFirstName
        {
            get { return _returnRequest.PickupAddress.FirstName; }
        }

        public string PickupAddressLastName
        {
            get { return _returnRequest.PickupAddress.LastName; }
        }

        public string PickupAddressPhoneNumber
        {
            get { return _returnRequest.PickupAddress.PhoneNumber; }
        }

        public string PickupAddressEmail
        {
            get { return _returnRequest.PickupAddress.Email; }
        }

        public string PickupAddressFaxNumber
        {
            get { return _returnRequest.PickupAddress.FaxNumber; }
        }

        public string PickupAddressCompany
        {
            get { return _returnRequest.PickupAddress.Company; }
        }

        public string PickupAddressVatNumber
        {
            get { return _returnRequest.PickupAddress.VatNumber; }
        }

        public string PickupAddressAddress1
        {
            get { return _returnRequest.PickupAddress.Address1; }
        }

        public string PickupAddressAddress2
        {
            get { return _returnRequest.PickupAddress.Address2; }
        }

        public string PickupAddressCity
        {
            get { return _returnRequest.PickupAddress.City; }
        }

        public string PickupAddressStateProvince
        {
            get { return !String.IsNullOrEmpty(_returnRequest.PickupAddress.StateProvinceId) ? EngineContext.Current.Resolve<IStateProvinceService>().GetStateProvinceById(_returnRequest.PickupAddress.StateProvinceId).GetLocalized(x => x.Name) : ""; }
        }

        public string PickupAddressZipPostalCode
        {
            get { return _returnRequest.PickupAddress.ZipPostalCode; }
        }

        public string PickupAddressCountry
        {
            get { return !String.IsNullOrEmpty(_returnRequest.PickupAddress.CountryId) ? EngineContext.Current.Resolve<ICountryService>().GetCountryById(_order.BillingAddress.CountryId).GetLocalized(x => x.Name) : ""; }
        }

        public string PickupAddressCustomAttributes
        {
            get { return _addressAttributeFormatter.FormatAttributes(_returnRequest.PickupAddress.CustomAttributes); }
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

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}