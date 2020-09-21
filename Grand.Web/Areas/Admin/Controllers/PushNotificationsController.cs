using Grand.Core;
using Grand.Domain.PushNotifications;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Security.Authorization;
using Grand.Services.Configuration;
using Grand.Services.Customers;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.PushNotifications;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Models.PushNotifications;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.PushNotifications)]
    public class PushNotificationsController : BaseAdminController
    {
        private readonly PushNotificationsSettings _pushNotificationsSettings;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IPushNotificationsService _pushNotificationsService;
        private readonly ICustomerService _customerService;
        private readonly IPictureService _pictureService;
        private readonly IDateTimeHelper _dateTimeHelper;

        public PushNotificationsController(
            PushNotificationsSettings pushNotificationsSettings,
            ILocalizationService localizationService, 
            ISettingService settingService, 
            IPushNotificationsService pushNotificationsService, 
            ICustomerService customerService, 
            IPictureService pictureService, 
            IDateTimeHelper dateTimeHelper)
        {
            _pushNotificationsSettings = pushNotificationsSettings;
            _localizationService = localizationService;
            _settingService = settingService;
            _pushNotificationsService = pushNotificationsService;
            _customerService = customerService;
            _pictureService = pictureService;
            _dateTimeHelper = dateTimeHelper;
        }

        public IActionResult Send()
        {
            var model = new PushModel
            {
                MessageText = _localizationService.GetResource("Admin.PushNotifications.MessageTextPlaceholder"),
                Title = _localizationService.GetResource("Admin.PushNotifications.MessageTitlePlaceholder"),
                PictureId = _pushNotificationsSettings.PictureId,
                ClickUrl = _pushNotificationsSettings.ClickUrl
            };

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Create)]
        [HttpPost]
        public async Task<IActionResult> Send(PushModel model)
        {
            if (!string.IsNullOrEmpty(_pushNotificationsSettings.PrivateApiKey) && !string.IsNullOrEmpty(model.MessageText))
            {
                _pushNotificationsSettings.PictureId = model.PictureId;
                _pushNotificationsSettings.ClickUrl = model.ClickUrl;
                await _settingService.SaveSetting(_pushNotificationsSettings);
                var pictureUrl = await _pictureService.GetPictureUrl(model.PictureId);
                var result = (await _pushNotificationsService.SendPushNotification(model.Title, model.MessageText, pictureUrl, model.ClickUrl));
                if (result.Item1)
                {
                    SuccessNotification(result.Item2);
                }
                else
                {
                    ErrorNotification(result.Item2);
                }
            }
            else
            {
                ErrorNotification(_localizationService.GetResource("PushNotifications.Error.PushApiMessage"));
            }

            return RedirectToAction("Send");
        }

        [PermissionAuthorizeAction(PermissionActionName.List)]
        public async Task<IActionResult> Messages()
        {
            var model = new MessagesModel
            {
                Allowed = await _pushNotificationsService.GetAllowedReceivers(),
                Denied = await _pushNotificationsService.GetDeniedReceivers()
            };

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.List)]
        public async Task<IActionResult> Receivers()
        {
            var model = new ReceiversModel
            {
                Allowed = await _pushNotificationsService.GetAllowedReceivers(),
                Denied = await _pushNotificationsService.GetDeniedReceivers()
            };

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> PushMessagesList(DataSourceRequest command)
        {
            var messages = await _pushNotificationsService.GetPushMessages(command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = messages.Select(x => new PushMessageGridModel
                {
                    Id = x.Id,
                    Text = x.Text,
                    Title = x.Title,
                    SentOn = _dateTimeHelper.ConvertToUserTime(x.SentOn, DateTimeKind.Utc),
                    NumberOfReceivers = x.NumberOfReceivers
                }),
                Total = messages.TotalCount
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> PushReceiversList(DataSourceRequest command)
        {
            var receivers = await _pushNotificationsService.GetPushReceivers(command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult();
            var list = new List<PushRegistrationGridModel>();
            foreach (var receiver in receivers)
            {
                var gridReceiver = new PushRegistrationGridModel();

                var customer = await _customerService.GetCustomerById(receiver.CustomerId);
                if (customer == null)
                {
                    await _pushNotificationsService.DeletePushReceiver(receiver);
                    continue;
                }

                if (!string.IsNullOrEmpty(customer.Email))
                {
                    gridReceiver.CustomerEmail = customer.Email;
                }
                else
                {
                    gridReceiver.CustomerEmail = _localizationService.GetResource("Admin.Customers.Guest");
                }

                gridReceiver.CustomerId = receiver.CustomerId;
                gridReceiver.Id = receiver.Id;
                gridReceiver.RegisteredOn = _dateTimeHelper.ConvertToUserTime(receiver.RegisteredOn, DateTimeKind.Utc);
                gridReceiver.Token = receiver.Token;
                gridReceiver.Allowed = receiver.Allowed;

                list.Add(gridReceiver);
            }

            gridModel.Data = list;
            gridModel.Total = receivers.TotalCount;

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> DeleteReceiver(string id)
        {
            var receiver = await _pushNotificationsService.GetPushReceiver(id);
            await _pushNotificationsService.DeletePushReceiver(receiver);
            return new NullJsonResult();
        }
    }
}
