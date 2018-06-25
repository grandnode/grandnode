using System;
using System.Collections.Generic;
using System.Linq;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Blogs;
using Grand.Core.Domain.Blogs;
using Grand.Core.Domain.Customers;
using Grand.Services.Blogs;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Core.Infrastructure;
using Grand.Services.Customers;
using MongoDB.Driver;
using Grand.Core.Domain.Localization;
using Microsoft.AspNetCore.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Extensions;
using Grand.Services.Media;

namespace Grand.Web.Areas.Admin.Controllers
{
    public partial class BlogController : BaseAdminController
	{
		#region Fields

        private readonly IBlogService _blogService;
        private readonly ILanguageService _languageService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IStoreService _storeService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IPictureService _pictureService;

        #endregion

        #region Constructors

        public BlogController(IBlogService blogService, ILanguageService languageService,
            IDateTimeHelper dateTimeHelper, 
            ILocalizationService localizationService, IPermissionService permissionService,
            IUrlRecordService urlRecordService,
            IStoreService storeService, IStoreMappingService storeMappingService,
            IPictureService pictureService)
        {
            this._blogService = blogService;
            this._languageService = languageService;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._urlRecordService = urlRecordService;
            this._storeService = storeService;
            this._storeMappingService = storeMappingService;
            this._pictureService = pictureService;

        }

		#endregion 
        
        #region Utilities

        [NonAction]
        protected virtual void PrepareStoresMappingModel(BlogPostModel model, BlogPost blogPost, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.AvailableStores = _storeService
                .GetAllStores()
                .Select(s => s.ToModel())
                .ToList();
            if (!excludeProperties)
            {
                if (blogPost != null)
                {
                    model.SelectedStoreIds = blogPost.Stores.ToArray();
                }
            }
        }

        [NonAction]
        protected virtual List<LocalizedProperty> UpdateLocales(BlogPost blogpost, BlogPostModel model)
        {
            List<LocalizedProperty> localized = new List<LocalizedProperty>();

            foreach (var local in model.Locales)
            {
                var seName = blogpost.ValidateSeName(local.SeName, local.Title, false);
                _urlRecordService.SaveSlug(blogpost, seName, local.LanguageId);

                if (!(String.IsNullOrEmpty(seName)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "SeName",
                        LocaleValue = seName
                    });

                if (!(String.IsNullOrEmpty(local.Title)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "Title",
                        LocaleValue = local.Title
                    });

                if (!(String.IsNullOrEmpty(local.BodyOverview)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "BodyOverview",
                        LocaleValue = local.BodyOverview
                    });

                if (!(String.IsNullOrEmpty(local.Body)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "Body",
                        LocaleValue = local.Body
                    });

                if (!(String.IsNullOrEmpty(local.MetaDescription)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "MetaDescription",
                        LocaleValue = local.MetaDescription
                    });

                if (!(String.IsNullOrEmpty(local.MetaKeywords)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "MetaKeywords",
                        LocaleValue = local.MetaKeywords
                    });

                if (!(String.IsNullOrEmpty(local.MetaTitle)))
                    localized.Add(new LocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        LocaleKey = "MetaTitle",
                        LocaleValue = local.MetaTitle
                    });

               

            }
            return localized;
        }

        [NonAction]
        protected virtual void UpdatePictureSeoNames(BlogPost blogpost)
        {
            var picture = _pictureService.GetPictureById(blogpost.PictureId);
            if (picture != null)
                _pictureService.SetSeoFilename(picture.Id, _pictureService.GetPictureSeName(blogpost.Title));
        }

        #endregion

        #region Blog posts

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

