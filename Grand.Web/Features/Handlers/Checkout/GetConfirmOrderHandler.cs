using Grand.Domain.Orders;
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
        private readonly ILocalizationService _localizationService;

        private readonly OrderSettings _orderSettings;

        public GetConfirmOrderHandler(IOrderProcessingService orderProcessingService,
            ILocalizationService localizationService,
            OrderSettings orderSettings)
        {
            _orderProcessingService = orderProcessingService;
            _localizationService = localizationService;
            _orderSettings = orderSettings;
        }

        public async Task<CheckoutConfirmModel> Handle(GetConfirmOrder request, CancellationToken cancellationToken)
        {
            var model = new CheckoutConfirmModel();
            //terms of service
            model.TermsOfServiceOnOrderConfirmPage = _orderSettings.TermsOfServiceOnOrderConfirmPage;
            //min order amount validation
            bool minOrderTotalAmountOk = await _orderProcessingService.ValidateOrderTotalAmount(request.Customer, request.Cart);
            if (!minOrderTotalAmountOk)
            {
                model.MinOrderTotalWarning = string.Format(_localizationService.GetResource("Checkout.MinMaxOrderTotalAmount"));
            }
            return model;
        }
    }
}
