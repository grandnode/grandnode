using Grand.Domain.Customers;
using Grand.Domain.Localization;
using Grand.Domain.Stores;
using Grand.Services.Messages.DotLiquidDrops;
using MediatR;
namespace Grand.Services.Commands.Models.Messages
{
    public class GetShoppingCartTokensCommand : IRequest<LiquidShoppingCart>
    {
        public Customer Customer { get; set; }
        public Store Store { get; set; }
        public Language Language { get; set; }
        public string PersonalMessage { get; set; } = "";
        public string CustomerEmail { get; set; } = "";
    }
}
