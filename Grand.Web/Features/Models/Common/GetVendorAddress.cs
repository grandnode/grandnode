using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Localization;
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
