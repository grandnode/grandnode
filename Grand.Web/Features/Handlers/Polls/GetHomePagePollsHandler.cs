using Grand.Core;
using Grand.Core.Caching;
using Grand.Services.Polls;
using Grand.Web.Extensions;
using Grand.Web.Features.Models.Polls;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Models.Polls;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Polls
{
    public class GetHomePagePollsHandler : IRequestHandler<GetHomePagePolls, IList<PollModel>>
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICacheBase _cacheBase;
        private readonly IPollService _pollService;

        public GetHomePagePollsHandler(IWorkContext workContext,
            IStoreContext storeContext,
            ICacheBase cacheManager,
            IPollService pollService
            )
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _cacheBase = cacheManager;
            _pollService = pollService;
        }

        public async Task<IList<PollModel>> Handle(GetHomePagePolls request, CancellationToken cancellationToken)
        {
            var cacheKey = string.Format(ModelCacheEventConst.HOMEPAGE_POLLS_MODEL_KEY, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);
            var model = await _cacheBase.GetAsync(cacheKey, async () =>
            {
                var pollModels = new List<PollModel>();
                var polls = await _pollService.GetPolls(_storeContext.CurrentStore.Id, true);
                foreach (var item in polls)
                {
                    pollModels.Add(item.ToModel(_workContext.WorkingLanguage, _workContext.CurrentCustomer));
                }
                return pollModels;
            });
            return model;
        }
    }
}
