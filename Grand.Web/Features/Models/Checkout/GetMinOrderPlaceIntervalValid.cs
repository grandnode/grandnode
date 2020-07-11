using Grand.Domain.Customers;
using Grand.Domain.Stores;
using MediatR;

namespace Grand.Web.Features.Models.Checkout
{
    public class GetMinOrderPlaceIntervalValid : IRequest<bool>
    {
        public Customer Customer { get; set; }
        public Store Store { get; set; }
    }
}
