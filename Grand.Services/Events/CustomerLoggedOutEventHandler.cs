using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Notifications.Customers;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Events
{
    public class CustomerLoggedOutEventHandler : INotificationHandler<CustomerLoggedOutEvent>
    {
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;

        public CustomerLoggedOutEventHandler(
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService)
        {
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
        }

        public async Task Handle(CustomerLoggedOutEvent notification, CancellationToken cancellationToken)
        {
            //activity log
            await _customerActivityService.InsertActivity("PublicStore.Logout", "", _localizationService.GetResource("ActivityLog.PublicStore.Logout"), notification.Customer);
        }
    }
}
