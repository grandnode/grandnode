using Grand.Domain.Customers;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Orders;
using Grand.Web.Features.Models.Orders;
using Grand.Web.Models.Orders;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Newsletter
{
    public class GetCustomerRewardPointsHandler : IRequestHandler<GetCustomerRewardPoints, CustomerRewardPointsModel>
    {
        private readonly IRewardPointsService _rewardPointsService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly RewardPointsSettings _rewardPointsSettings;

        public GetCustomerRewardPointsHandler(IRewardPointsService rewardPointsService, IDateTimeHelper dateTimeHelper,
            ICurrencyService currencyService, IPriceFormatter priceFormatter, IOrderTotalCalculationService orderTotalCalculationService,
            RewardPointsSettings rewardPointsSettings)
        {
            _rewardPointsService = rewardPointsService;
            _dateTimeHelper = dateTimeHelper;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _orderTotalCalculationService = orderTotalCalculationService;
            _rewardPointsSettings = rewardPointsSettings;
        }

        public async Task<CustomerRewardPointsModel> Handle(GetCustomerRewardPoints request, CancellationToken cancellationToken)
        {
            var model = new CustomerRewardPointsModel();
            foreach (var rph in await _rewardPointsService.GetRewardPointsHistory(request.Customer.Id, request.Store.Id))
            {
                model.RewardPoints.Add(new CustomerRewardPointsModel.RewardPointsHistoryModel {
                    Points = rph.Points,
                    PointsBalance = rph.PointsBalance,
                    Message = rph.Message,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(rph.CreatedOnUtc, DateTimeKind.Utc)
                });
            }
            //current amount/balance
            int rewardPointsBalance = await _rewardPointsService.GetRewardPointsBalance(request.Customer.Id, request.Store.Id);
            decimal rewardPointsAmountBase = await _orderTotalCalculationService.ConvertRewardPointsToAmount(rewardPointsBalance);
            decimal rewardPointsAmount = await _currencyService.ConvertFromPrimaryStoreCurrency(rewardPointsAmountBase, request.Currency);
            model.RewardPointsBalance = rewardPointsBalance;
            model.RewardPointsAmount = _priceFormatter.FormatPrice(rewardPointsAmount, true, false);
            //minimum amount/balance
            int minimumRewardPointsBalance = _rewardPointsSettings.MinimumRewardPointsToUse;
            decimal minimumRewardPointsAmountBase = await _orderTotalCalculationService.ConvertRewardPointsToAmount(minimumRewardPointsBalance);
            decimal minimumRewardPointsAmount = await _currencyService.ConvertFromPrimaryStoreCurrency(minimumRewardPointsAmountBase, request.Currency);
            model.MinimumRewardPointsBalance = minimumRewardPointsBalance;
            model.MinimumRewardPointsAmount = _priceFormatter.FormatPrice(minimumRewardPointsAmount, true, false);

            return model;
        }
    }
}
