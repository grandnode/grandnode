using Grand.Core;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Forums;
using Grand.Core.Infrastructure;
using Grand.Framework.Components;
using Grand.Services.Common;
using Grand.Services.Localization;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.ViewComponents
{
    public class HeaderLinksViewComponent : BaseViewComponent
    {
        private readonly ICommonViewModelService _commonViewModelService;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly ForumSettings _forumSettings;

        public HeaderLinksViewComponent(ICommonViewModelService commonViewModelService, IWorkContext workContext,
            ILocalizationService localizationService, ForumSettings forumSettings,
            IStoreContext storeContext)
        {
            this._commonViewModelService = commonViewModelService;
            this._workContext = workContext;
            this._localizationService = localizationService;
            this._forumSettings = forumSettings;
            this._storeContext = storeContext;
        }

        public IViewComponentResult Invoke()
        {
            var customer = _workContext.CurrentCustomer;
            var model = _commonViewModelService.PrepareHeaderLinks(customer);
            if (_forumSettings.AllowPrivateMessages)
            {
                var unreadMessageCount = _commonViewModelService.GetUnreadPrivateMessages();
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