using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Directory;
using Grand.Domain.Localization;
using Grand.Domain.Stores;
using Grand.Web.Models.Common;
using MediatR;
using System;
using System.Collections.Generic;
namespace Grand.Web.Features.Models.Common
{
    public class GetAddressModel : IRequest<AddressModel>
    {
        public AddressModel Model { get; set; }
        public Address Address { get; set; }
        public bool ExcludeProperties { get; set; }
        public Func<IList<Country>> LoadCountries { get; set; } = null;
        public bool PrePopulateWithCustomerFields { get; set; } = false;
        public Customer Customer { get; set; } = null;
        public Language Language { get; set; }
        public Store Store { get; set; }
        public IList<CustomAttribute> OverrideAttributes { get; set; }
    }
}
