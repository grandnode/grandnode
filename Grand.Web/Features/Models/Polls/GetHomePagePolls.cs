using Grand.Web.Models.Polls;
using MediatR;
using System.Collections.Generic;

namespace Grand.Web.Features.Models.Polls
{
    public class GetHomePagePolls : IRequest<IList<PollModel>>
    {
    }
}