		public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageBlog))
                return AccessDeniedView();

			return View();
		}

		[HttpPost]
        public IActionResult List(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageBlog))
                return AccessDeniedView();

            var blogPosts = _blogService.GetAllBlogPosts("", null, null, command.Page - 1, command.PageSize, true);
            var gridModel = new DataSourceResult
            {
                Data = blogPosts.Select(x =>
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
                }),
                Total = blogPosts.TotalCount
            };
            return Json(gridModel);
        }
        
        public IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageBlog))
                return AccessDeniedView();

            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            var model = new BlogPostModel();
            //Stores
            PrepareStoresMappingModel(model, null, false);
            //default values
            model.AllowComments = true;
            //locales
            AddLocales(_languageService, model.Locales);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(BlogPostModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageBlog))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var blogPost = model.ToEntity();
                blogPost.StartDateUtc = model.StartDate;
                blogPost.EndDateUtc = model.EndDate;
                blogPost.CreatedOnUtc = DateTime.UtcNow;
                blogPost.Stores = model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>();
                _blogService.InsertBlogPost(blogPost);
                
                //search engine name
                var seName = blogPost.ValidateSeName(model.SeName, model.Title, true);
                blogPost.SeName = seName;
                blogPost.Locales = UpdateLocales(blogPost, model);
                _blogService.UpdateBlogPost(blogPost);
                _urlRecordService.SaveSlug(blogPost, seName, "");

                //update picture seo file name
                UpdatePictureSeoNames(blogPost);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Blog.BlogPosts.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = blogPost.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            //Stores
            PrepareStoresMappingModel(model, null, true);
            return View(model);
        }

		public IActionResult Edit(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageBlog))
                return AccessDeniedView();

            var blogPost = _blogService.GetBlogPostById(id);
            if (blogPost == null)
                //No blog post found with the specified id
                return RedirectToAction("List");

            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            var model = blogPost.ToModel();
            model.StartDate = blogPost.StartDateUtc;
            model.EndDate = blogPost.EndDateUtc;
            //Store
            PrepareStoresMappingModel(model, blogPost, false);
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Title = blogPost.GetLocalized(x => x.Title, languageId, false, false);
                locale.Body = blogPost.GetLocalized(x => x.Body, languageId, false, false);
                locale.BodyOverview = blogPost.GetLocalized(x => x.BodyOverview, languageId, false, false);
                locale.MetaKeywords = blogPost.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaDescription = blogPost.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaTitle = blogPost.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = blogPost.GetSeName(languageId, false, false);
            });
            return View(model);
		}

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
		public IActionResult Edit(BlogPostModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageBlog))
                return AccessDeniedView();

            var blogPost = _blogService.GetBlogPostById(model.Id);
            if (blogPost == null)
                //No blog post found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                string prevPictureId = blogPost.PictureId;

                blogPost = model.ToEntity(blogPost);
                blogPost.StartDateUtc = model.StartDate;
                blogPost.EndDateUtc = model.EndDate;
                blogPost.Stores = model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>();

                _blogService.UpdateBlogPost(blogPost);

                //search engine name
                var seName = blogPost.ValidateSeName(model.SeName, model.Title, true);
                blogPost.SeName = seName;
                blogPost.Locales = UpdateLocales(blogPost, model);
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
                UpdatePictureSeoNames(blogPost);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Blog.BlogPosts.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new {id = blogPost.Id});
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            //Store
            PrepareStoresMappingModel(model, blogPost, true);
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Title = blogPost.GetLocalized(x => x.Title, languageId, false, false);
                locale.Body = blogPost.GetLocalized(x => x.Body, languageId, false, false);
                locale.BodyOverview = blogPost.GetLocalized(x => x.BodyOverview, languageId, false, false);
                locale.MetaKeywords = blogPost.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaDescription = blogPost.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaTitle = blogPost.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = blogPost.GetSeName(languageId, false, false);
            });
            return View(model);
		}

		[HttpPost]
		public IActionResult Delete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageBlog))
                return AccessDeniedView();

            var blogPost = _blogService.GetBlogPostById(id);
            if (blogPost == null)
                //No blog post found with the specified id
                return RedirectToAction("List");

            _blogService.DeleteBlogPost(blogPost);

            SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Blog.BlogPosts.Deleted"));
			return RedirectToAction("List");
		}

		#endregion

        #region Comments

        public IActionResult Comments(string filterByBlogPostId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageBlog))
                return AccessDeniedView();

            ViewBag.FilterByBlogPostId = filterByBlogPostId;
            return View();
        }

        [HttpPost]
        public IActionResult Comments(string filterByBlogPostId, DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageBlog))
                return AccessDeniedView();

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

            var gridModel = new DataSourceResult
            {
                Data = comments.PagedForCommand(command).Select(blogComment =>
                {
                    var commentModel = new BlogCommentModel();
                    commentModel.Id = blogComment.Id;
                    commentModel.BlogPostId = blogComment.BlogPostId;
                    commentModel.BlogPostTitle = blogComment.BlogPostTitle;
                    commentModel.CustomerId = blogComment.CustomerId;
                    var customer = EngineContext.Current.Resolve<ICustomerService>().GetCustomerById(blogComment.CustomerId);
                    commentModel.CustomerInfo = customer.IsRegistered() ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest");
                    commentModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(blogComment.CreatedOnUtc, DateTimeKind.Utc);
                    commentModel.Comment = Core.Html.HtmlHelper.FormatText(blogComment.CommentText, false, true, false, false, false, false);
                    return commentModel;
                }),
                Total = comments.Count,
            };
            return Json(gridModel);
        }

        public IActionResult CommentDelete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageBlog))
                return AccessDeniedView();

            var comment = _blogService.GetBlogCommentById(id);
            if (comment == null)
                throw new ArgumentException("No comment found with the specified id");

            var blogPost = _blogService.GetBlogPostById(comment.BlogPostId);
            
            _blogService.DeleteBlogComment(comment);
            //update totals
            blogPost.CommentCount = _blogService.GetBlogCommentsByBlogPostId(blogPost.Id).Count;
            _blogService.UpdateBlogPost(blogPost);
            
            return new NullJsonResult();
        }


        #endregion
    }
}
