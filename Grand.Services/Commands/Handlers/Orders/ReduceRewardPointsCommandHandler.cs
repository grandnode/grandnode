﻿using Grand.Services.Commands.Models.Orders;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Orders;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Commands.Handlers.Orders
{
    public class ReduceRewardPointsCommandHandler : IRequestHandler<ReduceRewardPointsCommand, bool>
    {
        private readonly ICustomerService _customerService;
        private readonly IRewardPointsService _rewardPointsService;
        private readonly IMediator _mediator;
        private readonly IOrderService _orderService;
        private readonly ILocalizationService _localizationService;
        private readonly ICurrencyService _currencyService;

        public ReduceRewardPointsCommandHandler(
            ICustomerService customerService,
            IRewardPointsService rewardPointsService,
            IMediator mediator,
            IOrderService orderService,
            ILocalizationService localizationService,
            ICurrencyService currencyService)
        {
            _customerService = customerService;
            _rewardPointsService = rewardPointsService;
            _mediator = mediator;
            _orderService = orderService;
            _localizationService = localizationService;
            _currencyService = currencyService;
        }

        public async Task<bool> Handle(ReduceRewardPointsCommand request, CancellationToken cancellationToken)
        {
            if (request.Order == null)
                throw new ArgumentNullException("order");

            var customer = await _customerService.GetCustomerById(request.Order.CustomerId);

            var currency = await _currencyService.GetCurrencyByCode(request.Order.CustomerCurrencyCode);

            var amount = await _currencyService.ConvertToPrimaryStoreCurrency(request.Order.OrderTotal - request.Order.OrderShippingInclTax, currency);

            int points = await _mediator.Send(new CalculateRewardPointsCommand() { Customer = customer, Amount = amount });
            if (points <= 0)
                return false;
            
            if(request.Order.CalcRewardPoints < points)
                return false;

            //reduce reward points
            await _rewardPointsService.AddRewardPointsHistory(customer.Id, -points, request.Order.StoreId,
                string.Format(_localizationService.GetResource("RewardPoints.Message.ReducedForOrder"), request.Order.OrderNumber));

            request.Order.CalcRewardPoints -= points;
            //assign to order
            await _orderService.UpdateOrder(request.Order);

            return true;
        }
    }
}
