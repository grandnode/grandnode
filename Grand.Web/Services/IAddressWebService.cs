using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Directory;
using Grand.Services.Common;
using Grand.Web.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

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

        void PrepareCustomAddressAttributes(AddressModel model,
            Address address, string overrideAttributesXml = "");

        string ParseCustomAddressAttributes(IFormCollection form);

        AddressSettings AddressSettings();
        IList<string> GetAttributeWarnings(string attributesXml);
    }
}