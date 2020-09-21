using Grand.Domain.Customers;
using Grand.Web.Models.Customer;
using MediatR;
using System.Collections.Generic;

namespace Grand.Web.Features.Models.Customers
{
    public class GetSubAccounts : IRequest<IList<SubAccountSimpleModel>>
    {
        public Customer Customer { get; set; }
    }
}
