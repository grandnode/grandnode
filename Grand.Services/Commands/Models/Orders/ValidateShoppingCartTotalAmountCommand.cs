using Grand.Domain.Customers;
using Grand.Domain.Orders;
using MediatR;
using System.Collections.Generic;

namespace Grand.Services.Commands.Models.Orders
{
    public class ValidateShoppingCartTotalAmountCommand : IRequest<bool>
    {
        public Customer Customer { get; set; }
        public IList<ShoppingCartItem> Cart { get; set; }
    }
}
