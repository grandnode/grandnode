using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Directory;
using Grand.Domain.Localization;
using Grand.Web.Models.Vendors;
using MediatR;
using System;
using System.Collections.Generic;

namespace Grand.Web.Features.Models.Common
{
    public class GetVendorAddress : IRequest<VendorAddressModel>
    {
        public VendorAddressModel Model { get; set; }
        public Address Address { get; set; }
        public bool ExcludeProperties { get; set; }
        public Func<IList<Country>> LoadCountries { get; set; } = null;
        public bool PrePopulateWithCustomerFields { get; set; } = false;
        public Customer Customer { get; set; } = null;
        public Language Language { get; set; }
    }
}
