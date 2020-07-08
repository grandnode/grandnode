using Grand.Domain.Catalog;
using Grand.Services.Messages.DotLiquidDrops;
using MediatR;

namespace Grand.Services.Commands.Models.Messages
{
    public class GetAuctionTokensCommand : IRequest<LiquidAuctions>
    {
        public Product Product { get; set; }
        public Bid Bid { get; set; }
    }
}
