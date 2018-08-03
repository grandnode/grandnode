using Grand.Core;
using Grand.Core.Domain.PushNotifications;
using Grand.Framework.Mvc;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.PushNotifications;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Grand.Web.Controllers
{
    public class PushNotificationsController : BasePublicController
    {
        private readonly IWorkContext _workContext;
        private readonly IPushNotificationsService _pushNotificationsService;
        private readonly ILogger _logger;
        private readonly ILocalizationService _localizationService;

        public PushNotificationsController(IWorkContext workContext, IPushNotificationsService pushNotificationsService, ILogger logger,
            ILocalizationService localizationService)
        {
            this._workContext = workContext;
            this._pushNotificationsService = pushNotificationsService;
            this._logger = logger;
            this._localizationService = localizationService;
        }

        [HttpPost]
        public IActionResult ProcessRegistration(bool success, string value)
        {
            if (success)
            {
                var toUpdate = _pushNotificationsService.GetPushReceiverByCustomerId(_workContext.CurrentCustomer.Id);

                if (toUpdate == null)
                {
                    _pushNotificationsService.InsertPushReceiver(new PushRegistration
                    {
                        CustomerId = _workContext.CurrentCustomer.Id,
                        Token = value,
                        RegisteredOn = DateTime.UtcNow,
                        Allowed = true
                    });
                }
                else
                {
                    toUpdate.Token = value;
                    toUpdate.RegisteredOn = DateTime.UtcNow;
                    toUpdate.Allowed = true;
                    _pushNotificationsService.UpdatePushReceiver(toUpdate);
                }
            }
            else
            {
                if (value == "Permission denied")
                {
                    var toUpdate = _pushNotificationsService.GetPushReceiverByCustomerId(_workContext.CurrentCustomer.Id);

                    if (toUpdate == null)
                    {
                        _pushNotificationsService.InsertPushReceiver(new PushRegistration
                        {
                            CustomerId = _workContext.CurrentCustomer.Id,
                            Token = "[DENIED]",
                            RegisteredOn = DateTime.UtcNow,
                            Allowed = false
                        });
                    }
                    else
                    {
                        toUpdate.Token = "[DENIED]";
                        toUpdate.RegisteredOn = DateTime.UtcNow;
                        toUpdate.Allowed = false;
                        _pushNotificationsService.UpdatePushReceiver(toUpdate);
                    }
                }
            }

            return new NullJsonResult();
        }
    }
}
