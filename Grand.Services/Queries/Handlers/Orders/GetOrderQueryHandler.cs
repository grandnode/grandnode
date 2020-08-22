using Grand.Domain.Data;
using Grand.Domain.Orders;
using Grand.Services.Queries.Models.Orders;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Queries.Handlers.Orders
{
    public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, IMongoQueryable<Order>>
    {
        private readonly IRepository<Order> _orderRepository;

        public GetOrderQueryHandler(IRepository<Order> orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public Task<IMongoQueryable<Order>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
        {
            int? orderStatusId = null;

            if (request.Os.HasValue)
                orderStatusId = (int)request.Os.Value;

            int? paymentStatusId = null;
            if (request.Ps.HasValue)
                paymentStatusId = (int)request.Ps.Value;

            int? shippingStatusId = null;
            if (request.Ss.HasValue)
                shippingStatusId = (int)request.Ss.Value;

            var query = _orderRepository.Table;
            if (!string.IsNullOrEmpty(request.OrderId))
                query = query.Where(o => o.Id == request.OrderId);

            if (!string.IsNullOrEmpty(request.StoreId))
                query = query.Where(o => o.StoreId == request.StoreId);

            if (!string.IsNullOrEmpty(request.VendorId))
            {
                query = query
                    .Where(o => o.OrderItems
                    .Any(orderItem => orderItem.VendorId == request.VendorId));
            }

            if (!string.IsNullOrEmpty(request.CustomerId))
                query = query.Where(o => o.CustomerId == request.CustomerId);

            if (!string.IsNullOrEmpty(request.ProductId))
            {
                query = query
                    .Where(o => o.OrderItems
                    .Any(orderItem => orderItem.ProductId == request.ProductId));
            }
            if (!string.IsNullOrEmpty(request.WarehouseId))
            {
                query = query
                    .Where(o => o.OrderItems
                    .Any(orderItem =>
                        orderItem.WarehouseId == request.WarehouseId
                        ));
            }
            if (!string.IsNullOrEmpty(request.BillingCountryId))
                query = query.Where(o => o.BillingAddress != null && o.BillingAddress.CountryId == request.BillingCountryId);

            if (!string.IsNullOrEmpty(request.PaymentMethodSystemName))
                query = query.Where(o => o.PaymentMethodSystemName == request.PaymentMethodSystemName);

            if (!string.IsNullOrEmpty(request.AffiliateId))
                query = query.Where(o => o.AffiliateId == request.AffiliateId);

            if (!string.IsNullOrEmpty(request.OwnerId))
                query = query.Where(o => o.OwnerId == request.OwnerId);

            if (request.CreatedFromUtc.HasValue)
                query = query.Where(o => request.CreatedFromUtc.Value <= o.CreatedOnUtc);

            if (request.CreatedToUtc.HasValue)
                query = query.Where(o => request.CreatedToUtc.Value >= o.CreatedOnUtc);

            if (orderStatusId.HasValue)
                query = query.Where(o => orderStatusId.Value == o.OrderStatusId);

            if (paymentStatusId.HasValue)
                query = query.Where(o => paymentStatusId.Value == o.PaymentStatusId);

            if (shippingStatusId.HasValue)
                query = query.Where(o => shippingStatusId.Value == o.ShippingStatusId);

            if (!string.IsNullOrEmpty(request.BillingEmail))
                query = query.Where(o => o.BillingAddress != null && o.BillingAddress.Email == request.BillingEmail);

            if (!string.IsNullOrEmpty(request.BillingLastName))
                query = query.Where(o => o.BillingAddress != null && o.BillingAddress.LastName.Contains(request.BillingLastName));

            if (!string.IsNullOrEmpty(request.OrderGuid))
            {
                if(Guid.TryParse(request.OrderGuid, out Guid orderguid))
                    query = query.Where(o => o.OrderGuid == orderguid); 
            }
            if (!string.IsNullOrEmpty(request.OrderCode))
            {
                query = query.Where(o => o.Code == request.OrderCode.ToUpperInvariant());
            }

            //tag filtering 
            if (!string.IsNullOrEmpty(request.OrderTagId))
                query = query.Where(o => o.OrderTags.Any(y => y == request.OrderTagId));

            query = query.Where(o => !o.Deleted);
            query = query.OrderByDescending(o => o.CreatedOnUtc);

            return Task.FromResult(query);
        }
    }
}
