using Grand.Core;
using Grand.Domain.Messages;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Messages;
using Grand.Web.Commands.Models.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Commands.Handler.Common
{
    public class PopupInteractiveCommandHandler : IRequestHandler<PopupInteractiveCommand, IList<string>>
    {
        private readonly IInteractiveFormService _interactiveFormService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly ICustomerActivityService _customerActivityService;

        public PopupInteractiveCommandHandler(IInteractiveFormService interactiveFormService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IEmailAccountService emailAccountService,
            IQueuedEmailService queuedEmailService,
            ICustomerActivityService customerActivityService)
        {
            _interactiveFormService = interactiveFormService;
            _localizationService = localizationService;
            _workContext = workContext;
            _emailAccountService = emailAccountService;
            _queuedEmailService = queuedEmailService;
            _customerActivityService = customerActivityService;
        }

        public async Task<IList<string>> Handle(PopupInteractiveCommand request, CancellationToken cancellationToken)
        {
            var errors = new List<string>();
            var formid = request.Form["Id"];
            var form = await _interactiveFormService.GetFormById(formid);
            if (form == null)
                return errors;

            string enquiry = "";

            foreach (var item in form.FormAttributes)
            {
                enquiry += string.Format("{0}: {1} <br />", item.Name, request.Form[item.SystemName]);

                if (!string.IsNullOrEmpty(item.RegexValidation))
                {
                    var valuesStr = request.Form[item.SystemName];
                    Regex regex = new Regex(item.RegexValidation);
                    Match match = regex.Match(valuesStr);
                    if (!match.Success)
                    {
                        errors.Add(string.Format(_localizationService.GetResource("PopupInteractiveForm.Fields.Regex"), item.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id)));
                    }
                }
                if (item.IsRequired)
                {
                    var valuesStr = request.Form[item.SystemName];
                    if (string.IsNullOrEmpty(valuesStr))
                        errors.Add(string.Format(_localizationService.GetResource("PopupInteractiveForm.Fields.IsRequired"), item.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id)));
                }
                if (item.ValidationMinLength.HasValue)
                {
                    if (item.AttributeControlType == FormControlType.TextBox ||
                        item.AttributeControlType == FormControlType.MultilineTextbox)
                    {
                        var valuesStr = request.Form[item.SystemName].ToString();
                        int enteredTextLength = String.IsNullOrEmpty(valuesStr) ? 0 : valuesStr.Length;
                        if (item.ValidationMinLength.Value > enteredTextLength)
                        {
                            errors.Add(string.Format(_localizationService.GetResource("PopupInteractiveForm.Fields.TextboxMinimumLength"), item.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id), item.ValidationMinLength.Value));
                        }
                    }
                }
                if (item.ValidationMaxLength.HasValue)
                {
                    if (item.AttributeControlType == FormControlType.TextBox ||
                        item.AttributeControlType == FormControlType.MultilineTextbox)
                    {
                        var valuesStr = request.Form[item.SystemName].ToString();
                        int enteredTextLength = String.IsNullOrEmpty(valuesStr) ? 0 : valuesStr.Length;
                        if (item.ValidationMaxLength.Value < enteredTextLength)
                        {
                            errors.Add(string.Format(_localizationService.GetResource("PopupInteractiveForm.Fields.TextboxMaximumLength"), item.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id), item.ValidationMaxLength.Value));
                        }
                    }
                }

            }

            if (!errors.Any())
            {
                var emailAccount = await _emailAccountService.GetEmailAccountById(form.EmailAccountId);
                if (emailAccount == null)
                    emailAccount = (await _emailAccountService.GetAllEmailAccounts()).FirstOrDefault();
                if (emailAccount == null)
                    throw new Exception("No email account could be loaded");

                string from;
                string fromName;
                string subject = string.Format(_localizationService.GetResource("PopupInteractiveForm.EmailForm"), form.Name);
                from = emailAccount.Email;
                fromName = emailAccount.DisplayName;

                await _queuedEmailService.InsertQueuedEmail(new QueuedEmail {
                    From = from,
                    FromName = fromName,
                    To = emailAccount.Email,
                    ToName = emailAccount.DisplayName,
                    Priority = QueuedEmailPriority.High,
                    Subject = subject,
                    Body = enquiry,
                    CreatedOnUtc = DateTime.UtcNow,
                    EmailAccountId = emailAccount.Id
                });

                //activity log
                await _customerActivityService.InsertActivity("PublicStore.InteractiveForm", form.Id, string.Format(_localizationService.GetResource("ActivityLog.PublicStore.InteractiveForm"), form.Name));
            }

            return errors;
        }
    }
}
