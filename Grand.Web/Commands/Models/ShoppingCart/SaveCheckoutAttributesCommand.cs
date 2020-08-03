using Grand.Domain.Customers;
using Grand.Domain.Orders;
using Grand.Domain.Stores;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Grand.Web.Commands.Models.ShoppingCart
{
    public class SaveCheckoutAttributesCommand : IRequest<string>
    {
        public Customer Customer { get; set; }
        public Store Store { get; set; }

        public IList<ShoppingCartItem> Cart { get; set; }
        public IFormCollection Form { get; set; }
    }
}
