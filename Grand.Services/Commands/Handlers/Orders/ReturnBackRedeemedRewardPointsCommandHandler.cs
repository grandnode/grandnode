using Grand.Services.Commands.Models.Orders;
using Grand.Services.Localization;
using Grand.Services.Orders;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Commands.Handlers.Orders
{
    public class ReturnBackRedeemedRewardPointsCommandHandler : IRequestHandler<ReturnBackRedeemedRewardPointsCommand, bool>
    {
        private readonly IRewardPointsService _rewardPointsService;
        private readonly ILocalizationService _localizationService;

        public ReturnBackRedeemedRewardPointsCommandHandler(
            IRewardPointsService rewardPointsService,
            ILocalizationService localizationService)
        {
            _rewardPointsService = rewardPointsService;
            _localizationService = localizationService;
        }

        public async Task<bool> Handle(ReturnBackRedeemedRewardPointsCommand request, CancellationToken cancellationToken)
        {
            if (request.Order == null)
                throw new ArgumentNullException("customer");

            //were some points redeemed when placing an order?
            if (request.Order.RedeemedRewardPointsEntry == null)
                return false;

            //return back
            await _rewardPointsService.AddRewardPointsHistory(request.Order.CustomerId, -request.Order.RedeemedRewardPointsEntry.Points, request.Order.StoreId,
                string.Format(_localizationService.GetResource("RewardPoints.Message.ReturnedForOrder"), request.Order.OrderNumber));

            return true;
        }
    }
}
