using Grand.Domain.Forums;
using Grand.Framework.Components;
using Grand.Services.Customers;
using Grand.Services.Forums;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Seo;
using Grand.Web.Models.Common;
using Grand.Web.Models.Profile;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class ProfilePostsViewComponent : BaseViewComponent
    {
        private readonly IForumService _forumService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly ForumSettings _forumSettings;

        public ProfilePostsViewComponent(IForumService forumService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            ForumSettings forumSettings)
        {
            _forumService = forumService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _forumSettings = forumSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync(string customerProfileId, int pageNumber)
        {
            var customer = await _customerService.GetCustomerById(customerProfileId);
            if (customer == null)
            {
                return Content("");
            }

            if (pageNumber > 0)
            {
                pageNumber -= 1;
            }

            var pageSize = _forumSettings.LatestCustomerPostsPageSize;

            var list = await _forumService.GetAllPosts("", customer.Id, string.Empty, false, pageNumber, pageSize);

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
                var forumtopic = await _forumService.GetTopicById(forumPost.TopicId);
                if (forumtopic != null)
                    latestPosts.Add(new PostsModel {
                        ForumTopicId = forumPost.TopicId,
                        ForumTopicTitle = forumtopic.Subject,
                        ForumTopicSlug = forumtopic.GetSeName(),
                        ForumPostText = forumPost.FormatPostText(),
                        Posted = posted
                    });
            }

            var pagerModel = new PagerModel(_localizationService) {
                PageSize = list.PageSize,
                TotalRecords = list.TotalCount,
                PageIndex = list.PageIndex,
                ShowTotalSummary = false,
                RouteActionName = "CustomerProfilePaged",
                UseRouteLinks = true,
                RouteValues = new RouteValues { pageNumber = pageNumber, id = customerProfileId }
            };

            var model = new ProfilePostsModel {
                PagerModel = pagerModel,
                Posts = latestPosts,
            };

            return View(model);
        }
    }
}