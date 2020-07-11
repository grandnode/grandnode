using Grand.Services.Customers;
using MediatR;

namespace Grand.Services.Notifications.Customers
{
    public class CustomerRegistrationEvent<C, R> : INotification where C : CustomerRegistrationResult where R : CustomerRegistrationRequest
    {
        private readonly C _result;
        private readonly R _request;

        public CustomerRegistrationEvent(C result, R request)
        {
            _result = result;
            _request = request;
        }
        public C Result { get { return _result; } }
        public R Request { get { return _request; } }

    }
}
