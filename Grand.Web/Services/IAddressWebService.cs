using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Directory;
using Grand.Web.Models.Common;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Grand.Core.Domain.Vendors;
using Grand.Web.Models.Vendors;

namespace Grand.Web.Services
{
    public partial interface IAddressWebService
    {

        void PrepareModel(AddressModel model,
            Address address, bool excludeProperties,
            Func<IList<Country>> loadCountries = null,
            bool prePopulateWithCustomerFields = false,
            Customer customer = null,
            string overrideAttributesXml = "");

        void PrepareAddressModel(AddressModel model,
            Address address, bool excludeProperties,
            Func<IList<Country>> loadCountries = null,
            bool prePopulateWithCustomerFields = false,
            Customer customer = null,
            AddressSettings addressSettings = null);

        void PrepareVendorAddressModel(VendorAddressModel model,
            Address address, bool excludeProperties,
            Func<IList<Country>> loadCountries = null,
            bool prePopulateWithCustomerFields = false,
            Customer customer = null,
            VendorSettings vendorSettings = null);

        void PrepareCustomAddressAttributes(AddressModel model,
            Address address, string overrideAttributesXml = "");

        string ParseCustomAddressAttributes(IFormCollection form);

        AddressSettings AddressSettings();
        IList<string> GetAttributeWarnings(string attributesXml);
    }
}