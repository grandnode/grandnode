using Grand.Domain.Messages;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Services.Security;
using Grand.Services.Stores;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.MessageTemplates)]
    public partial class MessageTemplateController : BaseAdminController
    {
        #region Fields

        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IStoreService _storeService;
        private readonly EmailAccountSettings _emailAccountSettings;

        #endregion Fields

        #region Constructors

        public MessageTemplateController(IMessageTemplateService messageTemplateService,
            IEmailAccountService emailAccountService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IMessageTokenProvider messageTokenProvider,
            IStoreService storeService,
            EmailAccountSettings emailAccountSettings)
        {
            _messageTemplateService = messageTemplateService;
            _emailAccountService = emailAccountService;
            _languageService = languageService;
            _localizationService = localizationService;
            _messageTokenProvider = messageTokenProvider;
            _storeService = storeService;
            _emailAccountSettings = emailAccountSettings;
        }

        #endregion

        #region Methods

        public IActionResult Index() => RedirectToAction("List");

        public async Task<IActionResult> List()
        {
            var model = new MessageTemplateListModel();
            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "" });
            foreach (var s in await _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Shortcut, Value = s.Id.ToString() });

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.List)]
        [HttpPost]
        public async Task<IActionResult> List(DataSourceRequest command, MessageTemplateListModel model)
        {
            var messageTemplates = await _messageTemplateService.GetAllMessageTemplates(model.SearchStoreId);

            if (!string.IsNullOrEmpty(model.Name))
            {
                messageTemplates = messageTemplates.Where
                    (x => x.Name.ToLowerInvariant().Contains(model.Name.ToLowerInvariant()) ||
                    x.Subject.ToLowerInvariant().Contains(model.Name.ToLowerInvariant())).ToList();
            }
            var items = new List<MessageTemplateModel>();
            foreach (var x in messageTemplates)
            {
                var templateModel = x.ToModel();
                await templateModel.PrepareStoresMappingModel(x, _storeService, false);
                var stores = (await _storeService
                        .GetAllStores())
                        .Where(s => !x.LimitedToStores || templateModel.SelectedStoreIds.Contains(s.Id))
                        .ToList();
                for (int i = 0; i < stores.Count; i++)
                {
                    templateModel.ListOfStores += stores[i].Shortcut;
                    if (i != stores.Count - 1)
                        templateModel.ListOfStores += ", ";
                }
                items.Add(templateModel);
            }
            var gridModel = new DataSourceResult {
                Data = items,
                Total = messageTemplates.Count
            };

            return Json(gridModel);
        }

        [PermissionAuthorizeAction(PermissionActionName.Create)]
        public async Task<IActionResult> Create()
        {
            var model = new MessageTemplateModel();

            //Stores
            await model.PrepareStoresMappingModel(null, _storeService, false);
            model.AllowedTokens = _messageTokenProvider.GetListOfAllowedTokens();
            //available email accounts
            foreach (var ea in await _emailAccountService.GetAllEmailAccounts())
                model.AvailableEmailAccounts.Add(ea.ToModel());

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Create)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(MessageTemplateModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var messageTemplate = model.ToEntity();
                //attached file
                if (!model.HasAttachedDownload)
                    messageTemplate.AttachedDownloadId = "";
                if (model.SendImmediately)
                    messageTemplate.DelayBeforeSend = null;

                await _messageTemplateService.InsertMessageTemplate(messageTemplate);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.MessageTemplates.AddNew"));

                if (continueEditing)
                {
                    //selected tab
                    await SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new { id = messageTemplate.Id });
                }
                return RedirectToAction("List");
            }


            //If we got this far, something failed, redisplay form
            model.HasAttachedDownload = !String.IsNullOrEmpty(model.AttachedDownloadId);
            model.AllowedTokens = _messageTokenProvider.GetListOfAllowedTokens();
            //available email accounts
            foreach (var ea in await _emailAccountService.GetAllEmailAccounts())
                model.AvailableEmailAccounts.Add(ea.ToModel());
            //Store
            await model.PrepareStoresMappingModel(null, _storeService, true);
            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Preview)]
        public async Task<IActionResult> Edit(string id)
        {
            var messageTemplate = await _messageTemplateService.GetMessageTemplateById(id);
            if (messageTemplate == null)
                //No message template found with the specified id
                return RedirectToAction("List");

            var model = messageTemplate.ToModel();
            model.SendImmediately = !model.DelayBeforeSend.HasValue;
            model.HasAttachedDownload = !String.IsNullOrEmpty(model.AttachedDownloadId);
            model.AllowedTokens = _messageTokenProvider.GetListOfAllowedTokens();
            //available email accounts
            foreach (var ea in await _emailAccountService.GetAllEmailAccounts())
                model.AvailableEmailAccounts.Add(ea.ToModel());
            //Store
            await model.PrepareStoresMappingModel(messageTemplate, _storeService, false);

            //locales
            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.BccEmailAddresses = messageTemplate.GetLocalized(x => x.BccEmailAddresses, languageId, false, false);
                locale.Subject = messageTemplate.GetLocalized(x => x.Subject, languageId, false, false);
                locale.Body = messageTemplate.GetLocalized(x => x.Body, languageId, false, false);

                var emailAccountId = messageTemplate.GetLocalized(x => x.EmailAccountId, languageId, false, false);
                locale.EmailAccountId = !String.IsNullOrEmpty(emailAccountId) ? emailAccountId : _emailAccountSettings.DefaultEmailAccountId;
            });

            return View(model);
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public async Task<IActionResult> Edit(MessageTemplateModel model, bool continueEditing)
        {
            var messageTemplate = await _messageTemplateService.GetMessageTemplateById(model.Id);
            if (messageTemplate == null)
                //No message template found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                messageTemplate = model.ToEntity(messageTemplate);
                //attached file
                if (!model.HasAttachedDownload)
                    messageTemplate.AttachedDownloadId = "";
                if (model.SendImmediately)
                    messageTemplate.DelayBeforeSend = null;

                await _messageTemplateService.UpdateMessageTemplate(messageTemplate);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.MessageTemplates.Updated"));

                if (continueEditing)
                {
                    //selected tab
                    await SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new { id = messageTemplate.Id });
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            model.HasAttachedDownload = !String.IsNullOrEmpty(model.AttachedDownloadId);
            model.AllowedTokens = _messageTokenProvider.GetListOfAllowedTokens();
            //available email accounts
            foreach (var ea in await _emailAccountService.GetAllEmailAccounts())
                model.AvailableEmailAccounts.Add(ea.ToModel());
            //Store
            await model.PrepareStoresMappingModel(messageTemplate, _storeService, true);

            return View(model);
        }
        
        [PermissionAuthorizeAction(PermissionActionName.Delete)]
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var messageTemplate = await _messageTemplateService.GetMessageTemplateById(id);
            if (messageTemplate == null)
                //No message template found with the specified id
                return RedirectToAction("List");

            await _messageTemplateService.DeleteMessageTemplate(messageTemplate);

            SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.MessageTemplates.Deleted"));
            return RedirectToAction("List");
        }

        [PermissionAuthorizeAction(PermissionActionName.Edit)]
        [HttpPost, ActionName("Edit")]
        [FormValueRequired("message-template-copy")]
        public async Task<IActionResult> CopyTemplate(MessageTemplateModel model)
        {
            var messageTemplate = await _messageTemplateService.GetMessageTemplateById(model.Id);
            if (messageTemplate == null)
                //No message template found with the specified id
                return RedirectToAction("List");

            try
            {
                var newMessageTemplate = await _messageTemplateService.CopyMessageTemplate(messageTemplate);
                SuccessNotification("The message template has been copied successfully");
                return RedirectToAction("Edit", new { id = newMessageTemplate.Id });
            }
            catch (Exception exc)
            {
                ErrorNotification(exc.Message);
                return RedirectToAction("Edit", new { id = model.Id });
            }
        }

        #endregion
    }
}