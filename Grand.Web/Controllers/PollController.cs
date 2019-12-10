using Grand.Core;
using Grand.Core.Domain.Customers;
using Grand.Services.Localization;
using Grand.Services.Polls;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Controllers
{
    public partial class PollController : BasePublicController
    {
        #region Fields

        private readonly IPollViewModelService _pollViewModelService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IPollService _pollService;
        #endregion

        #region Constructors

        public PollController(IPollViewModelService pollViewModelService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IPollService pollService)
        {
            _pollViewModelService = pollViewModelService;
            _localizationService = localizationService;
            _workContext = workContext;
            _pollService = pollService;
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
                await _pollViewModelService.PollVoting(poll, pollAnswer);
            }

            return Json(new
            {
                html = this.RenderPartialViewToString("_Poll", await _pollViewModelService.PreparePoll(poll, true)),
            });
        }
        
        #endregion

    }
}
