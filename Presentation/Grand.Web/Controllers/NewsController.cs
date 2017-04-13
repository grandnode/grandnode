using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Web.Mvc;
using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Media;
using Grand.Core.Domain.News;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Media;
using Grand.Services.Messages;
using Grand.Services.News;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Web.Framework;
using Grand.Web.Framework.Controllers;
using Grand.Web.Framework.Security;
using Grand.Web.Framework.Security.Captcha;
using Grand.Web.Models.News;
using Grand.Services.Security;
using Grand.Web.Services;

namespace Grand.Web.Controllers
{
    [NopHttpsRequirement(SslRequirement.No)]
    public partial class NewsController : BasePublicController
    {
        #region Fields

        private readonly INewsWebService _newsWebService;
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

        public NewsController(INewsWebService newsWebService, INewsService newsService, 
            IWorkContext workContext, IStoreContext storeContext, 
            ILocalizationService localizationService,
            IWebHelper webHelper, ICustomerActivityService customerActivityService,
            IStoreMappingService storeMappingService,
            IPermissionService permissionService,
            NewsSettings newsSettings,
            CaptchaSettings captchaSettings)
        {
            this._newsWebService = newsWebService;
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

        public virtual ActionResult HomePageNews()
        {
            if (!_newsSettings.Enabled || !_newsSettings.ShowNewsOnMainPage)
                return Content("");

            var model = _newsWebService.PrepareHomePageNewsItems();
            return PartialView(model);
        }

        public virtual ActionResult List(NewsPagingFilteringModel command)
        {
            if (!_newsSettings.Enabled)
                return RedirectToRoute("HomePage");

            var model = _newsWebService.PrepareNewsItemList(command);

            return View(model);
        }

        public virtual ActionResult ListRss(string languageId)
        {
            var feed = new SyndicationFeed(
                                    string.Format("{0}: News", _storeContext.CurrentStore.GetLocalized(x => x.Name)),
                                    "News",
                                    new Uri(_webHelper.GetStoreLocation(false)),
                                    string.Format("urn:store:{0}:news", _storeContext.CurrentStore.Id),
                                    DateTime.UtcNow);

            if (!_newsSettings.Enabled)
                return new RssActionResult { Feed = feed };

            var items = new List<SyndicationItem>();
            var newsItems = _newsService.GetAllNews(_storeContext.CurrentStore.Id);
            foreach (var n in newsItems)
            {
                string newsUrl = Url.RouteUrl("NewsItem", new { SeName = n.GetSeName() }, _webHelper.IsCurrentConnectionSecured() ? "https" : "http");
                items.Add(new SyndicationItem(n.Title, n.Short, new Uri(newsUrl), String.Format("urn:store:{0}:news:blog:{1}", _storeContext.CurrentStore.Id, n.Id), n.CreatedOnUtc));
            }
            feed.Items = items;
            return new RssActionResult { Feed = feed };
        }

        public virtual ActionResult NewsItem(string newsItemId)
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
            _newsWebService.PrepareNewsItemModel(model, newsItem, true);

            //display "edit" (manage) link
            if (_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) && _permissionService.Authorize(StandardPermissionProvider.ManageNews))
                DisplayEditLink(Url.Action("Edit", "News", new { id = newsItem.Id, area = "Admin" }));

            return View(model);
        }

        [HttpPost, ActionName("NewsItem")]
        [FormValueRequired("add-comment")]
        [PublicAntiForgery]
        [CaptchaValidator]
        public virtual ActionResult NewsCommentAdd(string newsItemId, NewsItemModel model, bool captchaValid)
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
                _newsWebService.InsertNewsComment(newsItem, model);
                //activity log
                _customerActivityService.InsertActivity("PublicStore.AddNewsComment", newsItem.Id, _localizationService.GetResource("ActivityLog.PublicStore.AddNewsComment"));

                //The text boxes should be cleared after a comment has been posted
                //That' why we reload the page
                TempData["Grand.news.addcomment.result"] = _localizationService.GetResource("News.Comments.SuccessfullyAdded");
                return RedirectToRoute("NewsItem", new {SeName = newsItem.GetSeName() });
            }

            //If we got this far, something failed, redisplay form
            _newsWebService.PrepareNewsItemModel(model, newsItem, true);
            return View(model);
        }

        [ChildActionOnly]
        public virtual ActionResult RssHeaderLink()
        {
            if (!_newsSettings.Enabled || !_newsSettings.ShowHeaderRssUrl)
                return Content("");

            string link = string.Format("<link href=\"{0}\" rel=\"alternate\" type=\"application/rss+xml\" title=\"{1}: News\" />",
                Url.RouteUrl("NewsRSS", new { languageId = _workContext.WorkingLanguage.Id }, _webHelper.IsCurrentConnectionSecured() ? "https" : "http"), _storeContext.CurrentStore.GetLocalized(x => x.Name));

            return Content(link);
        }

        #endregion
    }
}
