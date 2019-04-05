using Grand.Core.Domain.Blogs;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Seo;
using Grand.Services.Blogs;
using Grand.Services.Customers;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Blogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Services
{
    public partial class BlogViewModelService : IBlogViewModelService
    {
        private readonly IBlogService _blogService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IStoreService _storeService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IPictureService _pictureService;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly SeoSettings _seoSettings;

        public BlogViewModelService(IBlogService blogService, IDateTimeHelper dateTimeHelper, IStoreService storeService, IUrlRecordService urlRecordService,
            IStoreMappingService storeMappingService, IPictureService pictureService, ICustomerService customerService, ILocalizationService localizationService,
            ILanguageService languageService, SeoSettings seoSettings)
        {
            _blogService = blogService;
            _dateTimeHelper = dateTimeHelper;
            _storeService = storeService;
            _storeMappingService = storeMappingService;
            _urlRecordService = urlRecordService;
            _pictureService = pictureService;
            _customerService = customerService;
            _localizationService = localizationService;
            _languageService = languageService;
            _seoSettings = seoSettings;
        }

        public virtual async Task<(IEnumerable<BlogPostModel> blogPosts, int totalCount)> PrepareBlogPostsModel(int pageIndex, int pageSize)
        {
            var blogPosts = await _blogService.GetAllBlogPosts("", null, null, pageIndex - 1, pageSize, true);
            return (blogPosts.Select(x =>
                {
                    var m = x.ToModel();
                    m.Body = "";
                    if (x.StartDateUtc.HasValue)
                        m.StartDate = _dateTimeHelper.ConvertToUserTime(x.StartDateUtc.Value, DateTimeKind.Utc);
                    if (x.EndDateUtc.HasValue)
                        m.EndDate = _dateTimeHelper.ConvertToUserTime(x.EndDateUtc.Value, DateTimeKind.Utc);
                    m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                    m.Comments = x.CommentCount;
                    return m;
                }), blogPosts.TotalCount);
        }
        public virtual async Task<BlogPostModel> PrepareBlogPostModel()
        {
            var model = new BlogPostModel();
            //Stores
            await model.PrepareStoresMappingModel(null, false, _storeService);
            //default values
            model.AllowComments = true;
            //locales
            return model;
        }

        public virtual async Task<BlogPostModel> PrepareBlogPostModel(BlogPostModel blogPostmodel)
        {
            await blogPostmodel.PrepareStoresMappingModel(null, true, _storeService);
            return blogPostmodel;
        }
        public virtual async Task<BlogPostModel> PrepareBlogPostModel(BlogPostModel blogPostmodel, BlogPost blogPost)
        {
            //Store
            await blogPostmodel.PrepareStoresMappingModel(blogPost, true, _storeService);
            return blogPostmodel;
        }

        public virtual async Task<BlogPostModel> PrepareBlogPostModel(BlogPost blogPost)
        {
            var model = blogPost.ToModel();
            //Store
            await model.PrepareStoresMappingModel(blogPost, false, _storeService);
            return model;
        }

        public virtual async Task<BlogPost> InsertBlogPostModel(BlogPostModel model)
        {
            var blogPost = model.ToEntity();
            blogPost.CreatedOnUtc = DateTime.UtcNow;
            await _blogService.InsertBlogPost(blogPost);

            //search engine name
            var seName = await blogPost.ValidateSeName(model.SeName, model.Title, true, _seoSettings, _urlRecordService, _languageService);
            blogPost.SeName = seName;
            blogPost.Locales = await model.Locales.ToLocalizedProperty(blogPost, x => x.Title, _seoSettings, _urlRecordService, _languageService);
            await _blogService.UpdateBlogPost(blogPost);
            await _urlRecordService.SaveSlug(blogPost, seName, "");

            //update picture seo file name
            await _pictureService.UpdatePictureSeoNames(blogPost.PictureId, blogPost.Title);

            return blogPost;
        }

        public virtual async Task<BlogPost> UpdateBlogPostModel(BlogPostModel model, BlogPost blogPost)
        {
            string prevPictureId = blogPost.PictureId;
            blogPost = model.ToEntity(blogPost);
            await _blogService.UpdateBlogPost(blogPost);

            //search engine name
            var seName = await blogPost.ValidateSeName(model.SeName, model.Title, true, _seoSettings, _urlRecordService, _languageService);
            blogPost.SeName = seName;
            blogPost.Locales = await model.Locales.ToLocalizedProperty(blogPost, x => x.Title, _seoSettings, _urlRecordService, _languageService);
            await _blogService.UpdateBlogPost(blogPost);
            await _urlRecordService.SaveSlug(blogPost, seName, "");

            //delete an old picture (if deleted or updated)
            if (!String.IsNullOrEmpty(prevPictureId) && prevPictureId != blogPost.PictureId)
            {
                var prevPicture = await _pictureService.GetPictureById(prevPictureId);
                if (prevPicture != null)
                    await _pictureService.DeletePicture(prevPicture);
            }

            //update picture seo file name
            await _pictureService.UpdatePictureSeoNames(blogPost.PictureId, blogPost.Title);

            return blogPost;
        }
        public virtual async Task<(IEnumerable<BlogCommentModel> blogComments, int totalCount)> PrepareBlogPostCommentsModel(string filterByBlogPostId, int pageIndex, int pageSize)
        {
            IList<BlogComment> comments;
            if (!String.IsNullOrEmpty(filterByBlogPostId))
            {
                //filter comments by blog
                var blogPost = await _blogService.GetBlogPostById(filterByBlogPostId);
                comments = await _blogService.GetBlogCommentsByBlogPostId(blogPost.Id);
            }
            else
            {
                //load all blog comments
                comments = await _blogService.GetAllComments("");
            }
            var commentsList = new List<BlogCommentModel>();
            foreach (var blogComment in comments.Skip((pageIndex - 1) * pageSize).Take(pageSize))
            {
                var commentModel = new BlogCommentModel
                {
                    Id = blogComment.Id,
                    BlogPostId = blogComment.BlogPostId,
                    BlogPostTitle = blogComment.BlogPostTitle,
                    CustomerId = blogComment.CustomerId
                };
                var customer = await _customerService.GetCustomerById(blogComment.CustomerId);
                commentModel.CustomerInfo = customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest");
                commentModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(blogComment.CreatedOnUtc, DateTimeKind.Utc);
                commentModel.Comment = Core.Html.HtmlHelper.FormatText(blogComment.CommentText, false, true, false, false, false, false);
                commentsList.Add(commentModel);
            }
            return (commentsList, comments.Count);
        }
    }
}
