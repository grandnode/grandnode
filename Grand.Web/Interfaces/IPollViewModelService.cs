using Grand.Core.Domain.Polls;
using Grand.Web.Models.Polls;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Interfaces
{
    public partial interface IPollViewModelService
    {
        Task<PollModel> PreparePoll(Poll poll, bool setAlreadyVotedProperty);
        Task<PollModel> PreparePollBySystemName(string systemKeyword);
        Task<List<PollModel>> PrepareHomePagePoll();
        Task PollVoting(Poll poll, PollAnswer pollAnswer);
        Task<PollModel> PreparePollModel(Poll poll, bool setAlreadyVotedProperty);
    }
}