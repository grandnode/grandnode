using Grand.Services.Payments;
using Grand.Web.Models.Checkout;
using MediatR;

namespace Grand.Web.Features.Models.Checkout
{
    public class GetPaymentInfo : IRequest<CheckoutPaymentInfoModel>
    {
        public IPaymentMethod PaymentMethod { get; set; }
    }
}
