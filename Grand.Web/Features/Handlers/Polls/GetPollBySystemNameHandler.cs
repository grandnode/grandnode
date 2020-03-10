using Grand.Core;
using Grand.Core.Caching;
using Grand.Services.Polls;
using Grand.Services.Security;
using Grand.Web.Extensions;
using Grand.Web.Features.Models.Polls;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Models.Polls;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Polls
{
    public class GetPollBySystemNameHandler : IRequestHandler<GetPollBySystemName, PollModel>
    {
        private readonly ICacheManager _cacheManager;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IPollService _pollService;
        private readonly IAclService _aclService;

        public GetPollBySystemNameHandler(
            ICacheManager cacheManager,
            IWorkContext workContext,
            IStoreContext storeContext,
            IPollService pollService,
            IAclService aclService)
        {
            _cacheManager = cacheManager;
            _workContext = workContext;
            _storeContext = storeContext;
            _pollService = pollService;
            _aclService = aclService;
        }

        public async Task<PollModel> Handle(GetPollBySystemName request, CancellationToken cancellationToken)
        {
            var cacheKey = string.Format(ModelCacheEventConst.POLL_BY_SYSTEMNAME__MODEL_KEY, request.SystemName, _storeContext.CurrentStore.Id);
            var model = await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var poll = await _pollService.GetPollBySystemKeyword(request.SystemName, _storeContext.CurrentStore.Id);
                //ACL (access control list)
                if (!_aclService.Authorize(poll))
                    return new PollModel { Id = "" };

                if (poll == null ||
                    !poll.Published ||
                    (poll.StartDateUtc.HasValue && poll.StartDateUtc.Value > DateTime.UtcNow) ||
                    (poll.EndDateUtc.HasValue && poll.EndDateUtc.Value < DateTime.UtcNow))
                    //we do not cache nulls. that's why let's return an empty record (ID = 0)
                    return new PollModel { Id = "" };

                return poll.ToModel(_workContext.WorkingLanguage, _workContext.CurrentCustomer);
            });
            return model;
        }
    }
}
