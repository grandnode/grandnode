using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Blogs;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Web.Areas.Admin.Models.Blogs;
using Grand.Web.Areas.Admin.Services;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Blog)]
    public partial class BlogController : BaseAdminController
    {
        #region Fields

        private readonly IBlogService _blogService;
        private readonly IBlogViewModelService _blogViewModelService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreService _storeService;

        #endregion

        #region Constructors

        public BlogController(IBlogService blogService, IBlogViewModelService blogViewModelService, ILanguageService languageService, ILocalizationService localizationService,
            IStoreService storeService)
        {
            this._blogService = blogService;
            this._blogViewModelService = blogViewModelService;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._storeService = storeService;
        }

        #endregion

        #region Blog posts

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public IActionResult List()
        {
            return View();
        }

        [HttpPost]
        public IActionResult List(DataSourceRequest command)
        {
            var blogPosts = _blogViewModelService.PrepareBlogPostsModel(command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = blogPosts.blogPosts,
                Total = blogPosts.totalCount
            };
            return Json(gridModel);
        }

        public IActionResult Create()
        {
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            var model = _blogViewModelService.PrepareBlogPostModel();
            //locales
            AddLocales(_languageService, model.Locales);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(BlogPostModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var blogPost = _blogViewModelService.InsertBlogPostModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Blog.BlogPosts.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = blogPost.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            model = _blogViewModelService.PrepareBlogPostModel(model);
            return View(model);
        }

        public IActionResult Edit(string id)
        {
            var blogPost = _blogService.GetBlogPostById(id);
            if (blogPost == null)
                //No blog post found with the specified id
                return RedirectToAction("List");

            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            var model = _blogViewModelService.PrepareBlogPostModel(blogPost);
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
            var blogPost = _blogService.GetBlogPostById(model.Id);
            if (blogPost == null)
                //No blog post found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                blogPost = _blogViewModelService.UpdateBlogPostModel(model, blogPost);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Blog.BlogPosts.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new { id = blogPost.Id });
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);

            model = _blogViewModelService.PrepareBlogPostModel(model, blogPost);
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
            var blogPost = _blogService.GetBlogPostById(id);
            if (blogPost == null)
                //No blog post found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                _blogService.DeleteBlogPost(blogPost);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Blog.BlogPosts.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("Edit", new { id = id });
        }

        #endregion

        #region Comments

        public IActionResult Comments(string filterByBlogPostId)
        {
            ViewBag.FilterByBlogPostId = filterByBlogPostId;
            return View();
        }

        [HttpPost]
        public IActionResult Comments(string filterByBlogPostId, DataSourceRequest command)
        {
            var model = _blogViewModelService.PrepareBlogPostCommentsModel(filterByBlogPostId, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = model.blogComments,
                Total = model.totalCount,
            };
            return Json(gridModel);
        }

        public IActionResult CommentDelete(string id)
        {
            var comment = _blogService.GetBlogCommentById(id);
            if (comment == null)
                throw new ArgumentException("No comment found with the specified id");

            var blogPost = _blogService.GetBlogPostById(comment.BlogPostId);
            if (ModelState.IsValid)
            {
                _blogService.DeleteBlogComment(comment);
                //update totals
                blogPost.CommentCount = _blogService.GetBlogCommentsByBlogPostId(blogPost.Id).Count;
                _blogService.UpdateBlogPost(blogPost);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }
        #endregion
    }
}
