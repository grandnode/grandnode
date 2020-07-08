using Grand.Core;
using Grand.Domain.Customers;
using Grand.Domain.Polls;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Polls;
using Grand.Web.Events;
using Grand.Web.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Controllers
{
    public partial class PollController : BasePublicController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IPollService _pollService;
        private readonly ICustomerService _customerService;
        private readonly IMediator _mediator;

        #endregion

        #region Constructors

        public PollController(
            ILocalizationService localizationService,
            IWorkContext workContext,
            IPollService pollService,
            ICustomerService customerService,
            IMediator mediator)
        {
            _localizationService = localizationService;
            _workContext = workContext;
            _pollService = pollService;
            _customerService = customerService;
            _mediator = mediator;
        }

        #endregion

        #region Utilities

        protected async Task PollVoting(Poll poll, PollAnswer pollAnswer)
        {
            pollAnswer.PollVotingRecords.Add(new PollVotingRecord {
                PollId = poll.Id,
                PollAnswerId = pollAnswer.Id,
                CustomerId = _workContext.CurrentCustomer.Id,
                CreatedOnUtc = DateTime.UtcNow
            });
            //update totals
            pollAnswer.NumberOfVotes = pollAnswer.PollVotingRecords.Count;
            await _pollService.UpdatePoll(poll);

            if (!_workContext.CurrentCustomer.HasContributions)
            {
                await _customerService.UpdateContributions(_workContext.CurrentCustomer);
            }
            await _mediator.Publish(new PollVotingEvent(poll, pollAnswer));
        }

        #endregion

        #region Methods

        [HttpPost]
        public virtual async Task<IActionResult> Vote(string pollAnswerId, string pollId)
        {
            var poll = await _pollService.GetPollById(pollId); 
            if (!poll.Published)
                return Json(new
                {
                    error = "Poll is not available",
                });

            var pollAnswer = poll.PollAnswers.FirstOrDefault(x=>x.Id == pollAnswerId);
            if (pollAnswer == null)
                return Json(new
                {
                    error = "No poll answer found with the specified id",
                });


            if (_workContext.CurrentCustomer.IsGuest() && !poll.AllowGuestsToVote)
                return Json(new
                {
                    error = _localizationService.GetResource("Polls.OnlyRegisteredUsersVote"),
                });

            bool alreadyVoted = await _pollService.AlreadyVoted(poll.Id, _workContext.CurrentCustomer.Id);
            if (!alreadyVoted)
            {
                //vote
                await PollVoting(poll, pollAnswer);
            }

            return Json(new
            {
                html = await RenderPartialViewToString("_Poll", poll.ToModel(_workContext.WorkingLanguage, _workContext.CurrentCustomer)),
            });
        }
        
        #endregion

    }
}
