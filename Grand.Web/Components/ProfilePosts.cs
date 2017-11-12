using Microsoft.AspNetCore.Mvc;
using System;
using Grand.Web.Models.Profile;
using Grand.Services.Forums;
using Grand.Services.Customers;
using Grand.Services.Logging;
using Grand.Services.Localization;
using Grand.Core;
using Grand.Services.Helpers;
using Grand.Core.Domain.Forums;
using Grand.Core.Domain.Customers;
using Grand.Services.Media;
using Grand.Core.Domain.Media;
using Grand.Services.Directory;
using System.Collections.Generic;
using Grand.Services.Seo;
using Grand.Web.Models.Common;

namespace Grand.Web.ViewComponents
{
    public class ProfilePostsViewComponent : ViewComponent
    {
        private readonly IForumService _forumService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ForumSettings _forumSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly IPictureService _pictureService;
        private readonly MediaSettings _mediaSettings;
        private readonly ICountryService _countryService;

        public ProfilePostsViewComponent(IForumService forumService,
            ICustomerService customerService, ICustomerActivityService customerActivityService,
            ILocalizationService localizationService, IWorkContext workContext,
            IStoreContext storeContext, IDateTimeHelper dateTimeHelper,
            ForumSettings forumSettings, CustomerSettings customerSettings,
            IPictureService pictureService, MediaSettings mediaSettings,
            ICountryService countryService)
        {
            this._forumService = forumService;
            this._customerService = customerService;
            this._customerActivityService = customerActivityService;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._dateTimeHelper = dateTimeHelper;
            this._forumSettings = forumSettings;
            this._customerSettings = customerSettings;
            this._pictureService = pictureService;
            this._mediaSettings = mediaSettings;
            this._countryService = countryService;
        }

        public IViewComponentResult Invoke(string customerProfileId, int pageNumber)
        {
            var customer = _customerService.GetCustomerById(customerProfileId);
            if (customer == null)
            {
                return Content("");
            }

            if (pageNumber > 0)
            {
                pageNumber -= 1;
            }

            var pageSize = _forumSettings.LatestCustomerPostsPageSize;

            var list = _forumService.GetAllPosts("", customer.Id, string.Empty, false, pageNumber, pageSize);

            var latestPosts = new List<PostsModel>();

            foreach (var forumPost in list)
            {
                var posted = string.Empty;
                if (_forumSettings.RelativeDateTimeFormattingEnabled)
                {
                    posted = forumPost.CreatedOnUtc.ToString("f");
                }
                else
                {
                    posted = _dateTimeHelper.ConvertToUserTime(forumPost.CreatedOnUtc, DateTimeKind.Utc).ToString("f");
                }
                var forumtopic = _forumService.GetTopicById(forumPost.TopicId);
                latestPosts.Add(new PostsModel
                {
                    ForumTopicId = forumPost.TopicId,
                    ForumTopicTitle = forumtopic.Subject,
                    ForumTopicSlug = forumtopic.GetSeName(),
                    ForumPostText = forumPost.FormatPostText(),
                    Posted = posted
                });
            }

            var pagerModel = new PagerModel
            {
                PageSize = list.PageSize,
                TotalRecords = list.TotalCount,
                PageIndex = list.PageIndex,
                ShowTotalSummary = false,
                RouteActionName = "CustomerProfilePaged",
                UseRouteLinks = true,
                RouteValues = new RouteValues { pageNumber = pageNumber, id = customerProfileId }
            };

            var model = new ProfilePostsModel
            {
                PagerModel = pagerModel,
                Posts = latestPosts,
            };

            return View(model);
        }
    }
}