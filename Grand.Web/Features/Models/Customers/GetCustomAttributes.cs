using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Localization;
using Grand.Web.Models.Customer;
using MediatR;
using System.Collections.Generic;

namespace Grand.Web.Features.Models.Customers
{
    public class GetCustomAttributes : IRequest<IList<CustomerAttributeModel>>
    {
        public Customer Customer { get; set; }
        public Language Language { get; set; }
        public IList<CustomAttribute> OverrideAttributes { get; set; }
    }
}
