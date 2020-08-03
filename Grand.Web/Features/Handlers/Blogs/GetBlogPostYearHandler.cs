using Grand.Core;
using Grand.Core.Caching;
using Grand.Services.Blogs;
using Grand.Web.Features.Models.Blogs;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Models.Blogs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Blogs
{
    public class GetBlogPostYearHandler : IRequestHandler<GetBlogPostYear, IList<BlogPostYearModel>>
    {
        private readonly IBlogService _blogService;
        private readonly ICacheManager _cacheManager;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;

        public GetBlogPostYearHandler(IBlogService blogService, ICacheManager cacheManager, 
            IWorkContext workContext, IStoreContext storeContext)
        {
            _blogService = blogService;
            _cacheManager = cacheManager;
            _workContext = workContext;
            _storeContext = storeContext;
        }

        public async Task<IList<BlogPostYearModel>> Handle(GetBlogPostYear request, CancellationToken cancellationToken)
        {
            var cacheKey = string.Format(ModelCacheEventConst.BLOG_MONTHS_MODEL_KEY, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);
            var cachedModel = await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var model = new List<BlogPostYearModel>();

                var blogPosts = await _blogService.GetAllBlogPosts(_storeContext.CurrentStore.Id);
                if (blogPosts.Any())
                {
                    var months = new SortedDictionary<DateTime, int>();

                    var blogPost = blogPosts[blogPosts.Count - 1];
                    var first = blogPost.StartDateUtc ?? blogPost.CreatedOnUtc;
                    while (DateTime.SpecifyKind(first, DateTimeKind.Utc) <= DateTime.UtcNow.AddMonths(1))
                    {
                        var list = blogPosts.GetPostsByDate(new DateTime(first.Year, first.Month, 1), new DateTime(first.Year, first.Month, 1).AddMonths(1).AddSeconds(-1));
                        if (list.Any())
                        {
                            var date = new DateTime(first.Year, first.Month, 1);
                            months.Add(date, list.Count);
                        }

                        first = first.AddMonths(1);
                    }


                    int current = 0;
                    foreach (var kvp in months)
                    {
                        var date = kvp.Key;
                        var blogPostCount = kvp.Value;
                        if (current == 0)
                            current = date.Year;

                        if (date.Year > current || !model.Any())
                        {
                            var yearModel = new BlogPostYearModel {
                                Year = date.Year
                            };
                            model.Insert(0, yearModel);
                        }

                        model.First().Months.Insert(0, new BlogPostMonthModel {
                            Month = date.Month,
                            BlogPostCount = blogPostCount
                        });

                        current = date.Year;
                    }
                }
                return model;
            });
            return cachedModel;
        }
    }
}
