using Grand.Services.Orders;
using Grand.Web.Features.Models.Courses;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Courses
{
    public class GetCheckOrderHandler : IRequestHandler<GetCheckOrder, bool>
    {
        private readonly IOrderService _orderService;

        public GetCheckOrderHandler(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<bool> Handle(GetCheckOrder request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Course.ProductId))
                return true;

            var orders = await _orderService.SearchOrders(customerId: request.Customer.Id, productId: request.Course.ProductId, ps: Domain.Payments.PaymentStatus.Paid);
            if (orders.TotalCount > 0)
                return true;

            return false;
        }
    }
}
