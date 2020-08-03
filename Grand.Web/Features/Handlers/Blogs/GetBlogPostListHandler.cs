using Grand.Core;
using Grand.Domain;
using Grand.Domain.Blogs;
using Grand.Domain.Media;
using Grand.Services.Blogs;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Seo;
using Grand.Web.Features.Models.Blogs;
using Grand.Web.Models.Blogs;
using Grand.Web.Models.Media;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Blogs
{
    public class GetBlogPostListHandler : IRequestHandler<GetBlogPostList, BlogPostListModel>
    {
        private readonly IBlogService _blogService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;

        private readonly BlogSettings _blogSettings;
        private readonly MediaSettings _mediaSettings;

        public GetBlogPostListHandler(
            IBlogService blogService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IPictureService pictureService,
            ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper,
            BlogSettings blogSettings,
            MediaSettings mediaSettings)
        {
            _blogService = blogService;
            _workContext = workContext;
            _storeContext = storeContext;
            _pictureService = pictureService;
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;

            _blogSettings = blogSettings;
            _mediaSettings = mediaSettings;
        }
        public async Task<BlogPostListModel> Handle(GetBlogPostList request, CancellationToken cancellationToken)
        {
            var model = new BlogPostListModel();
            model.PagingFilteringContext.Tag = request.Command.Tag;
            model.PagingFilteringContext.Month = request.Command.Month;
            model.PagingFilteringContext.CategorySeName = request.Command.CategorySeName;
            model.WorkingLanguageId = _workContext.WorkingLanguage.Id;
            model.SearchKeyword = request.Command.SearchKeyword;

            if (request.Command.PageSize <= 0) request.Command.PageSize = _blogSettings.PostsPageSize;
            if (request.Command.PageNumber <= 0) request.Command.PageNumber = 1;

            DateTime? dateFrom = request.Command.GetFromMonth();
            DateTime? dateTo = request.Command.GetToMonth();

            IPagedList<BlogPost> blogPosts;
            if (string.IsNullOrEmpty(request.Command.CategorySeName))
            {
                if (string.IsNullOrEmpty(request.Command.Tag))
                {
                    blogPosts = await _blogService.GetAllBlogPosts(_storeContext.CurrentStore.Id,
                        dateFrom, dateTo, request.Command.PageNumber - 1, request.Command.PageSize, blogPostName: model.SearchKeyword);
                }
                else
                {
                    blogPosts = await _blogService.GetAllBlogPostsByTag(_storeContext.CurrentStore.Id,
                        request.Command.Tag, request.Command.PageNumber - 1, request.Command.PageSize);
                }
            }
            else
            {
                var categoryblog = await _blogService.GetBlogCategoryBySeName(request.Command.CategorySeName);
                var categoryId = categoryblog != null ? categoryblog.Id : "";
                blogPosts = await _blogService.GetAllBlogPosts(_storeContext.CurrentStore.Id,
                        dateFrom, dateTo, request.Command.PageNumber - 1, request.Command.PageSize, categoryId: categoryId, blogPostName: model.SearchKeyword);
            }
            model.PagingFilteringContext.LoadPagedList(blogPosts);

            foreach (var blogpost in blogPosts)
            {
                var blogPostModel = new BlogPostModel();
                await PrepareBlogPostModel(blogPostModel, blogpost);
                model.BlogPosts.Add(blogPostModel);
            }

            return model;
        }

        private async Task PrepareBlogPostModel(BlogPostModel model, BlogPost blogPost)
        {
            if (blogPost == null)
                throw new ArgumentNullException("blogPost");

            if (model == null)
                throw new ArgumentNullException("model");

            model.Id = blogPost.Id;
            model.MetaTitle = blogPost.GetLocalized(x => x.MetaTitle, _workContext.WorkingLanguage.Id);
            model.MetaDescription = blogPost.GetLocalized(x => x.MetaDescription, _workContext.WorkingLanguage.Id);
            model.MetaKeywords = blogPost.GetLocalized(x => x.MetaKeywords, _workContext.WorkingLanguage.Id);
            model.SeName = blogPost.GetSeName(_workContext.WorkingLanguage.Id);
            model.Title = blogPost.GetLocalized(x => x.Title, _workContext.WorkingLanguage.Id);
            model.Body = blogPost.GetLocalized(x => x.Body, _workContext.WorkingLanguage.Id);
            model.BodyOverview = blogPost.GetLocalized(x => x.BodyOverview, _workContext.WorkingLanguage.Id);
            model.AllowComments = blogPost.AllowComments;
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(blogPost.StartDateUtc ?? blogPost.CreatedOnUtc, DateTimeKind.Utc);
            model.Tags = blogPost.ParseTags().ToList();
            model.NumberOfComments = blogPost.CommentCount;
            model.GenericAttributes = blogPost.GenericAttributes;

            //prepare picture model
            await PrepareBlogPostPictureModel(model, blogPost);
        }

        private async Task PrepareBlogPostPictureModel(BlogPostModel model, BlogPost blogPost)
        {
            if (!string.IsNullOrEmpty(blogPost.PictureId))
            {
                var pictureModel = new PictureModel {
                    Id = blogPost.PictureId,
                    FullSizeImageUrl = await _pictureService.GetPictureUrl(blogPost.PictureId),
                    ImageUrl = await _pictureService.GetPictureUrl(blogPost.PictureId, _mediaSettings.BlogThumbPictureSize),
                    Title = string.Format(_localizationService.GetResource("Media.Blog.ImageLinkTitleFormat"), blogPost.Title),
                    AlternateText = string.Format(_localizationService.GetResource("Media.Blog.ImageAlternateTextFormat"), blogPost.Title)
                };

                model.PictureModel = pictureModel;
            }
        }
    }
}
