using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Stores;
using Grand.Core.Domain.Tax;
using Grand.Web.Models.ShoppingCart;
using MediatR;

namespace Grand.Web.Features.Models.ShoppingCart
{
    public class GetMiniShoppingCart : IRequest<MiniShoppingCartModel>
    {
        public Customer Customer { get; set; }
        public Language Language { get; set; }
        public Currency Currency { get; set; }
        public Store Store { get; set; }
        public TaxDisplayType TaxDisplayType { get; set; }
    }
}
