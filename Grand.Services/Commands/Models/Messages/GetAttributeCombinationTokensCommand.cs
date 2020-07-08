using Grand.Domain.Catalog;
using Grand.Services.Messages.DotLiquidDrops;
using MediatR;

namespace Grand.Services.Commands.Models.Messages
{
    public class GetAttributeCombinationTokensCommand : IRequest<LiquidAttributeCombination>
    {
        public Product Product { get; set; }
        public ProductAttributeCombination Combination { get; set; }
    }
}
