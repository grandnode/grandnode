using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Directory;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Stores;
using Grand.Web.Models.ShoppingCart;
using MediatR;
using System.Collections.Generic;

namespace Grand.Web.Features.Models.ShoppingCart
{
    public class GetEstimateShippingResult : IRequest<EstimateShippingResultModel>
    {
        public Customer Customer { get; set; }
        public Store Store { get; set; }
        public Currency Currency { get; set; }

        public IList<ShoppingCartItem> Cart { get; set; }
        public string CountryId { get; set; }
        public string StateProvinceId { get; set; }
        public string ZipPostalCode { get; set; }
    }
}
