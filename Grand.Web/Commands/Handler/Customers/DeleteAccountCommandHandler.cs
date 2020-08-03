using Grand.Domain.Localization;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Messages;
using Grand.Web.Commands.Models.Customers;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Commands.Handler.Customers
{
    public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, bool>
    {
        private readonly ICustomerService _customerService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly LocalizationSettings _localizationSettings;

        public DeleteAccountCommandHandler(
            ICustomerService customerService,
            IWorkflowMessageService workflowMessageService,
            IQueuedEmailService queuedEmailService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            LocalizationSettings localizationSettings)
        {
            _customerService = customerService;
            _workflowMessageService = workflowMessageService;
            _queuedEmailService = queuedEmailService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _localizationSettings = localizationSettings;
        }

        public async Task<bool> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
        {
            //activity log
            await _customerActivityService.InsertActivity("PublicStore.DeleteAccount", "", _localizationService.GetResource("ActivityLog.DeleteAccount"), request.Customer);

            //send notification to customer
            await _workflowMessageService.SendCustomerDeleteStoreOwnerNotification(request.Customer, _localizationSettings.DefaultAdminLanguageId);

            //delete emails
            await _queuedEmailService.DeleteCustomerEmail(request.Customer.Email);

            //delete newsletter subscription
            var newsletter = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreId(request.Customer.Email, request.Store.Id);
            if (newsletter != null)
                await _newsLetterSubscriptionService.DeleteNewsLetterSubscription(newsletter);

            //delete account
            await _customerService.DeleteCustomer(request.Customer);

            return true;
        }
    }
}
