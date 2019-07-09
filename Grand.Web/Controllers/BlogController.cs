using Grand.Core;
using Grand.Core.Domain.Blogs;
using Grand.Core.Domain.Customers;
using Grand.Framework.Controllers;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Mvc.Rss;
using Grand.Framework.Security;
using Grand.Framework.Security.Captcha;
using Grand.Services.Blogs;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Web.Models.Blogs;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Controllers
{
    public partial class BlogController : BasePublicController
    {
        #region Fields

        private readonly IBlogViewModelService _blogViewModelService;
        private readonly IBlogService _blogService;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly BlogSettings _blogSettings;
        private readonly CaptchaSettings _captchaSettings;

        #endregion

        #region Constructors

        public BlogController(IBlogViewModelService blogViewModelService,
            IBlogService blogService, 
            IStoreContext storeContext,
            ILocalizationService localizationService,
            IWebHelper webHelper,
            IWorkContext workContext,
            BlogSettings blogSettings,
            CaptchaSettings captchaSettings)
        {
            this._blogViewModelService = blogViewModelService;
            this._blogService = blogService;
            this._storeContext = storeContext;
            this._localizationService = localizationService;
            this._webHelper = webHelper;
            this._blogSettings = blogSettings;
            this._captchaSettings = captchaSettings;
            this._workContext = workContext;
        }

		#endregion

        #region Methods

        public virtual async Task<IActionResult> List(BlogPagingFilteringModel command)
        {
            if (!_blogSettings.Enabled)
                return RedirectToRoute("HomePage");
            
            var model = await _blogViewModelService.PrepareBlogPostListModel(command);
            return View("List", model);
        }
        public virtual async Task<IActionResult> BlogByTag(BlogPagingFilteringModel command)
        {
            if (!_blogSettings.Enabled)
                return RedirectToRoute("HomePage");

            var model = await _blogViewModelService.PrepareBlogPostListModel(command);
            return View("List", model);
        }
        public virtual async Task<IActionResult> BlogByMonth(BlogPagingFilteringModel command)
        {
            if (!_blogSettings.Enabled)
                return RedirectToRoute("HomePage");

            var model = await _blogViewModelService.PrepareBlogPostListModel(command);
            return View("List", model);
        }
        public virtual async Task<IActionResult> BlogByCategory(BlogPagingFilteringModel command)
        {
            if (!_blogSettings.Enabled)
                return RedirectToRoute("HomePage");

            var model = await _blogViewModelService.PrepareBlogPostListModel(command);
            return View("List", model);
        }
        public virtual async Task<IActionResult> BlogByKeyword(BlogPagingFilteringModel command)
        {
            if (!_blogSettings.Enabled)
                return RedirectToRoute("HomePage");

            var model = await _blogViewModelService.PrepareBlogPostListModel(command);
            return View("List", model);
        }
        public virtual async Task<IActionResult> ListRss(string languageId)
        {
            var feed = new RssFeed(
                string.Format("{0}: Blog", _storeContext.CurrentStore.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id)),
                "Blog",
                new Uri(_webHelper.GetStoreLocation()),
                DateTime.UtcNow);

            if (!_blogSettings.Enabled)
                return new RssActionResult(feed, _webHelper.GetThisPageUrl(false));

            var items = new List<RssItem>();
            var blogPosts = await _blogService.GetAllBlogPosts(_storeContext.CurrentStore.Id);
            foreach (var blogPost in blogPosts)
            {
                string blogPostUrl = Url.RouteUrl("BlogPost", new { SeName = blogPost.GetSeName(_workContext.WorkingLanguage.Id) }, _webHelper.IsCurrentConnectionSecured() ? "https" : "http");
                items.Add(new RssItem(blogPost.Title, blogPost.Body, new Uri(blogPostUrl), String.Format("urn:store:{0}:blog:post:{1}", _storeContext.CurrentStore.Id, blogPost.Id), blogPost.CreatedOnUtc));
            }
            feed.Items = items;
            return new RssActionResult(feed, _webHelper.GetThisPageUrl(false));
        }

        public virtual async Task<IActionResult> BlogPost(string blogPostId,
            [FromServices] IStoreMappingService storeMappingService,
            [FromServices] IPermissionService permissionService)
        {
            if (!_blogSettings.Enabled)
                return RedirectToRoute("HomePage");

            var blogPost = await _blogService.GetBlogPostById(blogPostId);
            if (blogPost == null ||
                (blogPost.StartDateUtc.HasValue && blogPost.StartDateUtc.Value >= DateTime.UtcNow) ||
                (blogPost.EndDateUtc.HasValue && blogPost.EndDateUtc.Value <= DateTime.UtcNow))
                return RedirectToRoute("HomePage");

            //Store mapping
            if (!storeMappingService.Authorize(blogPost))
                return InvokeHttp404();
            
            var model = new BlogPostModel();
            await _blogViewModelService.PrepareBlogPostModel(model, blogPost, true);

            //display "edit" (manage) link
            if (await permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) && await permissionService.Authorize(StandardPermissionProvider.ManageBlog))
                DisplayEditLink(Url.Action("Edit", "Blog", new { id = blogPost.Id, area = "Admin" }));

            return View(model);
        }

        [HttpPost, ActionName("BlogPost")]
        [PublicAntiForgery]
        [FormValueRequired("add-comment")]
        [ValidateCaptcha]
        public virtual async Task<IActionResult> BlogCommentAdd(string blogPostId, BlogPostModel model, bool captchaValid,
                       [FromServices] IWorkContext workContext)
        {
            if (!_blogSettings.Enabled)
                return RedirectToRoute("HomePage");

            var blogPost = await _blogService.GetBlogPostById(blogPostId);
            if (blogPost == null || !blogPost.AllowComments)
                return RedirectToRoute("HomePage");

            if (workContext.CurrentCustomer.IsGuest() && !_blogSettings.AllowNotRegisteredUsersToLeaveComments)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Blog.Comments.OnlyRegisteredUsersLeaveComments"));
            }

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnBlogCommentPage && !captchaValid)
            {
                ModelState.AddModelError("", _captchaSettings.GetWrongCaptchaMessage(_localizationService));
            }

            if (ModelState.IsValid)
            {
                await _blogViewModelService.InsertBlogComment(model, blogPost);
                //The text boxes should be cleared after a comment has been posted
                //That' why we reload the page
                TempData["Grand.blog.addcomment.result"] = _localizationService.GetResource("Blog.Comments.SuccessfullyAdded");
                return RedirectToRoute("BlogPost", new { SeName = blogPost.GetSeName(_workContext.WorkingLanguage.Id) });
            }

            //If we got this far, something failed, redisplay form
            await _blogViewModelService.PrepareBlogPostModel(model, blogPost, true);
            return View(model);
        }
        #endregion
    }
}
