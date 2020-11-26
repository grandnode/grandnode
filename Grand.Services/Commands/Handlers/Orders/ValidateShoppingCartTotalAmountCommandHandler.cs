using Grand.Services.Commands.Models.Orders;
using Grand.Services.Orders;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Commands.Handlers.Orders
{
    public class ValidateShoppingCartTotalAmountCommandHandler : IRequestHandler<ValidateShoppingCartTotalAmountCommand, bool>
    {
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;

        public ValidateShoppingCartTotalAmountCommandHandler(IOrderTotalCalculationService orderTotalCalculationService)
        {
            _orderTotalCalculationService = orderTotalCalculationService;
        }

        public async Task<bool> Handle(ValidateShoppingCartTotalAmountCommand request, CancellationToken cancellationToken)
        {
            if (request.Cart == null)
                throw new ArgumentNullException("cart");

            if (request.Customer == null)
                throw new ArgumentNullException("customer");

            var minroles = request.Customer.CustomerRoles.OrderBy(x => x.MinOrderAmount).FirstOrDefault(x => x.Active && x.MinOrderAmount.HasValue);
            var minOrderAmount = minroles?.MinOrderAmount ?? decimal.MinValue;

            var maxroles = request.Customer.CustomerRoles.OrderByDescending(x => x.MaxOrderAmount).FirstOrDefault(x => x.Active && x.MaxOrderAmount.HasValue);
            var maxOrderAmount = maxroles?.MaxOrderAmount ?? decimal.MaxValue;

            if (request.Cart.Any() && (minOrderAmount > decimal.Zero || maxOrderAmount > decimal.Zero))
            {
                decimal? shoppingCartTotalBase = (await _orderTotalCalculationService.GetShoppingCartTotal(request.Cart)).shoppingCartTotal;
                if (shoppingCartTotalBase.HasValue && (shoppingCartTotalBase.Value < minOrderAmount || shoppingCartTotalBase.Value > maxOrderAmount))
                    return false;
            }

            return true;
        }
    }
}
