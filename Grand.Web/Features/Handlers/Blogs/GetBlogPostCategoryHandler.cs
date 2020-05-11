using Grand.Core;
using Grand.Core.Caching;
using Grand.Services.Blogs;
using Grand.Services.Localization;
using Grand.Web.Features.Models.Blogs;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Models.Blogs;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Blogs
{
    public class GetBlogPostCategoryHandler : IRequestHandler<GetBlogPostCategory, IList<BlogPostCategoryModel>>
    {
        private readonly IBlogService _blogService;
        private readonly ICacheManager _cacheManager;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;

        public GetBlogPostCategoryHandler(IBlogService blogService, ICacheManager cacheManager,
            IWorkContext workContext, IStoreContext storeContext)
        {
            _blogService = blogService;
            _cacheManager = cacheManager;
            _workContext = workContext;
            _storeContext = storeContext;
        }

        public async Task<IList<BlogPostCategoryModel>> Handle(GetBlogPostCategory request, CancellationToken cancellationToken)
        {
            var cacheKey = string.Format(ModelCacheEventConst.BLOG_CATEGORY_MODEL_KEY, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);
            var cachedModel = await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var model = new List<BlogPostCategoryModel>();
                var categories = await _blogService.GetAllBlogCategories(_storeContext.CurrentStore.Id);
                foreach (var item in categories)
                {
                    model.Add(new BlogPostCategoryModel() {
                        Id = item.Id,
                        Name = item.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                        SeName = item.SeName,
                        BlogPostCount = item.BlogPosts.Count
                    });
                }
                return model;
            });
            return cachedModel;
        }
    }
}
