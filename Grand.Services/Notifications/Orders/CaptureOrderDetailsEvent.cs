using Grand.Services.Payments;
using MediatR;

namespace Grand.Services.Notifications.Orders
{
    public class CaptureOrderDetailsEvent<R, C> : INotification where R : CapturePaymentResult where C : CapturePaymentRequest
    {
        private readonly R _result;
        private readonly C _containter;

        public CaptureOrderDetailsEvent(R result, C containter)
        {
            _result = result;
            _containter = containter;
        }
        public R Result { get { return _result; } }
        public C Containter { get { return _containter; } }

    }

}
