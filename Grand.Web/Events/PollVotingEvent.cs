using Grand.Domain.Polls;
using MediatR;

namespace Grand.Web.Events
{
    public class PollVotingEvent : INotification
    {
        public Poll Poll { get; private set; }
        public PollAnswer PollAnswer { get; private set; }
        public PollVotingEvent(Poll poll, PollAnswer pollAnswer)
        {
            Poll = poll;
            PollAnswer = pollAnswer;
        }
    }
}
