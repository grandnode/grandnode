using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;
using Grand.Core.Infrastructure;
using Grand.Core;
using Grand.Services.Localization;
using Grand.Core.Domain.Forums;
using Grand.Services.Common;
using Grand.Core.Domain.Customers;

namespace Grand.Web.ViewComponents
{
    public class HeaderLinksViewComponent : ViewComponent
    {
        private readonly ICommonWebService _commonWebService;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly ForumSettings _forumSettings;

        public HeaderLinksViewComponent(ICommonWebService commonWebService, IWorkContext workContext,
            ILocalizationService localizationService, ForumSettings forumSettings,
            IStoreContext storeContext)
        {
            this._commonWebService = commonWebService;
            this._workContext = workContext;
            this._localizationService = localizationService;
            this._forumSettings = forumSettings;
            this._storeContext = storeContext;
        }

        public IViewComponentResult Invoke()
        {
            var customer = _workContext.CurrentCustomer;
            var model = _commonWebService.PrepareHeaderLinks(customer);
            if (_forumSettings.AllowPrivateMessages)
            {
                var unreadMessageCount = _commonWebService.GetUnreadPrivateMessages();
                var unreadMessage = string.Empty;
                var alertMessage = string.Empty;
                if (unreadMessageCount > 0)
                {
                    unreadMessage = string.Format(_localizationService.GetResource("PrivateMessages.TotalUnread"), unreadMessageCount);

                    //notifications here
                    if (_forumSettings.ShowAlertForPM &&
                        !customer.GetAttribute<bool>(SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages, _storeContext.CurrentStore.Id))
                    {
                        EngineContext.Current.Resolve<IGenericAttributeService>().SaveAttribute(customer, SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages, true, _storeContext.CurrentStore.Id);
                        alertMessage = string.Format(_localizationService.GetResource("PrivateMessages.YouHaveUnreadPM"), unreadMessageCount);
                    }
                }
                model.UnreadPrivateMessages = unreadMessage;
                model.AlertMessage = alertMessage;
            }
            return View(model);
        }
    }
}