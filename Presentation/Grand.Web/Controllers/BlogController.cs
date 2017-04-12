using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Web.Mvc;
using Grand.Core;
using Grand.Core.Domain.Blogs;
using Grand.Core.Domain.Customers;
using Grand.Services.Blogs;
using Grand.Services.Localization;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Web.Framework;
using Grand.Web.Framework.Controllers;
using Grand.Web.Framework.Security;
using Grand.Web.Framework.Security.Captcha;
using Grand.Web.Models.Blogs;
using Grand.Services.Security;
using Grand.Web.Services;

namespace Grand.Web.Controllers
{
    [NopHttpsRequirement(SslRequirement.No)]
    public partial class BlogController : BasePublicController
    {
        #region Fields

        private readonly IBlogWebService _blogWebService;
        private readonly IBlogService _blogService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IPermissionService _permissionService;

        private readonly BlogSettings _blogSettings;
        private readonly CaptchaSettings _captchaSettings;

        #endregion

        #region Constructors

        public BlogController(IBlogWebService blogWebService,
            IBlogService blogService, 
            IWorkContext workContext,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            IWebHelper webHelper,
            IStoreMappingService storeMappingService,
            IPermissionService permissionService,
            BlogSettings blogSettings,
            CaptchaSettings captchaSettings
        )
        {
            this._blogWebService = blogWebService;
            this._blogService = blogService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._localizationService = localizationService;
            this._webHelper = webHelper;
            this._storeMappingService = storeMappingService;
            this._permissionService = permissionService;

            this._blogSettings = blogSettings;
            this._captchaSettings = captchaSettings;
        }

		#endregion

        #region Methods

        public virtual ActionResult List(BlogPagingFilteringModel command)
        {
            if (!_blogSettings.Enabled)
                return RedirectToRoute("HomePage");
            
            var model = _blogWebService.PrepareBlogPostListModel(command);
            return View("List", model);
        }
        public virtual ActionResult BlogByTag(BlogPagingFilteringModel command)
        {
            if (!_blogSettings.Enabled)
                return RedirectToRoute("HomePage");

            var model = _blogWebService.PrepareBlogPostListModel(command);
            return View("List", model);
        }
        public virtual ActionResult BlogByMonth(BlogPagingFilteringModel command)
        {
            if (!_blogSettings.Enabled)
                return RedirectToRoute("HomePage");

            var model = _blogWebService.PrepareBlogPostListModel(command);
            return View("List", model);
        }

        public virtual ActionResult ListRss(string languageId)
        {
            var feed = new SyndicationFeed(
                                    string.Format("{0}: Blog", _storeContext.CurrentStore.GetLocalized(x => x.Name)),
                                    "Blog",
                                    new Uri(_webHelper.GetStoreLocation(false)),
                                    string.Format("urn:store:{0}:blog", _storeContext.CurrentStore.Id),
                                    DateTime.UtcNow);

            if (!_blogSettings.Enabled)
                return new RssActionResult { Feed = feed };

            var items = new List<SyndicationItem>();
            var blogPosts = _blogService.GetAllBlogPosts(_storeContext.CurrentStore.Id);
            foreach (var blogPost in blogPosts)
            {
                string blogPostUrl = Url.RouteUrl("BlogPost", new { SeName = blogPost.GetSeName() }, _webHelper.IsCurrentConnectionSecured() ? "https" : "http");
                items.Add(new SyndicationItem(blogPost.Title, blogPost.Body, new Uri(blogPostUrl), String.Format("urn:store:{0}:blog:post:{1}", _storeContext.CurrentStore.Id, blogPost.Id), blogPost.CreatedOnUtc));
            }
            feed.Items = items;
            return new RssActionResult { Feed = feed };
        }

        public virtual ActionResult BlogPost(string blogPostId)
        {
            if (!_blogSettings.Enabled)
                return RedirectToRoute("HomePage");

            var blogPost = _blogService.GetBlogPostById(blogPostId);
            if (blogPost == null ||
                (blogPost.StartDateUtc.HasValue && blogPost.StartDateUtc.Value >= DateTime.UtcNow) ||
                (blogPost.EndDateUtc.HasValue && blogPost.EndDateUtc.Value <= DateTime.UtcNow))
                return RedirectToRoute("HomePage");

            //Store mapping
            if (!_storeMappingService.Authorize(blogPost))
                return InvokeHttp404();
            
            var model = new BlogPostModel();
            _blogWebService.PrepareBlogPostModel(model, blogPost, true);

            //display "edit" (manage) link
            if (_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) && _permissionService.Authorize(StandardPermissionProvider.ManageBlog))
                DisplayEditLink(Url.Action("Edit", "Blog", new { id = blogPost.Id, area = "Admin" }));


            return View(model);
        }

        [HttpPost, ActionName("BlogPost")]
        [PublicAntiForgery]
        [FormValueRequired("add-comment")]
        [CaptchaValidator]
        public virtual ActionResult BlogCommentAdd(string blogPostId, BlogPostModel model, bool captchaValid)
        {
            if (!_blogSettings.Enabled)
                return RedirectToRoute("HomePage");

            var blogPost = _blogService.GetBlogPostById(blogPostId);
            if (blogPost == null || !blogPost.AllowComments)
                return RedirectToRoute("HomePage");

            if (_workContext.CurrentCustomer.IsGuest() && !_blogSettings.AllowNotRegisteredUsersToLeaveComments)
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
                _blogWebService.InsertBlogComment(model, blogPost);
                //The text boxes should be cleared after a comment has been posted
                //That' why we reload the page
                TempData["Grand.blog.addcomment.result"] = _localizationService.GetResource("Blog.Comments.SuccessfullyAdded");
                return RedirectToRoute("BlogPost", new { SeName = blogPost.GetSeName() });
            }

            //If we got this far, something failed, redisplay form
            _blogWebService.PrepareBlogPostModel(model, blogPost, true);
            return View(model);
        }

        [ChildActionOnly]
        public virtual ActionResult BlogTags()
        {
            if (!_blogSettings.Enabled)
                return Content("");

            var model = _blogWebService.PrepareBlogPostTagListModel();

            return PartialView(model);
        }

        [ChildActionOnly]
        public virtual ActionResult BlogMonths()
        {
            if (!_blogSettings.Enabled)
                return Content("");

            var model = _blogWebService.PrepareBlogPostYearModel();
            return PartialView(model);
        }

        [ChildActionOnly]
        public virtual ActionResult RssHeaderLink()
        {
            if (!_blogSettings.Enabled || !_blogSettings.ShowHeaderRssUrl)
                return Content("");

            string link = string.Format("<link href=\"{0}\" rel=\"alternate\" type=\"application/rss+xml\" title=\"{1}: Blog\" />",
                Url.RouteUrl("BlogRSS", new { languageId = _workContext.WorkingLanguage.Id }, _webHelper.IsCurrentConnectionSecured() ? "https" : "http"), _storeContext.CurrentStore.GetLocalized(x => x.Name));

            return Content(link);
        }
        #endregion
    }
}
