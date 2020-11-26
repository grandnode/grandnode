﻿using Grand.Core;
using Grand.Domain.Blogs;
using Grand.Domain.Customers;
using Grand.Framework.Controllers;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Mvc.Rss;
using Grand.Framework.Security.Captcha;
using Grand.Services.Blogs;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Web.Commands.Models.Blogs;
using Grand.Web.Events;
using Grand.Web.Features.Models.Blogs;
using Grand.Web.Models.Blogs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Controllers
{
    public partial class BlogController : BasePublicController
    {
        #region Fields

        private readonly IMediator _mediator;
        private readonly IBlogService _blogService;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly BlogSettings _blogSettings;
        private readonly CaptchaSettings _captchaSettings;

        #endregion

        #region Constructors

        public BlogController(
            IMediator mediator,
            IBlogService blogService,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            IWebHelper webHelper,
            IWorkContext workContext,
            BlogSettings blogSettings,
            CaptchaSettings captchaSettings)
        {
            _mediator = mediator;
            _blogService = blogService;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _webHelper = webHelper;
            _blogSettings = blogSettings;
            _captchaSettings = captchaSettings;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> List(BlogPagingFilteringModel command)
        {
            if (!_blogSettings.Enabled)
                return RedirectToRoute("HomePage");

            var model = await _mediator.Send(new GetBlogPostList() { Command = command });
            return View("List", model);
        }
        public virtual async Task<IActionResult> BlogByTag(BlogPagingFilteringModel command)
        {
            if (!_blogSettings.Enabled)
                return RedirectToRoute("HomePage");

            var model = await _mediator.Send(new GetBlogPostList() { Command = command });
            return View("List", model);
        }
        public virtual async Task<IActionResult> BlogByMonth(BlogPagingFilteringModel command)
        {
            if (!_blogSettings.Enabled)
                return RedirectToRoute("HomePage");

            var model = await _mediator.Send(new GetBlogPostList() { Command = command });
            return View("List", model);
        }
        public virtual async Task<IActionResult> BlogByCategory(BlogPagingFilteringModel command)
        {
            if (!_blogSettings.Enabled)
                return RedirectToRoute("HomePage");

            var model = await _mediator.Send(new GetBlogPostList() { Command = command });
            return View("List", model);
        }
        public virtual async Task<IActionResult> BlogByKeyword(BlogPagingFilteringModel command)
        {
            if (!_blogSettings.Enabled)
                return RedirectToRoute("HomePage");

            var model = await _mediator.Send(new GetBlogPostList() { Command = command });
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

            var model = await _mediator.Send(new GetBlogPost() { BlogPost = blogPost });

            //display "edit" (manage) link
            if (await permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) && await permissionService.Authorize(StandardPermissionProvider.ManageBlog))
                DisplayEditLink(Url.Action("Edit", "Blog", new { id = blogPost.Id, area = "Admin" }));

            return View(model);
        }

        [HttpPost, ActionName("BlogPost")]
        [AutoValidateAntiforgeryToken]
        [FormValueRequired("add-comment")]
        [ValidateCaptcha]
        public virtual async Task<IActionResult> BlogCommentAdd(string blogPostId, BlogPostModel model, bool captchaValid)
        {
            if (!_blogSettings.Enabled)
                return RedirectToRoute("HomePage");

            var blogPost = await _blogService.GetBlogPostById(blogPostId);
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
                await _mediator.Send(new InsertBlogCommentCommand() { Model = model, BlogPost = blogPost });

                //notification
                await _mediator.Publish(new BlogCommentEvent(blogPost, model.AddNewComment));

                //The text boxes should be cleared after a comment has been posted
                TempData["Grand.blog.addcomment.result"] = _localizationService.GetResource("Blog.Comments.SuccessfullyAdded");
                return RedirectToRoute("BlogPost", new { SeName = blogPost.GetSeName(_workContext.WorkingLanguage.Id) });
            }

            //If we got this far, something failed, redisplay form
            model = await _mediator.Send(new GetBlogPost() { BlogPost = blogPost });
            return View(model);
        }
        #endregion
    }
}
