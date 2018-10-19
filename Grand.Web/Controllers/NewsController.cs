using Grand.Core;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.News;
using Grand.Framework.Controllers;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Mvc.Rss;
using Grand.Framework.Security;
using Grand.Framework.Security.Captcha;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.News;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Web.Models.News;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Grand.Web.Controllers
{
    public partial class NewsController : BasePublicController
    {
        #region Fields

        private readonly INewsViewModelService _newsViewModelService;
        private readonly INewsService _newsService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IPermissionService _permissionService;

        private readonly NewsSettings _newsSettings;
        private readonly CaptchaSettings _captchaSettings;
        
        #endregion

		#region Constructors

        public NewsController(INewsViewModelService newsViewModelService, INewsService newsService, 
            IWorkContext workContext, IStoreContext storeContext, 
            ILocalizationService localizationService,
            IWebHelper webHelper, ICustomerActivityService customerActivityService,
            IStoreMappingService storeMappingService,
            IPermissionService permissionService,
            NewsSettings newsSettings,
            CaptchaSettings captchaSettings)
        {
            this._newsViewModelService = newsViewModelService;
            this._newsService = newsService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._localizationService = localizationService;
            this._webHelper = webHelper;
            this._customerActivityService = customerActivityService;
            this._storeMappingService = storeMappingService;
            this._permissionService = permissionService;
            this._newsSettings = newsSettings;
            this._captchaSettings = captchaSettings;
        }

        #endregion

        #region Utilities

        #endregion

        #region Methods

        public virtual IActionResult List(NewsPagingFilteringModel command)
        {
            if (!_newsSettings.Enabled)
                return RedirectToRoute("HomePage");

            var model = _newsViewModelService.PrepareNewsItemList(command);

            return View(model);
        }

        public virtual IActionResult ListRss(string languageId)
        {
            var feed = new RssFeed(
                string.Format("{0}: News", _storeContext.CurrentStore.GetLocalized(x => x.Name)),
                "News",
                new Uri(_webHelper.GetStoreLocation()),
                DateTime.UtcNow);

            if (!_newsSettings.Enabled)
                return new RssActionResult(feed, _webHelper.GetThisPageUrl(false));

            var items = new List<RssItem>();
            var newsItems = _newsService.GetAllNews(_storeContext.CurrentStore.Id);
            foreach (var n in newsItems)
            {
                string newsUrl = Url.RouteUrl("NewsItem", new { SeName = n.GetSeName() }, _webHelper.IsCurrentConnectionSecured() ? "https" : "http");
                items.Add(new RssItem(n.Title, n.Short, new Uri(newsUrl), String.Format("urn:store:{0}:news:blog:{1}", _storeContext.CurrentStore.Id, n.Id), n.CreatedOnUtc));
            }
            feed.Items = items;
            return new RssActionResult(feed, _webHelper.GetThisPageUrl(false));
        }

        public virtual IActionResult NewsItem(string newsItemId)
        {
            if (!_newsSettings.Enabled)
                return RedirectToRoute("HomePage");

            var newsItem = _newsService.GetNewsById(newsItemId);
            if (newsItem == null || 
                !newsItem.Published ||
                (newsItem.StartDateUtc.HasValue && newsItem.StartDateUtc.Value >= DateTime.UtcNow) ||
                (newsItem.EndDateUtc.HasValue && newsItem.EndDateUtc.Value <= DateTime.UtcNow) ||
                //Store mapping
                !_storeMappingService.Authorize(newsItem))
                return RedirectToRoute("HomePage");

            var model = new NewsItemModel();
            _newsViewModelService.PrepareNewsItemModel(model, newsItem, true);

            //display "edit" (manage) link
            if (_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) && _permissionService.Authorize(StandardPermissionProvider.ManageNews))
                DisplayEditLink(Url.Action("Edit", "News", new { id = newsItem.Id, area = "Admin" }));

            return View(model);
        }

        [HttpPost, ActionName("NewsItem")]
        [FormValueRequired("add-comment")]
        [PublicAntiForgery]
        [ValidateCaptcha]
        public virtual IActionResult NewsCommentAdd(string newsItemId, NewsItemModel model, bool captchaValid)
        {
            if (!_newsSettings.Enabled)
                return RedirectToRoute("HomePage");

            var newsItem = _newsService.GetNewsById(newsItemId);
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
                _newsViewModelService.InsertNewsComment(newsItem, model);
                //activity log
                _customerActivityService.InsertActivity("PublicStore.AddNewsComment", newsItem.Id, _localizationService.GetResource("ActivityLog.PublicStore.AddNewsComment"));

                //The text boxes should be cleared after a comment has been posted
                //That' why we reload the page
                TempData["Grand.news.addcomment.result"] = _localizationService.GetResource("News.Comments.SuccessfullyAdded");
                return RedirectToRoute("NewsItem", new {SeName = newsItem.GetSeName() });
            }

            //If we got this far, something failed, redisplay form
            _newsViewModelService.PrepareNewsItemModel(model, newsItem, true);
            return View(model);
        }
        #endregion
    }
}
