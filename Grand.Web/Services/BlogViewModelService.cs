using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Domain.Blogs;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Media;
using Grand.Framework.Security.Captcha;
using Grand.Services.Blogs;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Media;
using Grand.Services.Messages;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Interfaces;
using Grand.Web.Models.Blogs;
using Grand.Web.Models.Media;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Services
{
    public partial class BlogViewModelService : IBlogViewModelService
    {
        private readonly IBlogService _blogService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IWebHelper _webHelper;
        private readonly ICacheManager _cacheManager;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IPermissionService _permissionService;
        private readonly IServiceProvider _serviceProvider;

        private readonly MediaSettings _mediaSettings;
        private readonly BlogSettings _blogSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly CaptchaSettings _captchaSettings;

        public BlogViewModelService(IBlogService blogService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IPictureService pictureService,
            ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper,
            IWorkflowMessageService workflowMessageService,
            IWebHelper webHelper,
            ICacheManager cacheManager,
            ICustomerActivityService customerActivityService,
            IStoreMappingService storeMappingService,
            IPermissionService permissionService,
            IServiceProvider serviceProvider,
            MediaSettings mediaSettings,
            BlogSettings blogSettings,
            LocalizationSettings localizationSettings,
            CustomerSettings customerSettings,
            CaptchaSettings captchaSettings)
        {
            _blogService = blogService;
            _workContext = workContext;
            _storeContext = storeContext;
            _pictureService = pictureService;
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;
            _workflowMessageService = workflowMessageService;
            _webHelper = webHelper;
            _cacheManager = cacheManager;
            _customerActivityService = customerActivityService;
            _storeMappingService = storeMappingService;
            _permissionService = permissionService;
            _serviceProvider = serviceProvider;
            _mediaSettings = mediaSettings;
            _blogSettings = blogSettings;
            _localizationSettings = localizationSettings;
            _customerSettings = customerSettings;
            _captchaSettings = captchaSettings;
        }

        public async Task<HomePageBlogItemsModel> PrepareHomePageBlogItems()
        {
            var cacheKey = string.Format(ModelCacheEventConsumer.BLOG_HOMEPAGE_MODEL_KEY, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);
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
                    item.Short = description?.Length > _blogSettings.MaxTextSizeHomePage ? description.Substring(0, _blogSettings.MaxTextSizeHomePage): description;
                    item.CreatedOn = _dateTimeHelper.ConvertToUserTime(post.StartDateUtc ?? post.CreatedOnUtc, DateTimeKind.Utc);

                    //prepare picture model
                    if (!string.IsNullOrEmpty(post.PictureId))
                    {
                        int pictureSize = _mediaSettings.BlogThumbPictureSize;
                        var categoryPictureCacheKey = string.Format(ModelCacheEventConsumer.BLOG_PICTURE_MODEL_KEY, post.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
                        item.PictureModel = await _cacheManager.GetAsync(categoryPictureCacheKey, async () =>
                        {
                            var picture = await _pictureService.GetPictureById(post.PictureId);
                            var pictureModel = new PictureModel
                            {
                                Id = post.PictureId,
                                FullSizeImageUrl = await _pictureService.GetPictureUrl(picture),
                                ImageUrl = await _pictureService.GetPictureUrl(picture, pictureSize),
                                Title = string.Format(_localizationService.GetResource("Media.Blog.ImageLinkTitleFormat"), post.Title),
                                AlternateText = string.Format(_localizationService.GetResource("Media.Blog.ImageAlternateTextFormat"), post.Title)
                            };
                            return pictureModel;
                        });
                    }
                    model.Items.Add(item);
                }
                return model;
            });

            return cachedModel;
        }

        public async Task<BlogCommentModel> PrepareBlogPostCommentModel(BlogComment blogComment)
        {
            var customer = await _serviceProvider.GetRequiredService<ICustomerService>().GetCustomerById(blogComment.CustomerId);
            var model = new BlogCommentModel
            {
                Id = blogComment.Id,
                CustomerId = blogComment.CustomerId,
                CustomerName = customer.FormatUserName(_customerSettings.CustomerNameFormat),
                CommentText = blogComment.CommentText,
                CreatedOn = _dateTimeHelper.ConvertToUserTime(blogComment.CreatedOnUtc, DateTimeKind.Utc),
                AllowViewingProfiles = _customerSettings.AllowViewingProfiles && customer != null && !customer.IsGuest(),
            };
            if (_customerSettings.AllowCustomersToUploadAvatars)
            {
                model.CustomerAvatarUrl = await _pictureService.GetPictureUrl(
                    customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.AvatarPictureId),
                    _mediaSettings.AvatarPictureSize,
                    _customerSettings.DefaultAvatarEnabled,
                    defaultPictureType: PictureType.Avatar);
            }

            return model;
        }

        public async Task<BlogPostListModel> PrepareBlogPostListModel(BlogPagingFilteringModel command)
        {
            if (command == null)
                throw new ArgumentNullException("command");

            var model = new BlogPostListModel();
            model.PagingFilteringContext.Tag = command.Tag;
            model.PagingFilteringContext.Month = command.Month;
            model.PagingFilteringContext.CategoryId = command.CategoryId;
            model.WorkingLanguageId = _workContext.WorkingLanguage.Id;
            model.SearchKeyword = command.SearchKeyword;

            if (command.PageSize <= 0) command.PageSize = _blogSettings.PostsPageSize;
            if (command.PageNumber <= 0) command.PageNumber = 1;

            DateTime? dateFrom = command.GetFromMonth();
            DateTime? dateTo = command.GetToMonth();

            IPagedList<BlogPost> blogPosts;
            if (string.IsNullOrEmpty(command.CategoryId))
            {
                if (String.IsNullOrEmpty(command.Tag))
                {
                    blogPosts = await _blogService.GetAllBlogPosts(_storeContext.CurrentStore.Id,
                        dateFrom, dateTo, command.PageNumber - 1, command.PageSize, blogPostName: model.SearchKeyword);
                }
                else
                {
                    blogPosts = await _blogService.GetAllBlogPostsByTag(_storeContext.CurrentStore.Id,
                        command.Tag, command.PageNumber - 1, command.PageSize);
                }
            }
            else
            {
                blogPosts = await _blogService.GetAllBlogPosts(_storeContext.CurrentStore.Id,
                        dateFrom, dateTo, command.PageNumber - 1, command.PageSize, categoryId: command.CategoryId, blogPostName: model.SearchKeyword);
            }
            model.PagingFilteringContext.LoadPagedList(blogPosts);

            model.BlogPosts = blogPosts
                .Select(x =>
                {
                    var blogPostModel = new BlogPostModel();
                    PrepareBlogPostModel(blogPostModel, x, false).Wait();
                    return blogPostModel;
                })
                .ToList();

            return model;
        }

        public async Task PrepareBlogPostModel(BlogPostModel model, BlogPost blogPost, bool prepareComments)
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
            model.AddNewComment.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnBlogCommentPage;
            if (prepareComments)
            {
                var blogComments = await _blogService.GetBlogCommentsByBlogPostId(blogPost.Id);
                foreach (var bc in blogComments)
                {
                    var commentModel = await PrepareBlogPostCommentModel(bc);
                    model.Comments.Add(commentModel);
                }
            }

            //prepare picture model
            if (!string.IsNullOrEmpty(blogPost.PictureId))
            {
                int pictureSize = _mediaSettings.BlogThumbPictureSize;
                var categoryPictureCacheKey = string.Format(ModelCacheEventConsumer.BLOG_PICTURE_MODEL_KEY, blogPost.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
                model.PictureModel = await _cacheManager.GetAsync(categoryPictureCacheKey, async () =>
                {
                    var picture = await _pictureService.GetPictureById(blogPost.PictureId);
                    var pictureModel = new PictureModel
                    {
                        Id = blogPost.PictureId,
                        FullSizeImageUrl = await _pictureService.GetPictureUrl(picture),
                        ImageUrl = await _pictureService.GetPictureUrl(picture, pictureSize),
                        Title = string.Format(_localizationService.GetResource("Media.Blog.ImageLinkTitleFormat"), blogPost.Title),
                        AlternateText = string.Format(_localizationService.GetResource("Media.Blog.ImageAlternateTextFormat"), blogPost.Title)
                    };
                    return pictureModel;
                });
            }

        }

        public async Task<BlogPostTagListModel> PrepareBlogPostTagListModel()
        {
            var cacheKey = string.Format(ModelCacheEventConsumer.BLOG_TAGS_MODEL_KEY, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);
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
                    model.Tags.Add(new BlogPostTagModel
                    {
                        Name = tag.Name,
                        BlogPostCount = tag.BlogPostCount
                    });
                return model;
            });
            return cachedModel;
        }

        public async Task<List<BlogPostYearModel>> PrepareBlogPostYearModel()
        {
            var cacheKey = string.Format(ModelCacheEventConsumer.BLOG_MONTHS_MODEL_KEY, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);
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
                            var yearModel = new BlogPostYearModel
                            {
                                Year = date.Year
                            };
                            model.Insert(0, yearModel);
                        }

                        model.First().Months.Insert(0, new BlogPostMonthModel
                        {
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

        public async Task<List<BlogPostCategoryModel>> PrepareBlogPostCategoryModel()
        {
            var cacheKey = string.Format(ModelCacheEventConsumer.BLOG_CATEGORY_MODEL_KEY, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);
            var cachedModel = await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var model = new List<BlogPostCategoryModel>();
                var categories = await _blogService.GetAllBlogCategories(_storeContext.CurrentStore.Id);
                foreach (var item in categories)
                {
                    model.Add(new BlogPostCategoryModel()
                    {
                        Id = item.Id,
                        Name = item.GetLocalized(x=>x.Name, _workContext.WorkingLanguage.Id),
                        BlogPostCount = item.BlogPosts.Count
                    });
                }  
                return model;
            });
            return cachedModel;
        }

        public async Task<BlogComment> InsertBlogComment(BlogPostModel model, BlogPost blogPost)
        {
            var customer = _workContext.CurrentCustomer;
            var comment = new BlogComment
            {
                BlogPostId = blogPost.Id,
                CustomerId = customer.Id,
                CommentText = model.AddNewComment.CommentText,
                CreatedOnUtc = DateTime.UtcNow,
                BlogPostTitle = blogPost.Title,
            };
            await _blogService.InsertBlogComment(comment);

            //update totals
            var comments = await _blogService.GetBlogCommentsByBlogPostId(blogPost.Id);
            blogPost.CommentCount = comments.Count;
            await _blogService.UpdateBlogPost(blogPost);
            if (!customer.HasContributions)
            {
                await _serviceProvider.GetRequiredService<ICustomerService>().UpdateContributions(customer);
            }
            //notify a store owner
            if (_blogSettings.NotifyAboutNewBlogComments)
                await _workflowMessageService.SendBlogCommentNotificationMessage(blogPost, comment, _localizationSettings.DefaultAdminLanguageId);

            //activity log
            await _customerActivityService.InsertActivity("PublicStore.AddBlogComment", comment.Id, _localizationService.GetResource("ActivityLog.PublicStore.AddBlogComment"));

            return comment;
        }
    }
}