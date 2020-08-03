using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Seo;
using Grand.Web.Features.Models.Customers;
using Grand.Web.Models.Customer;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Customers
{
    public class GetAuctionsHandler : IRequestHandler<GetAuctions, CustomerAuctionsModel>
    {
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICurrencyService _currencyService;
        private readonly IAuctionService _auctionService;
        private readonly IProductService _productService;
        private readonly IDateTimeHelper _dateTimeHelper;

        public GetAuctionsHandler(IPriceFormatter priceFormatter, 
            ICurrencyService currencyService, 
            IAuctionService auctionService, 
            IProductService productService, 
            IDateTimeHelper dateTimeHelper)
        {
            _priceFormatter = priceFormatter;
            _currencyService = currencyService;
            _auctionService = auctionService;
            _productService = productService;
            _dateTimeHelper = dateTimeHelper;
        }

        public async Task<CustomerAuctionsModel> Handle(GetAuctions request, CancellationToken cancellationToken)
        {
            var model = new CustomerAuctionsModel();
            var primaryCurrency = await _currencyService.GetPrimaryStoreCurrency();

            var customerBids = (await _auctionService.GetBidsByCustomerId(request.Customer.Id)).GroupBy(x => x.ProductId);
            foreach (var item in customerBids)
            {
                var product = await _productService.GetProductById(item.Key);
                if (product != null)
                {
                    var bid = new ProductBidTuple();
                    bid.Ended = product.AuctionEnded;
                    bid.OrderId = item.Where(x => x.Win && x.CustomerId == request.Customer.Id).FirstOrDefault()?.OrderId;
                    var amount = product.HighestBid;
                    bid.CurrentBidAmount = _priceFormatter.FormatPrice(amount, true, primaryCurrency);
                    bid.CurrentBidAmountValue = amount;
                    bid.HighestBidder = product.HighestBidder == request.Customer.Id;
                    bid.EndBidDate = product.AvailableEndDateTimeUtc.HasValue ? _dateTimeHelper.ConvertToUserTime(product.AvailableEndDateTimeUtc.Value, DateTimeKind.Utc) : DateTime.MaxValue;
                    bid.ProductName = product.GetLocalized(x => x.Name, request.Language.Id);
                    bid.ProductSeName = product.GetSeName(request.Language.Id);
                    bid.BidAmountValue = item.Max(x => x.Amount);
                    bid.BidAmount = _priceFormatter.FormatPrice(bid.BidAmountValue, true, primaryCurrency);
                    model.ProductBidList.Add(bid);
                }
            }

            model.CustomerId = request.Customer.Id;

            return model;
        }
    }
}
