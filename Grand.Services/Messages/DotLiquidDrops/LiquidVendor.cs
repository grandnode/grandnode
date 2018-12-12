using DotLiquid;
using Grand.Core.Domain.Vendors;
using Grand.Core.Infrastructure;
using Grand.Services.Directory;
using Grand.Services.Localization;
using System;
using System.Collections.Generic;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public partial class LiquidVendor : Drop
    {
        private Vendor _vendor;

        public LiquidVendor(Vendor vendor)
        {
            this._vendor = vendor;

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

        public string StateProvince
        {
            get { return !String.IsNullOrEmpty(_vendor.Address?.StateProvinceId) ? EngineContext.Current.Resolve<IStateProvinceService>().GetStateProvinceById(_vendor.Address?.StateProvinceId).GetLocalized(x => x.Name) : ""; }
        }

        public string Country
        {
            get { return !String.IsNullOrEmpty(_vendor.Address?.CountryId) ? EngineContext.Current.Resolve<ICountryService>().GetCountryById(_vendor.Address?.CountryId).GetLocalized(x => x.Name) : ""; }
        }

        public IDictionary<string, string> AdditionalTokens { get; set; }
    }
}