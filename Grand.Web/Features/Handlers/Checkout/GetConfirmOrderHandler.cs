using Grand.Core.Domain.Orders;
using Grand.Services.Catalog;
using Grand.Services.Directory;
using Grand.Services.Localization;
using Grand.Services.Orders;
using Grand.Web.Features.Models.Checkout;
using Grand.Web.Models.Checkout;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Checkout
{
    public class GetConfirmOrderHandler : IRequestHandler<GetConfirmOrder, CheckoutConfirmModel>
    {
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ILocalizationService _localizationService;

        private readonly OrderSettings _orderSettings;

        public GetConfirmOrderHandler(IOrderProcessingService orderProcessingService,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            ILocalizationService localizationService,
            OrderSettings orderSettings)
        {
            _orderProcessingService = orderProcessingService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _localizationService = localizationService;
            _orderSettings = orderSettings;
        }

        public async Task<CheckoutConfirmModel> Handle(GetConfirmOrder request, CancellationToken cancellationToken)
        {
            var model = new CheckoutConfirmModel();
            //terms of service
            model.TermsOfServiceOnOrderConfirmPage = _orderSettings.TermsOfServiceOnOrderConfirmPage;
            //min order amount validation
            bool minOrderTotalAmountOk = await _orderProcessingService.ValidateMinOrderTotalAmount(request.Cart);
            if (!minOrderTotalAmountOk)
            {
                decimal minOrderTotalAmount = await _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinOrderTotalAmount, request.Currency);
                model.MinOrderTotalWarning = string.Format(_localizationService.GetResource("Checkout.MinOrderTotalAmount"), _priceFormatter.FormatPrice(minOrderTotalAmount, true, false));
            }
            return model;
        }
    }
}
