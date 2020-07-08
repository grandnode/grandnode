using Grand.Core;
using Grand.Domain.Customers;
using Grand.Domain.Forums;
using Grand.Framework.Components;
using Grand.Services.Common;
using Grand.Services.Forums;
using Grand.Services.Localization;
using Grand.Web.Models.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class HeaderLinksViewComponent : BaseViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;

        private readonly CustomerSettings _customerSettings;
        private readonly ForumSettings _forumSettings;

        public HeaderLinksViewComponent(IWorkContext workContext,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            CustomerSettings customerSettings,
            ForumSettings forumSettings)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _customerSettings = customerSettings;
            _forumSettings = forumSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await PrepareHeaderLinks();
            return View(model);
        }
        private async Task<HeaderLinksModel> PrepareHeaderLinks()
        {
            var isRegister = _workContext.CurrentCustomer.IsRegistered();
            var model = new HeaderLinksModel {
                IsAuthenticated = isRegister,
                CustomerEmailUsername = isRegister ? (_customerSettings.UsernamesEnabled ? _workContext.CurrentCustomer.Username : _workContext.CurrentCustomer.Email) : "",
                AllowPrivateMessages = isRegister && _forumSettings.AllowPrivateMessages,
            };
            if (_forumSettings.AllowPrivateMessages)
            {
                var unreadMessageCount = await GetUnreadPrivateMessages();
                var unreadMessage = string.Empty;
                var alertMessage = string.Empty;
                if (unreadMessageCount > 0)
                {
                    unreadMessage = string.Format(_localizationService.GetResource("PrivateMessages.TotalUnread"), unreadMessageCount);

                    //notifications here
                    if (_forumSettings.ShowAlertForPM &&
                        !_workContext.CurrentCustomer.GetAttributeFromEntity<bool>(SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages, _storeContext.CurrentStore.Id))
                    {
                        await HttpContext.RequestServices.GetRequiredService<IGenericAttributeService>().SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages, true, _storeContext.CurrentStore.Id);
                        alertMessage = string.Format(_localizationService.GetResource("PrivateMessages.YouHaveUnreadPM"), unreadMessageCount);
                    }
                }
                model.UnreadPrivateMessages = unreadMessage;
                model.AlertMessage = alertMessage;
            }
            return model;
        }

        private async Task<int> GetUnreadPrivateMessages()
        {
            var result = 0;
            var customer = _workContext.CurrentCustomer;
            if (_forumSettings.AllowPrivateMessages && !customer.IsGuest())
            {
                var forumservice = HttpContext.RequestServices.GetRequiredService<IForumService>();
                var privateMessages = await forumservice.GetAllPrivateMessages(_storeContext.CurrentStore.Id,
                    "", customer.Id, false, null, false, string.Empty, 0, 1);

                if (privateMessages.Any())
                {
                    result = privateMessages.TotalCount;
                }
            }
            return result;

        }
    }
}