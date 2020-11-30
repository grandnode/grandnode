using Grand.Core;
using Grand.Domain.Customers;
using Grand.Domain.PushNotifications;
using Grand.Framework.Components;
using Grand.Web.Models.PushNotifications;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.Components
{
    public class PushNotificationsRegistration : BaseViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly PushNotificationsSettings _pushNotificationsSettings;

        public PushNotificationsRegistration(IWorkContext workContext, PushNotificationsSettings pushNotificationsSettings)
        {
            _workContext = workContext;
            _pushNotificationsSettings = pushNotificationsSettings;
        }

        public IViewComponentResult Invoke()
        {
            if(!_pushNotificationsSettings.Enabled)
                return Content("");

            var model = new PublicInfoModel {
                PublicApiKey = _pushNotificationsSettings.PublicApiKey,
                SenderId = _pushNotificationsSettings.SenderId,
                AuthDomain = _pushNotificationsSettings.AuthDomain,
                ProjectId = _pushNotificationsSettings.ProjectId,
                StorageBucket = _pushNotificationsSettings.StorageBucket,
                DatabaseUrl = _pushNotificationsSettings.DatabaseUrl,
                AppId = _pushNotificationsSettings.AppId
            };
            if (_pushNotificationsSettings.Enabled)
            {
                if (!_pushNotificationsSettings.AllowGuestNotifications && _workContext.CurrentCustomer.IsGuest())
                    return Content("");

                return View(model);
            }

            return Content("");
        }
    }
}
