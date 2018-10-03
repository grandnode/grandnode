using Grand.Core;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.PushNotifications;
using Grand.Framework.Components;
using Grand.Web.Models.PushNotifications;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.Components
{
    public class PushNotificationsRegistration : BaseViewComponent
    {
        private readonly PushNotificationsSettings _pushNotificationsSettings;
        private readonly IWorkContext _workContext;

        public PushNotificationsRegistration(PushNotificationsSettings pushNotificationsSettings, IWorkContext workContext)
        {
            _pushNotificationsSettings = pushNotificationsSettings;
            _workContext = workContext;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var model = new PublicInfoModel();
            model.PublicApiKey = _pushNotificationsSettings.PublicApiKey;
            model.SenderId = _pushNotificationsSettings.SenderId;
            model.AuthDomain = _pushNotificationsSettings.AuthDomain;
            model.ProjectId = _pushNotificationsSettings.ProjectId;
            model.StorageBucket = _pushNotificationsSettings.StorageBucket;
            model.DatabaseUrl = _pushNotificationsSettings.DatabaseUrl;
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
