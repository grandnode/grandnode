using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Stores;
using Grand.Core.Domain.Vendors;
using Grand.Services.Messages.DotLiquidDrops;
using MediatR;

namespace Grand.Services.Commands.Models.Messages
{
    public class GetOrderTokensCommand : IRequest<LiquidOrder>
    {
        public Order Order { get; set; }
        public Customer Customer { get; set; }
        public Store Store { get; set; }
        public OrderNote OrderNote { get; set; } = null;
        public Vendor Vendor { get; set; } = null;
        public decimal RefundedAmount { get; set; } = 0;
    }
}
