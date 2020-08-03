using Grand.Services.Commands.Models.Catalog;
using Grand.Services.Messages;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Commands.Handlers.Catalog
{
    public class SendOutBidCustomerNotificationCommandHandler : IRequestHandler<SendOutBidCustomerNotificationCommand, bool>
    {
        private readonly IWorkflowMessageService _workflowMessageService;

        public SendOutBidCustomerNotificationCommandHandler(IWorkflowMessageService workflowMessageService)
        {
            _workflowMessageService = workflowMessageService;
        }

        public async Task<bool> Handle(SendOutBidCustomerNotificationCommand request, CancellationToken cancellationToken)
        {
            await _workflowMessageService.SendOutBidCustomerNotification(request.Product, request.Language.Id, request.Bid);
            return true;
        }
    }
}
