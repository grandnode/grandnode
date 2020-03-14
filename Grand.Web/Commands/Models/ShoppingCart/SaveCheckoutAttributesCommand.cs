using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Stores;
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
