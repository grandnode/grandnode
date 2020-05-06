using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Stores;
using Grand.Services.Messages.DotLiquidDrops;
using MediatR;

namespace Grand.Services.Commands.Models.Messages
{
    public class GetReturnRequestTokensCommand : IRequest<LiquidReturnRequest>
    {
        public ReturnRequest ReturnRequest { get; set; }
        public Store Store { get; set; }
        public Order Order { get; set; }
        public Language Language { get; set; }
        public ReturnRequestNote ReturnRequestNote { get; set; } = null;
    }
}
