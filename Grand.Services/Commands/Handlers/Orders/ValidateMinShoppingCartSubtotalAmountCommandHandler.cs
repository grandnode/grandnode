using Grand.Domain.Orders;
using Grand.Services.Commands.Models.Orders;
using Grand.Services.Orders;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Commands.Handlers.Orders
{
    public class ValidateMinShoppingCartSubtotalAmountCommandHandler : IRequestHandler<ValidateMinShoppingCartSubtotalAmountCommand, bool>
    {
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly OrderSettings _orderSettings;

        public ValidateMinShoppingCartSubtotalAmountCommandHandler(
            IOrderTotalCalculationService orderTotalCalculationService,
            OrderSettings orderSettings)
        {
            _orderTotalCalculationService = orderTotalCalculationService;
            _orderSettings = orderSettings;
        }

        public async Task<bool> Handle(ValidateMinShoppingCartSubtotalAmountCommand request, CancellationToken cancellationToken)
        {
            if (request.Cart == null)
                throw new ArgumentNullException("cart");

            if (request.Customer == null)
                throw new ArgumentNullException("customer");

            return await ValidateMinOrderSubtotalAmount(request.Cart);
        }

        protected virtual async Task<bool> ValidateMinOrderSubtotalAmount(IList<ShoppingCartItem> cart)
        {
            if (cart == null)
                throw new ArgumentNullException("cart");

            if (!cart.Any())
                return false;

            //min order amount sub-total validation
            if (cart.Any() && _orderSettings.MinOrderSubtotalAmount > decimal.Zero)
            {
                //subtotal
                var (_, _, subTotalWithoutDiscount, _, _) = await _orderTotalCalculationService.GetShoppingCartSubTotal(cart, false);
                if (subTotalWithoutDiscount < _orderSettings.MinOrderSubtotalAmount)
                    return false;
            }

            return true;
        }

    }
}
