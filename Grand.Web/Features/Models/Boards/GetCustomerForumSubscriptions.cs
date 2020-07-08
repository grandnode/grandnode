using Grand.Domain.Customers;
using Grand.Web.Models.Boards;
using MediatR;

namespace Grand.Web.Features.Models.Boards
{
    public class GetCustomerForumSubscriptions : IRequest<CustomerForumSubscriptionsModel>
    {
        public Customer Customer { get; set; }
        public int PageIndex { get; set; }
    }
}
