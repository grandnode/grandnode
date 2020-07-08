using Grand.Core;
using Grand.Domain.Customers;
using Grand.Domain.News;
using Grand.Framework.Controllers;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Mvc.Rss;
using Grand.Framework.Security.Captcha;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.News;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Web.Commands.Models.News;
using Grand.Web.Events;
using Grand.Web.Features.Models.News;
using Grand.Web.Models.News;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Controllers
{
    public partial class NewsController : BasePublicController
    {
        #region Fields

        private readonly INewsService _newsService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IPermissionService _permissionService;
        private readonly IMediator _mediator;
        private readonly NewsSettings _newsSettings;
        private readonly CaptchaSettings _captchaSettings;

        #endregion

        #region Constructors

        public NewsController(INewsService newsService,
            IWorkContext workContext,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            IWebHelper webHelper,
            ICustomerActivityService customerActivityService,
            IStoreMappingService storeMappingService,
            IPermissionService permissionService,
            IMediator mediator,
            NewsSettings newsSettings,
            CaptchaSettings captchaSettings)
        {
            _newsService = newsService;
            _workContext = workContext;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _webHelper = webHelper;
            _customerActivityService = customerActivityService;
            _storeMappingService = storeMappingService;
            _permissionService = permissionService;
            _mediator = mediator;
            _newsSettings = newsSettings;
            _captchaSettings = captchaSettings;
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> List(NewsPagingFilteringModel command)
        {
            if (!_newsSettings.Enabled)
                return RedirectToRoute("HomePage");

            var model = await _mediator.Send(new GetNewsItemList() { Command = command });
            return View(model);
        }

        public virtual async Task<IActionResult> ListRss(string languageId)
        {
            var feed = new RssFeed(
                string.Format("{0}: News", _storeContext.CurrentStore.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id)),
                "News",
                new Uri(_webHelper.GetStoreLocation()),
                DateTime.UtcNow);

            if (!_newsSettings.Enabled)
                return new RssActionResult(feed, _webHelper.GetThisPageUrl(false));

            var items = new List<RssItem>();
            var newsItems = await _newsService.GetAllNews(_storeContext.CurrentStore.Id);
            foreach (var n in newsItems)
            {
                string newsUrl = Url.RouteUrl("NewsItem", new { SeName = n.GetSeName(_workContext.WorkingLanguage.Id) }, _webHelper.IsCurrentConnectionSecured() ? "https" : "http");
                items.Add(new RssItem(n.Title, n.Short, new Uri(newsUrl), String.Format("urn:store:{0}:news:blog:{1}", _storeContext.CurrentStore.Id, n.Id), n.CreatedOnUtc));
            }
            feed.Items = items;
            return new RssActionResult(feed, _webHelper.GetThisPageUrl(false));
        }

        public virtual async Task<IActionResult> NewsItem(string newsItemId)
        {
            if (!_newsSettings.Enabled)
                return RedirectToRoute("HomePage");

            var newsItem = await _newsService.GetNewsById(newsItemId);
            if (newsItem == null ||
                !newsItem.Published ||
                (newsItem.StartDateUtc.HasValue && newsItem.StartDateUtc.Value >= DateTime.UtcNow) ||
                (newsItem.EndDateUtc.HasValue && newsItem.EndDateUtc.Value <= DateTime.UtcNow) ||
                //Store mapping
                !_storeMappingService.Authorize(newsItem))
                return RedirectToRoute("HomePage");

            var model = await _mediator.Send(new GetNewsItem() { NewsItem = newsItem });

            //display "edit" (manage) link
            if (await _permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) && await _permissionService.Authorize(StandardPermissionProvider.ManageNews))
                DisplayEditLink(Url.Action("Edit", "News", new { id = newsItem.Id, area = "Admin" }));

            return View(model);
        }

        [HttpPost, ActionName("NewsItem")]
        [FormValueRequired("add-comment")]
        [AutoValidateAntiforgeryToken]
        [ValidateCaptcha]
        public virtual async Task<IActionResult> NewsCommentAdd(string newsItemId, NewsItemModel model, bool captchaValid)
        {
            if (!_newsSettings.Enabled)
                return RedirectToRoute("HomePage");

            var newsItem = await _newsService.GetNewsById(newsItemId);
            if (newsItem == null || !newsItem.Published || !newsItem.AllowComments)
                return RedirectToRoute("HomePage");

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnNewsCommentPage && !captchaValid)
            {
                ModelState.AddModelError("", _captchaSettings.GetWrongCaptchaMessage(_localizationService));
            }

            if (_workContext.CurrentCustomer.IsGuest() && !_newsSettings.AllowNotRegisteredUsersToLeaveComments)
            {
                ModelState.AddModelError("", _localizationService.GetResource("News.Comments.OnlyRegisteredUsersLeaveComments"));
            }

            if (ModelState.IsValid)
            {
                await _mediator.Send(new InsertNewsCommentCommand() { NewsItem = newsItem, Model = model });

                //notification
                await _mediator.Publish(new NewsCommentEvent(newsItem, model.AddNewComment));

                //activity log
                await _customerActivityService.InsertActivity("PublicStore.AddNewsComment", newsItem.Id, _localizationService.GetResource("ActivityLog.PublicStore.AddNewsComment"));

                //The text boxes should be cleared after a comment has been posted
                TempData["Grand.news.addcomment.result"] = _localizationService.GetResource("News.Comments.SuccessfullyAdded");
                return RedirectToRoute("NewsItem", new { SeName = newsItem.GetSeName(_workContext.WorkingLanguage.Id) });
            }

            //If we got this far, something failed, redisplay form
            model = await _mediator.Send(new GetNewsItem() { NewsItem = newsItem });
            return View(model);
        }
        #endregion
    }
}
