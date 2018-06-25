using Grand.Core;
using Grand.Core.Domain;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.PushNotifications;
using Grand.Web.Areas.Admin.Models.PushNotifications;
using Grand.Web.Models.PushNotifications;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Components
{
    public class PushNotificationsRegistration : ViewComponent
    {
        private PushNotificationsSettings _pushNotificationsSettings;
        private IWorkContext _workContext;

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
