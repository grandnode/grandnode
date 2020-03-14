using Grand.Core.Domain.Customers;
using MediatR;

namespace Grand.Services.Commands.Models.Orders
{
    public class ApplyGiftCardCommand : IRequest
    {
        public string GiftCardCouponCode { get; set; }
        public Customer Customer { get; set; }
    }
}
