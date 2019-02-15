using Grand.Core.Domain.Blogs;
using Grand.Core.Domain.Customers;
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

        public BlogViewModelService(IBlogService blogService, IDateTimeHelper dateTimeHelper, IStoreService storeService, IUrlRecordService urlRecordService,
            IStoreMappingService storeMappingService, IPictureService pictureService, ICustomerService customerService, ILocalizationService localizationService)
        {
            _blogService = blogService;
            _dateTimeHelper = dateTimeHelper;
            _storeService = storeService;
            _storeMappingService = storeMappingService;
            _urlRecordService = urlRecordService;
            _pictureService = pictureService;
            _customerService = customerService;
            _localizationService = localizationService;
        }

        public virtual (IEnumerable<BlogPostModel> blogPosts, int totalCount) PrepareBlogPostsModel(int pageIndex, int pageSize)
        {
            var blogPosts = _blogService.GetAllBlogPosts("", null, null, pageIndex - 1, pageSize, true);
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
        public virtual BlogPostModel PrepareBlogPostModel()
        {
            var model = new BlogPostModel();
            //Stores
            model.PrepareStoresMappingModel(null, false, _storeService);
            //default values
            model.AllowComments = true;
            //locales
            return model;
        }

        public virtual BlogPostModel PrepareBlogPostModel(BlogPostModel blogPostmodel)
        {
            blogPostmodel.PrepareStoresMappingModel(null, true, _storeService);
            return blogPostmodel;
        }
        public virtual BlogPostModel PrepareBlogPostModel(BlogPostModel blogPostmodel, BlogPost blogPost)
        {
            //Store
            blogPostmodel.PrepareStoresMappingModel(blogPost, true, _storeService);
            return blogPostmodel;
        }

        public virtual BlogPostModel PrepareBlogPostModel(BlogPost blogPost)
        {
            var model = blogPost.ToModel();
            //Store
            model.PrepareStoresMappingModel(blogPost, false, _storeService);
            return model;
        }

        public virtual BlogPost InsertBlogPostModel(BlogPostModel model)
        {
            var blogPost = model.ToEntity();
            blogPost.CreatedOnUtc = DateTime.UtcNow;
            _blogService.InsertBlogPost(blogPost);

            //search engine name
            var seName = blogPost.ValidateSeName(model.SeName, model.Title, true);
            blogPost.SeName = seName;
            blogPost.Locales = model.Locales.ToLocalizedProperty(blogPost, x => x.Title, _urlRecordService);
            _blogService.UpdateBlogPost(blogPost);
            _urlRecordService.SaveSlug(blogPost, seName, "");

            //update picture seo file name
            _pictureService.UpdatePictureSeoNames(blogPost.PictureId, blogPost.Title);

            return blogPost;
        }

        public virtual BlogPost UpdateBlogPostModel(BlogPostModel model, BlogPost blogPost)
        {
            string prevPictureId = blogPost.PictureId;
            blogPost = model.ToEntity(blogPost);
            _blogService.UpdateBlogPost(blogPost);

            //search engine name
            var seName = blogPost.ValidateSeName(model.SeName, model.Title, true);
            blogPost.SeName = seName;
            blogPost.Locales = model.Locales.ToLocalizedProperty(blogPost, x => x.Title, _urlRecordService);
            _blogService.UpdateBlogPost(blogPost);
            _urlRecordService.SaveSlug(blogPost, seName, "");

            //delete an old picture (if deleted or updated)
            if (!String.IsNullOrEmpty(prevPictureId) && prevPictureId != blogPost.PictureId)
            {
                var prevPicture = _pictureService.GetPictureById(prevPictureId);
                if (prevPicture != null)
                    _pictureService.DeletePicture(prevPicture);
            }

            //update picture seo file name
            _pictureService.UpdatePictureSeoNames(blogPost.PictureId, blogPost.Title);

            return blogPost;
        }
        public virtual (IEnumerable<BlogCommentModel> blogComments, int totalCount) PrepareBlogPostCommentsModel(string filterByBlogPostId, int pageIndex, int pageSize)
        {
            IList<BlogComment> comments;
            if (!String.IsNullOrEmpty(filterByBlogPostId))
            {
                //filter comments by blog
                var blogPost = _blogService.GetBlogPostById(filterByBlogPostId);
                comments = _blogService.GetBlogCommentsByBlogPostId(blogPost.Id);
            }
            else
            {
                //load all blog comments
                comments = _blogService.GetAllComments("");
            }

            return (comments.Skip((pageIndex - 1) * pageSize).Take(pageSize).Select(blogComment =>
                {
                    var commentModel = new BlogCommentModel();
                    commentModel.Id = blogComment.Id;
                    commentModel.BlogPostId = blogComment.BlogPostId;
                    commentModel.BlogPostTitle = blogComment.BlogPostTitle;
                    commentModel.CustomerId = blogComment.CustomerId;
                    var customer = _customerService.GetCustomerById(blogComment.CustomerId);
                    commentModel.CustomerInfo = customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest");
                    commentModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(blogComment.CreatedOnUtc, DateTimeKind.Utc);
                    commentModel.Comment = Core.Html.HtmlHelper.FormatText(blogComment.CommentText, false, true, false, false, false, false);
                    return commentModel;
                }), comments.Count);
        }
    }
}
