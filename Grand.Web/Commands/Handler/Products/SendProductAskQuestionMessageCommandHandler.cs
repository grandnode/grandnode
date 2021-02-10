using Grand.Services.Common;
using Grand.Services.Messages;
using Grand.Web.Commands.Models.Products;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Commands.Handler.Products
{
    public class SendProductAskQuestionMessageCommandHandler : IRequestHandler<SendProductAskQuestionMessageCommand, bool>
    {
        private readonly IWorkflowMessageService _workflowMessageService;

        public SendProductAskQuestionMessageCommandHandler(IWorkflowMessageService workflowMessageService)
        {
            _workflowMessageService = workflowMessageService;
        }

        public async Task<bool> Handle(SendProductAskQuestionMessageCommand request, CancellationToken cancellationToken)
        {
            await _workflowMessageService.SendProductQuestionMessage(request.Customer, request.Store,
                               request.Language.Id, request.Product, request.Model.Email, request.Model.FullName, request.Model.Phone,
                               FormatText.ConvertText(request.Model.Message));

            return true;
        }
    }
}
