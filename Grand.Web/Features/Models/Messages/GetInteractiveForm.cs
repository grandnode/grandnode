using Grand.Web.Models.Messages;
using MediatR;

namespace Grand.Web.Features.Models.Messages
{
    public class GetInteractiveForm : IRequest<InteractiveFormModel>
    {
        public string SystemName { get; set; }
    }
}
