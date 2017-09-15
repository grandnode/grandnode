using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Grand.Core;
using Grand.Core.Domain.Customers;
using Grand.Services.Localization;
using Grand.Services.Polls;
using Grand.Web.Services;

namespace Grand.Web.Controllers
{
    public partial class PollController : BasePublicController
    {
        #region Fields

        private readonly IPollWebService _pollWebService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IPollService _pollService;
        #endregion

        #region Constructors

        public PollController(IPollWebService pollWebService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IPollService pollService)
        {
            this._pollWebService = pollWebService;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._pollService = pollService;
        }

        #endregion

        #region Methods

        [HttpPost]
        public virtual IActionResult Vote(string pollAnswerId, string pollId)
        {
            var poll = _pollService.GetPollById(pollId); 
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

            bool alreadyVoted = _pollService.AlreadyVoted(poll.Id, _workContext.CurrentCustomer.Id);
            if (!alreadyVoted)
            {
                //vote
                _pollWebService.PollVoting(poll, pollAnswer);
            }

            return Json(new
            {
                html = this.RenderPartialViewToString("_Poll", _pollWebService.PreparePoll(poll, true)),
            });
        }
        
        #endregion

    }
}
