using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Directory;
using Grand.Domain.Localization;
using Grand.Domain.Orders;
using Grand.Domain.Stores;
using Grand.Domain.Tax;
using Grand.Web.Models.ShoppingCart;
using MediatR;
using System;

namespace Grand.Web.Features.Models.ShoppingCart
{
    public class GetAddToCart : IRequest<AddToCartModel>
    {
        public Product Product { get; set; }
        public Customer Customer { get; set; }
        public Language Language { get; set; }
        public Currency Currency { get; set; }
        public Store Store { get; set; }
        public TaxDisplayType TaxDisplayType { get; set; }
        public int Quantity { get; set; }
        public decimal CustomerEnteredPrice { get; set; }
        public string AttributesXml { get; set; }
        public ShoppingCartType CartType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ReservationId { get; set; }
        public string Parameter { get; set; }
        public string Duration { get; set; }
    }
}
