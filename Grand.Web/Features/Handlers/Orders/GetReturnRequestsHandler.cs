using Grand.Core;
using Grand.Core.Domain.Tax;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Orders;
using Grand.Web.Features.Models.Orders;
using Grand.Web.Models.Orders;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Orders
{
    public class GetReturnRequestsHandler : IRequestHandler<GetReturnRequests, CustomerReturnRequestsModel>
    {
        private readonly IReturnRequestService _returnRequestService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IOrderService _orderService;
        private readonly ICurrencyService _currencyService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPriceFormatter _priceFormatter;

        public GetReturnRequestsHandler(IReturnRequestService returnRequestService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IOrderService orderService,
            ICurrencyService currencyService,
            ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper,
            IPriceFormatter priceFormatter)
        {
            _returnRequestService = returnRequestService;
            _workContext = workContext;
            _storeContext = storeContext;
            _orderService = orderService;
            _currencyService = currencyService;
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;
            _priceFormatter = priceFormatter;
        }

        public async Task<CustomerReturnRequestsModel> Handle(GetReturnRequests request, CancellationToken cancellationToken)
        {
            var model = new CustomerReturnRequestsModel();

            var returnRequests = await _returnRequestService.SearchReturnRequests(_storeContext.CurrentStore.Id, _workContext.CurrentCustomer.Id);
            foreach (var returnRequest in returnRequests)
            {
                var order = await _orderService.GetOrderById(returnRequest.OrderId);
                decimal total = 0;
                foreach (var rrItem in returnRequest.ReturnRequestItems)
                {
                    var orderItem = order.OrderItems.Where(x => x.Id == rrItem.OrderItemId).First();

                    if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                    {
                        //including tax
                        var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                        total += unitPriceInclTaxInCustomerCurrency * rrItem.Quantity;
                    }
                    else
                    {
                        //excluding tax
                        var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                        total += unitPriceExclTaxInCustomerCurrency * rrItem.Quantity;
                    }
                }

                var itemModel = new CustomerReturnRequestsModel.ReturnRequestModel {
                    Id = returnRequest.Id,
                    ReturnNumber = returnRequest.ReturnNumber,
                    ReturnRequestStatus = returnRequest.ReturnRequestStatus.GetLocalizedEnum(_localizationService, _workContext),
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(returnRequest.CreatedOnUtc, DateTimeKind.Utc),
                    ProductsCount = returnRequest.ReturnRequestItems.Sum(x => x.Quantity),
                    ReturnTotal = _priceFormatter.FormatPrice(total)
                };

                model.Items.Add(itemModel);
            }

            return model;
        }
    }
}
