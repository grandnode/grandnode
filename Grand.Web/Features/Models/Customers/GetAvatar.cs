using Grand.Domain.Customers;
using Grand.Web.Models.Customer;
using MediatR;

namespace Grand.Web.Features.Models.Customers
{
    public class GetAvatar : IRequest<CustomerAvatarModel>
    {
        public Customer Customer { get; set; }
    }
}
