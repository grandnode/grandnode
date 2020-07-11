using Grand.Core;
using Grand.Domain.Customers;
using Grand.Domain.Forums;
using Grand.Framework.Components;
using Grand.Services.Customers;
using Grand.Services.Forums;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Web.Models.Common;
using Grand.Web.Models.PrivateMessages;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class PrivateMessagesInboxViewComponent : BaseViewComponent
    {
        private readonly ForumSettings _forumSettings;
        private readonly IForumService _forumService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerService _customerService;
        private readonly CustomerSettings _customerSettings;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;

        public PrivateMessagesInboxViewComponent(ForumSettings forumSettings, IForumService forumService,
            IWorkContext workContext, IStoreContext storeContext, ICustomerService customerService,
            CustomerSettings customerSettings, IDateTimeHelper dateTimeHelper, ILocalizationService localizationService)
        {
            _forumSettings = forumSettings;
            _forumService = forumService;
            _workContext = workContext;
            _storeContext = storeContext;
            _customerService = customerService;
            _customerSettings = customerSettings;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
        }

        public async Task<IViewComponentResult> InvokeAsync(int pageNumber, string tab)
        {
            if (pageNumber > 0)
            {
                pageNumber -= 1;
            }

            var pageSize = _forumSettings.PrivateMessagesPageSize;

            var list = await _forumService.GetAllPrivateMessages(_storeContext.CurrentStore.Id,
                "", _workContext.CurrentCustomer.Id, null, null, false, string.Empty, pageNumber, pageSize);

            var inbox = new List<PrivateMessageModel>();

            foreach (var pm in list)
            {
                var fromCustomer = await _customerService.GetCustomerById(pm.FromCustomerId);
                var toCustomer = await _customerService.GetCustomerById(pm.ToCustomerId);
                inbox.Add(new PrivateMessageModel
                {
                    Id = pm.Id,
                    FromCustomerId = fromCustomer.Id,
                    CustomerFromName = fromCustomer.FormatUserName(_customerSettings.CustomerNameFormat),
                    AllowViewingFromProfile = _customerSettings.AllowViewingProfiles && fromCustomer != null && !fromCustomer.IsGuest(),
                    ToCustomerId = toCustomer.Id,
                    CustomerToName = toCustomer.FormatUserName(_customerSettings.CustomerNameFormat),
                    AllowViewingToProfile = _customerSettings.AllowViewingProfiles && toCustomer != null && !toCustomer.IsGuest(),
                    Subject = pm.Subject,
                    Message = pm.Text,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(pm.CreatedOnUtc, DateTimeKind.Utc),
                    IsRead = pm.IsRead,
                });
            }

            var pagerModel = new PagerModel(_localizationService)
            {
                PageSize = list.PageSize,
                TotalRecords = list.TotalCount,
                PageIndex = list.PageIndex,
                ShowTotalSummary = false,
                RouteActionName = "PrivateMessagesPaged",
                UseRouteLinks = true,
                RouteValues = new PrivateMessageRouteValues { pageNumber = pageNumber, tab = tab }
            };

            var model = new PrivateMessageListModel
            {
                Messages = inbox,
                PagerModel = pagerModel
            };

            return View(model);

        }
    }
}