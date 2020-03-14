using Grand.Core.Domain.Customers;
using MediatR;

namespace Grand.Services.Commands.Models.Orders
{
    public class RemoveGiftCardCommand : IRequest
    {
        public string GiftCardId { get; set; }
        public Customer Customer { get; set; }
    }
}
