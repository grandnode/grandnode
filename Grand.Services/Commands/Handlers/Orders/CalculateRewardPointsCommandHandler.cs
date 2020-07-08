using Grand.Domain.Customers;
using Grand.Services.Commands.Models.Orders;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Commands.Handlers.Orders
{
    public class CalculateRewardPointsCommandHandler : IRequestHandler<CalculateRewardPointsCommand, int>
    {
        private readonly RewardPointsSettings _rewardPointsSettings;

        public CalculateRewardPointsCommandHandler(RewardPointsSettings rewardPointsSettings)
        {
            _rewardPointsSettings = rewardPointsSettings;
        }

        public async Task<int> Handle(CalculateRewardPointsCommand request, CancellationToken cancellationToken)
        {
            if (!_rewardPointsSettings.Enabled)
                return 0;

            if (_rewardPointsSettings.PointsForPurchases_Amount <= decimal.Zero)
                return 0;

            //Ensure that reward points are applied only to registered users
            if (request.Customer == null || request.Customer.IsGuest())
                return 0;

            var points = (int)Math.Truncate(request.Amount / _rewardPointsSettings.PointsForPurchases_Amount * _rewardPointsSettings.PointsForPurchases_Points);

            return await Task.FromResult(points);
        }
    }
}
