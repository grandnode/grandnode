using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Polls;
using Grand.Services.Localization;
using Grand.Services.Polls;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Models.Polls;
using Grand.Core.Infrastructure;
using Grand.Services.Customers;
using MongoDB.Bson;
using Grand.Services.Security;

namespace Grand.Web.Controllers
{
    public partial class PollController : BasePublicController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IPollService _pollService;
        private readonly ICacheManager _cacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IAclService _aclService;
        #endregion

        #region Constructors

        public PollController(ILocalizationService localizationService,
            IWorkContext workContext,
            IPollService pollService,
            ICacheManager cacheManager,
            IStoreContext storeContext,
            IAclService aclService)
        {
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._pollService = pollService;
            this._cacheManager = cacheManager;
            this._storeContext = storeContext;
            this._aclService = aclService;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected virtual PollModel PreparePollModel(Poll poll, bool setAlreadyVotedProperty)
        {
            var model = new PollModel
            {
                Id = poll.Id,
                AlreadyVoted = setAlreadyVotedProperty && _pollService.AlreadyVoted(poll.Id, _workContext.CurrentCustomer.Id),
                Name = poll.GetLocalized(x=>x.Name)
            };
            var answers = poll.PollAnswers.OrderBy(x => x.DisplayOrder);
            foreach (var answer in answers)
                model.TotalVotes += answer.NumberOfVotes;
            foreach (var pa in answers)
            {
                model.Answers.Add(new PollAnswerModel
                {
                    Id = pa.Id,
                    PollId = poll.Id,
                    Name = pa.GetLocalized(x=>x.Name),
                    NumberOfVotes = pa.NumberOfVotes,
                    PercentOfTotalVotes = model.TotalVotes > 0 ? ((Convert.ToDouble(pa.NumberOfVotes) / Convert.ToDouble(model.TotalVotes)) * Convert.ToDouble(100)) : 0,
                });
            }

            return model;
        }

        #endregion

        #region Methods

        [ChildActionOnly]
        public ActionResult PollBlock(string systemKeyword)
        {
            if (String.IsNullOrWhiteSpace(systemKeyword))
                return Content("");

            var cacheKey = string.Format(ModelCacheEventConsumer.POLL_BY_SYSTEMNAME__MODEL_KEY, systemKeyword, _storeContext.CurrentStore.Id);
            var cachedModel = _cacheManager.Get(cacheKey, () =>
            {
                Poll poll = _pollService.GetPollBySystemKeyword(systemKeyword, _storeContext.CurrentStore.Id);
                //ACL (access control list)
                if (!_aclService.Authorize(poll))
                    return new PollModel { Id = "" };

                if (poll == null ||
                    !poll.Published ||
                    (poll.StartDateUtc.HasValue && poll.StartDateUtc.Value > DateTime.UtcNow) ||
                    (poll.EndDateUtc.HasValue && poll.EndDateUtc.Value < DateTime.UtcNow))
                    //we do not cache nulls. that's why let's return an empty record (ID = 0)
                    return new PollModel { Id = ""};

                return PreparePollModel(poll, false);
            });
            if (cachedModel == null || cachedModel.Id == "")
                return Content("");

            //"AlreadyVoted" property of "PollModel" object depends on the current customer. Let's update it.
            //But first we need to clone the cached model (the updated one should not be cached)
            var model = (PollModel)cachedModel.Clone();
            model.AlreadyVoted = _pollService.AlreadyVoted(model.Id, _workContext.CurrentCustomer.Id);

            return PartialView(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Vote(string pollAnswerId, string pollId)
        {
            var poll = _pollService.GetPollById(pollId); //pollAnswer.Poll;
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
                pollAnswer.PollVotingRecords.Add(new PollVotingRecord
                {
                    PollId = pollId,
                    PollAnswerId = pollAnswer.Id,
                    CustomerId = _workContext.CurrentCustomer.Id,
                    CreatedOnUtc = DateTime.UtcNow
                });
                //update totals
                pollAnswer.NumberOfVotes = pollAnswer.PollVotingRecords.Count;
                _pollService.UpdatePoll(poll);

                if(!_workContext.CurrentCustomer.IsHasPoolVoting)
                {
                    _workContext.CurrentCustomer.IsHasPoolVoting = true;
                    EngineContext.Current.Resolve<ICustomerService>().UpdateHasPoolVoting(_workContext.CurrentCustomer.Id);
                }
            }

            return Json(new
            {
                html = this.RenderPartialViewToString("_Poll", PreparePollModel(poll, true)),
            });
        }
        
        [ChildActionOnly]
        public ActionResult HomePagePolls()
        {
            var cacheKey = string.Format(ModelCacheEventConsumer.HOMEPAGE_POLLS_MODEL_KEY, _workContext.WorkingLanguage.Id);
            var cachedModel = _cacheManager.Get(cacheKey, () =>
                _pollService.GetPolls(_storeContext.CurrentStore.Id, true)
                .Select(x => PreparePollModel(x, false))
                .ToList());
            //"AlreadyVoted" property of "PollModel" object depends on the current customer. Let's update it.
            //But first we need to clone the cached model (the updated one should not be cached)
            var model = new List<PollModel>();
            foreach (var p in cachedModel)
            {
                var pollModel = (PollModel) p.Clone();
                pollModel.AlreadyVoted = _pollService.AlreadyVoted(pollModel.Id, _workContext.CurrentCustomer.Id);
                model.Add(pollModel);
            }

            if (!model.Any())
                Content("");

            return PartialView(model);
        }

        #endregion

    }
}
