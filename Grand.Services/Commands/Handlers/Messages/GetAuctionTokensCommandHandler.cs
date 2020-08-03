using Grand.Services.Catalog;
using Grand.Services.Commands.Models.Messages;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Messages.DotLiquidDrops;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Commands.Handlers.Messages
{
    public class GetAuctionTokensCommandHandler : IRequestHandler<GetAuctionTokensCommand, LiquidAuctions>
    {
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IDateTimeHelper _dateTimeHelper;

        public GetAuctionTokensCommandHandler(
            ICurrencyService currencyService, 
            IPriceFormatter priceFormatter, 
            IDateTimeHelper dateTimeHelper)
        {
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _dateTimeHelper = dateTimeHelper;
        }

        public async Task<LiquidAuctions> Handle(GetAuctionTokensCommand request, CancellationToken cancellationToken)
        {
            var liquidAuctions = new LiquidAuctions(request.Product, request.Bid);
            var defaultCurrency = await _currencyService.GetPrimaryStoreCurrency();
            liquidAuctions.Price = _priceFormatter.FormatPrice(request.Bid.Amount, true, defaultCurrency);
            liquidAuctions.EndTime = _dateTimeHelper.ConvertToUserTime(request.Product.AvailableEndDateTimeUtc.Value, DateTimeKind.Utc).ToString();
            return liquidAuctions;
        }
    }
}
