using Grand.Services.Orders;
using Grand.Web.Features.Models.Checkout;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Checkout
{
    public class GetIsPaymentWorkflowRequiredHandler : IRequestHandler<GetIsPaymentWorkflowRequired, bool>
    {
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;

        public GetIsPaymentWorkflowRequiredHandler(IOrderTotalCalculationService orderTotalCalculationService)
        {
            _orderTotalCalculationService = orderTotalCalculationService;
        }
        public async Task<bool> Handle(GetIsPaymentWorkflowRequired request, CancellationToken cancellationToken)
        {
            bool result = true;
            //check whether order total equals zero
            decimal? shoppingCartTotalBase = (await _orderTotalCalculationService.GetShoppingCartTotal(request.Cart, useRewardPoints: request.UseRewardPoints)).shoppingCartTotal;
            if (shoppingCartTotalBase.HasValue && shoppingCartTotalBase.Value == decimal.Zero)
                result = false;
            return result;
        }
    }
}
