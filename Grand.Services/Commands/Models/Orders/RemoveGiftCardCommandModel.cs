using Grand.Core.Domain.Customers;
using MediatR;

namespace Grand.Services.Commands.Models.Orders
{
    public class RemoveGiftCardCommandModel : IRequest
    {
        public string GiftCardId { get; set; }
        public Customer Customer { get; set; }
    }
}
