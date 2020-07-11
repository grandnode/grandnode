using Grand.Domain.Orders;
using Grand.Services.Messages.DotLiquidDrops;
using MediatR;

namespace Grand.Services.Commands.Models.Messages
{
    public class GetGiftCardTokensCommand : IRequest<LiquidGiftCard>
    {
        public GiftCard GiftCard { get; set; }
    }
}
