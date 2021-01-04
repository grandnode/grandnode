using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Localization;
using Grand.Domain.Stores;
using Grand.Web.Models.Customer;
using MediatR;
using System.Collections.Generic;

namespace Grand.Web.Features.Models.Customers
{
    public class GetRegister : IRequest<RegisterModel>
    {
        public Customer Customer { get; set; }
        public Store Store { get; set; }
        public Language Language { get; set; }
        public RegisterModel Model { get; set; }
        public bool ExcludeProperties { get; set; }
        public IList<CustomAttribute> OverrideCustomCustomerAttributes { get; set; }
    }
}
