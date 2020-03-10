using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Stores;
using Grand.Web.Models.Polls;
using MediatR;

namespace Grand.Web.Features.Models.Polls
{
    public class GetPollBySystemName : IRequest<PollModel>
    {
        public string SystemName { get; set; }
    }
}
