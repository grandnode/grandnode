using Grand.Domain.Localization;
using Grand.Services.Commands.Models.Catalog;
using Grand.Services.Messages;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Commands.Handlers.Catalog
{
    public class SendQuantityBelowStoreOwnerNotificationCommandHandler : IRequestHandler<SendQuantityBelowStoreOwnerNotificationCommand, bool>
    {
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;

        public SendQuantityBelowStoreOwnerNotificationCommandHandler(
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings)
        {
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
        }

        public async Task<bool> Handle(SendQuantityBelowStoreOwnerNotificationCommand request, CancellationToken cancellationToken)
        {
            if (request.ProductAttributeCombination == null)
                await _workflowMessageService.SendQuantityBelowStoreOwnerNotification(request.Product, _localizationSettings.DefaultAdminLanguageId);
            else
                await _workflowMessageService.SendQuantityBelowStoreOwnerNotification(request.Product, request.ProductAttributeCombination, _localizationSettings.DefaultAdminLanguageId);

            return true;
        }
    }
}
