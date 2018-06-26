using Grand.Core;
using Grand.Core.Domain.PushNotifications;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Services.Configuration;
using Grand.Services.Customers;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.PushNotifications;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Web.Areas.Admin.Models.PushNotifications;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Controllers
{
    public class PushNotificationsController : BaseAdminController
    {
        private readonly PushNotificationsSettings _pushNotificationsSettings;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;
        private readonly IPushNotificationsService _pushNotificationsService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IPictureService _pictureService;
        private readonly IDateTimeHelper _dateTimeHelper;

        public PushNotificationsController(PushNotificationsSettings pushNotificationsSettings,
            ILocalizationService localizationService, ISettingService settingService, IStoreService storeService,
            IPushNotificationsService pushNotificationsService, IPermissionService permissionService, IWorkContext workContext,
            ICustomerService customerService, IPictureService pictureService, IDateTimeHelper dateTimeHelper)
        {
            this._pushNotificationsSettings = pushNotificationsSettings;
            this._localizationService = localizationService;
            this._settingService = settingService;
            this._storeService = storeService;
            this._pushNotificationsService = pushNotificationsService;
            this._permissionService = permissionService;
            this._workContext = workContext;
            this._customerService = customerService;
            this._pictureService = pictureService;
            this._dateTimeHelper = dateTimeHelper;
        }

        public IActionResult Send()
        {
            var model = new PushModel();
            model.MessageText = _localizationService.GetResource("PushNotifications.MessageTextPlaceholder");
            model.Title = _localizationService.GetResource("PushNotifications.MessageTitlePlaceholder");
            model.PictureId = _pushNotificationsSettings.PicutreId;
            model.ClickUrl = _pushNotificationsSettings.ClickUrl;

            return View(model);
        }

        [HttpPost]
        public IActionResult Send(PushModel model)
        {
            if (!string.IsNullOrEmpty(_pushNotificationsSettings.PrivateApiKey) && !string.IsNullOrEmpty(model.MessageText))
            {
                _pushNotificationsSettings.PicutreId = model.PictureId;
                _pushNotificationsSettings.ClickUrl = model.ClickUrl;
                _settingService.SaveSetting(_pushNotificationsSettings);

                Tuple<bool, string> result = _pushNotificationsService.SendPushNotification(model.Title, model.MessageText,
                    _pictureService.GetPictureUrl(model.PictureId), model.ClickUrl);

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

        public IActionResult Messages()
        {
            var model = new MessagesModel();
            model.Allowed = _pushNotificationsService.GetAllowedReceivers();
            model.Denied = _pushNotificationsService.GetDeniedReceivers();

            return View(model);
        }

        public IActionResult Receivers()
        {
            var model = new ReceiversModel();
            model.Allowed = _pushNotificationsService.GetAllowedReceivers();
            model.Denied = _pushNotificationsService.GetDeniedReceivers();

            return View(model);
        }
               
        [HttpPost]
        public IActionResult PushMessagesList(DataSourceRequest command)
        {
            var messages = _pushNotificationsService.GetPushMessages(command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult();
            gridModel.Data = messages.Select(x => new PushMessageGridModel
            {
                Id = x.Id,
                Text = x.Text,
                Title = x.Title,
                SentOn = _dateTimeHelper.ConvertToUserTime(x.SentOn, DateTimeKind.Utc),
                NumberOfReceivers = x.NumberOfReceivers
            });
            gridModel.Total = messages.TotalCount;

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult PushReceiversList(DataSourceRequest command)
        {
            var receivers = _pushNotificationsService.GetPushReceivers(command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult();
            var list = new List<PushRegistrationGridModel>();
            foreach (var receiver in receivers)
            {
                var gridReceiver = new PushRegistrationGridModel();

                var customer = _customerService.GetCustomerById(receiver.CustomerId);
                if (customer == null)
                {
                    _pushNotificationsService.DeletePushReceiver(receiver);
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

        [HttpPost]
        public IActionResult DeleteReceiver(string id)
        {
            var receiver = _pushNotificationsService.GetPushReceiver(id);
            _pushNotificationsService.DeletePushReceiver(receiver);

            return new NullJsonResult();
        }
    }
}
