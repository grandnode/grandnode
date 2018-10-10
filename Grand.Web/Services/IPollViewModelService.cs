using Grand.Core.Domain.Polls;
using Grand.Web.Models.Polls;
using System.Collections.Generic;

namespace Grand.Web.Services
{
    public partial interface IPollViewModelService
    {
        PollModel PreparePoll(Poll poll, bool setAlreadyVotedProperty);
        PollModel PreparePollBySystemName(string systemKeyword);
        List<PollModel> PrepareHomePagePoll();

        void PollVoting(Poll poll, PollAnswer pollAnswer);
        PollModel PreparePollModel(Poll poll, bool setAlreadyVotedProperty);
    }
}