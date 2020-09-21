using Grand.Core;
using Grand.Core.Caching;
using Grand.Domain.Blogs;
using Grand.Domain.Media;
using Grand.Services.Blogs;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Seo;
using Grand.Web.Features.Models.Blogs;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Models.Blogs;
using Grand.Web.Models.Media;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Blogs
{
    public class GetHomePageBlogHandler : IRequestHandler<GetHomePageBlog, HomePageBlogItemsModel>
    {
        private readonly IBlogService _blogService;
        private readonly ICacheManager _cacheManager;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;

        private readonly BlogSettings _blogSettings;
        private readonly MediaSettings _mediaSettings;

        public GetHomePageBlogHandler(IBlogService blogService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IPictureService pictureService,
            ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper,
            ICacheManager cacheManager,
            BlogSettings blogSettings,
            MediaSettings mediaSettings)
        {
            _blogService = blogService;
            _workContext = workContext;
            _storeContext = storeContext;
            _pictureService = pictureService;
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;
            _cacheManager = cacheManager;

            _blogSettings = blogSettings;
            _mediaSettings = mediaSettings;
        }

        public async Task<HomePageBlogItemsModel> Handle(GetHomePageBlog request, CancellationToken cancellationToken)
        {
            var cacheKey = string.Format(ModelCacheEventConst.BLOG_HOMEPAGE_MODEL_KEY, 
                _workContext.WorkingLanguage.Id, 
                _storeContext.CurrentStore.Id);
            var cachedModel = await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var model = new HomePageBlogItemsModel();

                var blogPosts = await _blogService.GetAllBlogPosts(_storeContext.CurrentStore.Id,
                        null, null, 0, _blogSettings.HomePageBlogCount);

                foreach (var post in blogPosts)
                {
                    var item = new HomePageBlogItemsModel.BlogItemModel();
                    var description = post.GetLocalized(x => x.BodyOverview, _workContext.WorkingLanguage.Id);
                    item.SeName = post.GetSeName(_workContext.WorkingLanguage.Id);
                    item.Title = post.GetLocalized(x => x.Title, _workContext.WorkingLanguage.Id);
                    item.Short = description?.Length > _blogSettings.MaxTextSizeHomePage ? description.Substring(0, _blogSettings.MaxTextSizeHomePage) : description;
                    item.CreatedOn = _dateTimeHelper.ConvertToUserTime(post.StartDateUtc ?? post.CreatedOnUtc, DateTimeKind.Utc);
                    item.GenericAttributes = post.GenericAttributes;

                    //prepare picture model
                    if (!string.IsNullOrEmpty(post.PictureId))
                    {
                        var pictureModel = new PictureModel {
                            Id = post.PictureId,
                            FullSizeImageUrl = await _pictureService.GetPictureUrl(post.PictureId),
                            ImageUrl = await _pictureService.GetPictureUrl(post.PictureId, _mediaSettings.BlogThumbPictureSize),
                            Title = string.Format(_localizationService.GetResource("Media.Blog.ImageLinkTitleFormat"), post.Title),
                            AlternateText = string.Format(_localizationService.GetResource("Media.Blog.ImageAlternateTextFormat"), post.Title)
                        };
                        item.PictureModel = pictureModel;
                    }
                    model.Items.Add(item);
                }
                return model;
            });

            return cachedModel;
        }
    }
}
