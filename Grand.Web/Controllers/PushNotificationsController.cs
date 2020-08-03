using Grand.Core;
using Grand.Domain.PushNotifications;
using Grand.Framework.Mvc;
using Grand.Services.PushNotifications;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Grand.Web.Controllers
{
    public class PushNotificationsController : BasePublicController
    {
        private readonly IWorkContext _workContext;
        private readonly IPushNotificationsService _pushNotificationsService;

        public PushNotificationsController(IWorkContext workContext, IPushNotificationsService pushNotificationsService)
        {
            _workContext = workContext;
            _pushNotificationsService = pushNotificationsService;
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProcessRegistration(bool success, string value)
        {
            if (success)
            {
                var toUpdate = await _pushNotificationsService.GetPushReceiverByCustomerId(_workContext.CurrentCustomer.Id);

                if (toUpdate == null)
                {
                    await _pushNotificationsService.InsertPushReceiver(new PushRegistration {
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
                    await _pushNotificationsService.UpdatePushReceiver(toUpdate);
                }
            }
            else
            {
                if (value == "Permission denied")
                {
                    var toUpdate = await _pushNotificationsService.GetPushReceiverByCustomerId(_workContext.CurrentCustomer.Id);

                    if (toUpdate == null)
                    {
                        await _pushNotificationsService.InsertPushReceiver(new PushRegistration {
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
                        await _pushNotificationsService.UpdatePushReceiver(toUpdate);
                    }
                }
            }

            return new NullJsonResult();
        }
    }
}
