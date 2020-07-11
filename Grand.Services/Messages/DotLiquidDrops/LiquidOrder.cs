using DotLiquid;
using Grand.Domain.Customers;
using Grand.Domain.Directory;
using Grand.Domain.Localization;
using Grand.Domain.Orders;
using Grand.Domain.Shipping;
using Grand.Domain.Stores;
using Grand.Domain.Vendors;
using Grand.Services.Orders;
using Grand.Services.Payments;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidOrder : Drop
    {
        private Order _order;
        private Language _language;
        private Customer _customer;
        private Currency _currency;
        private Store _store;
        private OrderNote _orderNote;
        private ICollection<LiquidOrderItem> _orderItems;
        private Vendor _vendor;
        public LiquidOrder(Order order, Customer customer, Language language, Currency currency, Store store, OrderNote orderNote = null, Vendor vendor = null)
        {

            _order = order;
            _customer = customer;
            _language = language;
            _orderNote = orderNote;
            _currency = currency;
            _store = store;
            _vendor = vendor;
            _orderItems = new List<LiquidOrderItem>();
            AdditionalTokens = new Dictionary<string, string>();
        }

        public string OrderNumber {
            get { return _order.OrderNumber.ToString(); }
        }

        public string OrderCode {
            get { return _order.Code; }
        }

        public string CustomerFullName {
            get { return string.Format("{0} {1}", _order.BillingAddress.FirstName, _order.BillingAddress.LastName); }
        }

        public string CustomerEmail {
            get { return _order.BillingAddress.Email; }
        }

        public string BillingFirstName {
            get { return _order.BillingAddress.FirstName; }
        }

        public string BillingLastName {
            get { return _order.BillingAddress.LastName; }
        }

        public string BillingPhoneNumber {
            get { return _order.BillingAddress.PhoneNumber; }
        }

        public string BillingEmail {
            get { return _order.BillingAddress.Email; }
        }

        public string BillingFaxNumber {
            get { return _order.BillingAddress.FaxNumber; }
        }

        public string BillingCompany {
            get { return _order.BillingAddress.Company; }
        }

        public string BillingVatNumber {
            get { return _order.BillingAddress.VatNumber; }
        }

        public string BillingAddress1 {
            get { return _order.BillingAddress.Address1; }
        }

        public string BillingAddress2 {
            get { return _order.BillingAddress.Address2; }
        }

        public string BillingCity {
            get { return _order.BillingAddress.City; }
        }

        public string BillingStateProvince { get; set; }

        public string BillingZipPostalCode {
            get { return _order.BillingAddress.ZipPostalCode; }
        }

        public string BillingCountry { get; set; }

        public string BillingCustomAttributes { get; set; }

        public string ShippingMethod {
            get { return _order.ShippingMethod; }
        }

        public string ShippingAdditionDescription {
            get { return _order.ShippingOptionAttributeDescription; }
        }

        public string ShippingFirstName {
            get { return _order.ShippingAddress != null ? _order.ShippingAddress.FirstName : ""; }
        }

        public string ShippingLastName {
            get { return _order.ShippingAddress != null ? _order.ShippingAddress.LastName : ""; }
        }

        public string ShippingPhoneNumber {
            get { return _order.ShippingAddress != null ? _order.ShippingAddress.PhoneNumber : ""; }
        }

        public string ShippingEmail {
            get { return _order.ShippingAddress != null ? _order.ShippingAddress.Email : ""; }
        }

        public string ShippingFaxNumber {
            get { return _order.ShippingAddress != null ? _order.ShippingAddress.FaxNumber : ""; }
        }

        public string ShippingCompany {
            get { return _order.ShippingAddress != null ? _order.ShippingAddress.Company : ""; }
        }

        public string ShippingAddress1 {
            get { return _order.ShippingAddress != null ? _order.ShippingAddress.Address1 : ""; }
        }

        public string ShippingAddress2 {
            get { return _order.ShippingAddress != null ? _order.ShippingAddress.Address2 : ""; }
        }

        public string ShippingCity {
            get { return _order.ShippingAddress != null ? _order.ShippingAddress.City : ""; }
        }

        public string ShippingStateProvince { get; set; }

        public string ShippingZipPostalCode {
            get { return _order.ShippingAddress != null ? _order.ShippingAddress.ZipPostalCode : ""; }
        }

        public string ShippingCountry { get; set; }

        public string ShippingCustomAttributes { get; set; }

        public string PaymentMethod { get; set; }


        public string VatNumber {
            get { return _order.VatNumber; }
        }

        public string CustomValues {
            get {
                var sbCustomValues = new StringBuilder();
                var customValues = _order.DeserializeCustomValues();
                if (customValues != null)
                {
                    foreach (var item in customValues)
                    {
                        sbCustomValues.AppendFormat("{0}: {1}", WebUtility.HtmlEncode(item.Key), WebUtility.HtmlEncode(item.Value != null ? item.Value.ToString() : ""));
                        sbCustomValues.Append("<br />");
                    }
                }
                return sbCustomValues.ToString();
            }
        }

        public string CreatedOn {
            get {
                return _order.CreatedOnUtc.ToLocalTime().ToString("D");
            }
        }

        public string OrderURLForCustomer {
            get { return string.Format("{0}orderdetails/{1}", (_store.SslEnabled ? _store.SecureUrl : _store.Url), _order.Id); }
        }

        public string AmountRefunded { get; set; }

        public string NewNoteText {
            get { return _orderNote.FormatOrderNoteText(); }
        }

        public string OrderNoteAttachmentUrl {
            get {
                return string.Format("{0}download/ordernotefile/{1}", (_store.SslEnabled ? _store.SecureUrl : _store.Url), _orderNote.Id);
            }
        }

        public string VendorName {
            get { return _vendor?.Name; }
        }

        public string VendorEmail {
            get { return _vendor?.Email; }
        }

        public ICollection<LiquidOrderItem> OrderItems {
            get {
                return _orderItems;
            }
        }

        public bool DisplaySubTotalDiscount { get; set; }

        public string SubTotalDiscount { get; set; }

        public string SubTotal { get; set; }

        public string Shipping { get; set; }

        public string Tax { get; set; }

        public string Total { get; set; }

        public bool DisplayShipping {
            get {
                return _order.ShippingStatus != ShippingStatus.ShippingNotRequired;
            }
        }
        public bool DisplayPaymentMethodFee {
            get {
                return _order.PaymentMethodAdditionalFeeExclTax > decimal.Zero;
            }
        }

        public string PaymentMethodAdditionalFee { get; set; }

        public bool DisplayTax { get; set; }

        public bool DisplayTaxRates { get; set; }

        public Dictionary<string, string> TaxRates { get; set; }

        public bool DisplayDiscount { get; set; }

        public string Discount { get; set; }

        public string CheckoutAttributeDescription {
            get {
                return _order.CheckoutAttributeDescription;
            }
        }

        public Dictionary<string, string> GiftCards { get; set; }

        public bool RedeemedRewardPointsEntryExists {
            get {
                return _order.RedeemedRewardPointsEntry != null;
            }
        }

        public string RPTitle { get; set; }

        public string RPAmount { get; set; }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}
