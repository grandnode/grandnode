using Grand.Web.Models.Polls;
using MediatR;

namespace Grand.Web.Features.Models.Polls
{
    public class GetPollBySystemName : IRequest<PollModel>
    {
        public string SystemName { get; set; }
    }
}
