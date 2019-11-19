using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Domain.Polls;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Polls;
using Grand.Services.Security;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Interfaces;
using Grand.Web.Models.Polls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Services
{
    public partial class PollViewModelService : IPollViewModelService
    {
        private readonly IPollService _pollService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICacheManager _cacheManager;
        private readonly IAclService _aclService;
        private readonly IServiceProvider _serviceProvider;

        public PollViewModelService(IPollService pollService, IWorkContext workContext, IStoreContext storeContext, ICacheManager cacheManager, IAclService aclService,
            IServiceProvider serviceProvider)
        {
            _pollService = pollService;
            _workContext = workContext;
            _storeContext = storeContext;
            _cacheManager = cacheManager;
            _aclService = aclService;
            _serviceProvider = serviceProvider;
        }

        public virtual async Task<PollModel> PreparePoll(Poll poll, bool setAlreadyVotedProperty)
        {
            var model = new PollModel {
                Id = poll.Id,
                AlreadyVoted = setAlreadyVotedProperty && await _pollService.AlreadyVoted(poll.Id, _workContext.CurrentCustomer.Id),
                Name = poll.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id)
            };
            var answers = poll.PollAnswers.OrderBy(x => x.DisplayOrder);
            foreach (var answer in answers)
                model.TotalVotes += answer.NumberOfVotes;
            foreach (var pa in answers)
            {
                model.Answers.Add(new PollAnswerModel {
                    Id = pa.Id,
                    PollId = poll.Id,
                    Name = pa.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                    NumberOfVotes = pa.NumberOfVotes,
                    PercentOfTotalVotes = model.TotalVotes > 0 ? ((Convert.ToDouble(pa.NumberOfVotes) / Convert.ToDouble(model.TotalVotes)) * Convert.ToDouble(100)) : 0,
                });
            }

            return model;
        }
        public virtual async Task<PollModel> PreparePollBySystemName(string systemKeyword)
        {
            var cacheKey = string.Format(ModelCacheEventConsumer.POLL_BY_SYSTEMNAME__MODEL_KEY, systemKeyword, _storeContext.CurrentStore.Id);
            var cachedModel = await _cacheManager.GetAsync(cacheKey, async () =>
            {
                Poll poll = await _pollService.GetPollBySystemKeyword(systemKeyword, _storeContext.CurrentStore.Id);
                //ACL (access control list)
                if (!_aclService.Authorize(poll))
                    return new PollModel { Id = "" };

                if (poll == null ||
                    !poll.Published ||
                    (poll.StartDateUtc.HasValue && poll.StartDateUtc.Value > DateTime.UtcNow) ||
                    (poll.EndDateUtc.HasValue && poll.EndDateUtc.Value < DateTime.UtcNow))
                    //we do not cache nulls. that's why let's return an empty record (ID = 0)
                    return new PollModel { Id = "" };

                return await PreparePoll(poll, false);
            });
            if (cachedModel == null || cachedModel.Id == "")
                return null;

            //"AlreadyVoted" property of "PollModel" object depends on the current customer. Let's update it.
            //But first we need to clone the cached model (the updated one should not be cached)
            var model = (PollModel)cachedModel.Clone();
            model.AlreadyVoted = await _pollService.AlreadyVoted(model.Id, _workContext.CurrentCustomer.Id);
            return model;
        }
        public virtual async Task<List<PollModel>> PrepareHomePagePoll()
        {
            var cacheKey = string.Format(ModelCacheEventConsumer.HOMEPAGE_POLLS_MODEL_KEY, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);
            var cachedModel = await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var pollModels = new List<PollModel>();
                var polls = await _pollService.GetPolls(_storeContext.CurrentStore.Id, true);
                foreach (var item in polls)
                {
                    pollModels.Add(await PreparePoll(item, false));
                }
                return pollModels;
            });

            //"AlreadyVoted" property of "PollModel" object depends on the current customer. Let's update it.
            //But first we need to clone the cached model (the updated one should not be cached)
            var model = new List<PollModel>();
            foreach (var p in cachedModel)
            {
                var pollModel = (PollModel)p.Clone();
                pollModel.AlreadyVoted = await _pollService.AlreadyVoted(pollModel.Id, _workContext.CurrentCustomer.Id);
                model.Add(pollModel);
            }
            return model;
        }

        public virtual async Task PollVoting(Poll poll, PollAnswer pollAnswer)
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
                await _serviceProvider.GetRequiredService<ICustomerService>().UpdateContributions(_workContext.CurrentCustomer);
            }
        }

        public virtual async Task<PollModel> PreparePollModel(Poll poll, bool setAlreadyVotedProperty)
        {
            var model = new PollModel {
                Id = poll.Id,
                AlreadyVoted = setAlreadyVotedProperty && await _pollService.AlreadyVoted(poll.Id, _workContext.CurrentCustomer.Id),
                Name = poll.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id)
            };
            var answers = poll.PollAnswers.OrderBy(x => x.DisplayOrder);
            foreach (var answer in answers)
                model.TotalVotes += answer.NumberOfVotes;
            foreach (var pa in answers)
            {
                model.Answers.Add(new PollAnswerModel {
                    Id = pa.Id,
                    PollId = poll.Id,
                    Name = pa.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                    NumberOfVotes = pa.NumberOfVotes,
                    PercentOfTotalVotes = model.TotalVotes > 0 ? ((Convert.ToDouble(pa.NumberOfVotes) / Convert.ToDouble(model.TotalVotes)) * Convert.ToDouble(100)) : 0,
                });
            }
            return model;
        }

    }
}