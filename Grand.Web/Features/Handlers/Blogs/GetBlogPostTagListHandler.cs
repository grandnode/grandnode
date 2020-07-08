using Grand.Core;
using Grand.Core.Caching;
using Grand.Domain.Blogs;
using Grand.Services.Blogs;
using Grand.Web.Features.Models.Blogs;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Models.Blogs;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Blogs
{
    public class GetBlogPostTagListHandler : IRequestHandler<GetBlogPostTagList, BlogPostTagListModel>
    {
        private readonly IBlogService _blogService;
        private readonly ICacheManager _cacheManager;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;

        private readonly BlogSettings _blogSettings;

        public GetBlogPostTagListHandler(IBlogService blogService, ICacheManager cacheManager, IWorkContext workContext, IStoreContext storeContext, BlogSettings blogSettings)
        {
            _blogService = blogService;
            _cacheManager = cacheManager;
            _workContext = workContext;
            _storeContext = storeContext;
            _blogSettings = blogSettings;
        }

        public async Task<BlogPostTagListModel> Handle(GetBlogPostTagList request, CancellationToken cancellationToken)
        {
            var cacheKey = string.Format(ModelCacheEventConst.BLOG_TAGS_MODEL_KEY, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);
            var cachedModel = await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var model = new BlogPostTagListModel();

                //get tags
                var tags = await _blogService.GetAllBlogPostTags(_storeContext.CurrentStore.Id);
                tags = tags.OrderByDescending(x => x.BlogPostCount)
                    .Take(_blogSettings.NumberOfTags)
                    .ToList();
                //sorting
                tags = tags.OrderBy(x => x.Name).ToList();

                foreach (var tag in tags)
                    model.Tags.Add(new BlogPostTagModel {
                        Name = tag.Name,
                        BlogPostCount = tag.BlogPostCount
                    });
                return model;
            });
            return cachedModel;
        }
    }
}
