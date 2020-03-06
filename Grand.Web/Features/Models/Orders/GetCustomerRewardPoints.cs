using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Stores;
using Grand.Web.Models.Orders;
using MediatR;

namespace Grand.Web.Features.Models.Orders
{
    public class GetCustomerRewardPoints : IRequest<CustomerRewardPointsModel>
    {
        public Customer Customer { get; set; }
        public Store Store { get; set; }
        public Currency Currency { get; set; }
    }
}
