using DotLiquid;
using Grand.Core.Domain.Orders;
using Grand.Core.Html;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidReturnRequest : Drop
    {
        private ReturnRequest _returnRequest;
        private Order _order;

        public LiquidReturnRequest(ReturnRequest returnRequest, Order order)
        {
            this._returnRequest = returnRequest;
            this._order = order;
                       
            AdditionalTokens = new Dictionary<string, string>();
        }

        public string Id
        {
            get { return _returnRequest.Id; }
        }

        public int ReturnNumber
        {
            get { return _returnRequest.ReturnNumber; }
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

        public string Status { get; set; }

        public string Products { get; set; }

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

        public string PickupAddressStateProvince { get; set; }

        public string PickupAddressZipPostalCode
        {
            get { return _returnRequest.PickupAddress.ZipPostalCode; }
        }

        public string PickupAddressCountry { get; set; }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}