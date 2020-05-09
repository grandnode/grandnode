using Grand.Services.Commands.Models.Orders;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Orders;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Commands.Handlers.Orders
{
    public class AwardRewardPointsCommandHandler : IRequestHandler<AwardRewardPointsCommand, bool>
    {
        private readonly ICustomerService _customerService;
        private readonly IRewardPointsService _rewardPointsService;
        private readonly IMediator _mediator;
        private readonly IOrderService _orderService;
        private readonly ILocalizationService _localizationService;

        public AwardRewardPointsCommandHandler(
            ICustomerService customerService, 
            IRewardPointsService rewardPointsService, 
            IMediator mediator,
            IOrderService orderService,
            ILocalizationService localizationService)
        {
            _customerService = customerService;
            _rewardPointsService = rewardPointsService;
            _mediator = mediator;
            _orderService = orderService;
            _localizationService = localizationService;
        }

        public async Task<bool> Handle(AwardRewardPointsCommand request, CancellationToken cancellationToken)
        {
            if (request.Order == null)
                throw new ArgumentNullException("order");

            var customer = await _customerService.GetCustomerById(request.Order.CustomerId);

            int points = await _mediator.Send(new CalculateRewardPointsCommand() { Customer = customer, Amount = request.Order.OrderTotal - request.Order.OrderShippingInclTax });
            if (points <= 0)
                return false;
            
            //add reward points
            await _rewardPointsService.AddRewardPointsHistory(customer.Id, points,
                request.Order.StoreId, string.Format(_localizationService.GetResource("RewardPoints.Message.EarnedForOrder"), request.Order.OrderNumber));

            request.Order.CalcRewardPoints += points;
            //assign to order
            await _orderService.UpdateOrder(request.Order);

            return true;

        }
    }
}
