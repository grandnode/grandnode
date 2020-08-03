using Grand.Domain.Orders;
using Grand.Services.Orders;
using Grand.Web.Features.Models.Checkout;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Checkout
{
    public class GetMinOrderPlaceIntervalValidHandler : IRequestHandler<GetMinOrderPlaceIntervalValid, bool>
    {
        private readonly OrderSettings _orderSettings;
        private readonly IOrderService _orderService;

        public GetMinOrderPlaceIntervalValidHandler(IOrderService orderService,
            OrderSettings orderSettings)
        {
            _orderSettings = orderSettings;
            _orderService = orderService;
        }

        public async Task<bool> Handle(GetMinOrderPlaceIntervalValid request, CancellationToken cancellationToken)
        {
            if (_orderSettings.MinimumOrderPlacementInterval == 0)
                return true;

            var lastOrder = (await _orderService.SearchOrders(storeId: request.Store.Id,
                customerId: request.Customer.Id, pageSize: 1))
                .FirstOrDefault();
            if (lastOrder == null)
                return true;

            var interval = DateTime.UtcNow - lastOrder.CreatedOnUtc;
            return interval.TotalSeconds > _orderSettings.MinimumOrderPlacementInterval;
        }
    }
}
