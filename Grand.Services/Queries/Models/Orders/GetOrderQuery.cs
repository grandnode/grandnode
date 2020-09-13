using Grand.Domain.Orders;
using Grand.Domain.Payments;
using Grand.Domain.Shipping;
using MediatR;
using MongoDB.Driver.Linq;
using System;

namespace Grand.Services.Queries.Models.Orders
{
    public class GetOrderQuery : IRequest<IMongoQueryable<Order>>
    {
        public string OrderId { get; set; } = "";
        public string StoreId { get; set; } = "";
        public string VendorId { get; set; } = "";
        public string CustomerId { get; set; } = "";
        public string ProductId { get; set; } = "";
        public string AffiliateId { get; set; } = "";
        public string WarehouseId { get; set; } = "";
        public string BillingCountryId { get; set; } = "";
        public string OwnerId { get; set; } = "";
        public string PaymentMethodSystemName { get; set; } = null;
        public DateTime? CreatedFromUtc { get; set; } = null;
        public DateTime? CreatedToUtc { get; set; } = null;
        public OrderStatus? Os { get; set; } = null;
        public PaymentStatus? Ps { get; set; } = null;
        public ShippingStatus? Ss { get; set; } = null;
        public string BillingEmail { get; set; } = null;
        public string BillingLastName { get; set; } = "";
        public string OrderGuid { get; set; } = null;
        public string OrderCode { get; set; } = null;
        public int PageIndex { get; set; } = 0;
        public int PageSize { get; set; } = int.MaxValue;
        public string OrderTagId { get; set; } = "";
    }
}
