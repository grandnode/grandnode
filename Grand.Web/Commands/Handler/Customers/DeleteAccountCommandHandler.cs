using Grand.Core.Domain.Localization;
using Grand.Services.Customers;
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
        private readonly LocalizationSettings _localizationSettings;

        public DeleteAccountCommandHandler(
            ICustomerService customerService,
            IWorkflowMessageService workflowMessageService,
            IQueuedEmailService queuedEmailService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            LocalizationSettings localizationSettings)
        {
            _customerService = customerService;
            _workflowMessageService = workflowMessageService;
            _queuedEmailService = queuedEmailService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _localizationSettings = localizationSettings;
        }

        public async Task<bool> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
        {
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
