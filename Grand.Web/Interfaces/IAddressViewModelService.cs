using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Vendors;
using Grand.Web.Models.Common;
using Grand.Web.Models.Vendors;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Interfaces
{
    public partial interface IAddressViewModelService
    {

        Task PrepareModel(AddressModel model,
            Address address, bool excludeProperties,
            Func<IList<Country>> loadCountries = null,
            bool prePopulateWithCustomerFields = false,
            Customer customer = null,
            string overrideAttributesXml = "");

        Task PrepareAddressModel(AddressModel model,
            Address address, bool excludeProperties,
            Func<IList<Country>> loadCountries = null,
            bool prePopulateWithCustomerFields = false,
            Customer customer = null,
            AddressSettings addressSettings = null);

        Task PrepareVendorAddressModel(VendorAddressModel model,
            Address address, bool excludeProperties,
            Func<IList<Country>> loadCountries = null,
            bool prePopulateWithCustomerFields = false,
            Customer customer = null,
            VendorSettings vendorSettings = null);

        Task PrepareCustomAddressAttributes(AddressModel model,
            Address address, string overrideAttributesXml = "");

        Task<string> ParseCustomAddressAttributes(IFormCollection form);

        AddressSettings AddressSettings();
        Task<IList<string>> GetAttributeWarnings(string attributesXml);
    }
}