using DotLiquid;
using Grand.Domain.Vendors;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidVendor : Drop
    {
        private Vendor _vendor;

        public LiquidVendor(Vendor vendor)
        {
            _vendor = vendor;
            AdditionalTokens = new Dictionary<string, string>();
        }

        public string Name
        {
            get { return _vendor.Name; }
        }

        public string Email
        {
            get { return _vendor.Email; }
        }

        public string Description
        {
            get { return _vendor.Description; }
        }

        public string Address1
        {
            get { return _vendor.Address?.Address1; }
        }

        public string Address2
        {
            get { return _vendor.Address?.Address2; }
        }

        public string City
        {
            get { return _vendor.Address?.City; }
        }

        public string Company
        {
            get { return _vendor.Address?.Company; }
        }

        public string FaxNumber
        {
            get { return _vendor.Address?.FaxNumber; }
        }

        public string PhoneNumber
        {
            get { return _vendor.Address?.PhoneNumber; }
        }

        public string ZipPostalCode
        {
            get { return _vendor.Address?.ZipPostalCode; }
        }

        public string StateProvince { get; set; }

        public string Country { get; set; }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}