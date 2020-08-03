using Grand.Core;
using Grand.Domain.Messages;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Services.Security;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Messages;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.MessageQueue)]
    public partial class QueuedEmailController : BaseAdminController
	{
		private readonly IQueuedEmailService _queuedEmailService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;

		public QueuedEmailController(IQueuedEmailService queuedEmailService,
            IEmailAccountService emailAccountService,
            IDateTimeHelper dateTimeHelper, 
            ILocalizationService localizationService,
            IWorkContext workContext)
		{
            _queuedEmailService = queuedEmailService;
            _emailAccountService = emailAccountService;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _workContext = workContext;
		}

        public IActionResult Index() => RedirectToAction("List");

		public IActionResult List()
        {
		    var model = new QueuedEmailListModel
		    {
                //default value
		        SearchMaxSentTries = 10
		    };
            return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> QueuedEmailList(DataSourceRequest command, QueuedEmailListModel model)
        {
            DateTime? startDateValue = (model.SearchStartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.SearchStartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.SearchEndDate == null) ? null 
                            :(DateTime?)_dateTimeHelper.ConvertToUtcTime(model.SearchEndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var queuedEmails = await _queuedEmailService.SearchEmails(model.SearchFromEmail, model.SearchToEmail, 
                startDateValue, endDateValue, 
                model.SearchLoadNotSent, false, model.SearchMaxSentTries, true,
                command.Page - 1, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = queuedEmails.Select(x => {
                    var m = x.ToModel();
                    m.PriorityName = x.Priority.GetLocalizedEnum(_localizationService, _workContext);
                    m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                    if (x.DontSendBeforeDateUtc.HasValue)
                        m.DontSendBeforeDate = _dateTimeHelper.ConvertToUserTime(x.DontSendBeforeDateUtc.Value, DateTimeKind.Utc);
                    if (x.SentOnUtc.HasValue)
                        m.SentOn = _dateTimeHelper.ConvertToUserTime(x.SentOnUtc.Value, DateTimeKind.Utc);

                    m.Body = "";

                    return m;
                }),
                Total = queuedEmails.TotalCount
            };
            return Json(gridModel);
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("go-to-email-by-number")]
        public async Task<IActionResult> GoToEmailByNumber(QueuedEmailListModel model)
        {
            var queuedEmail = await _queuedEmailService.GetQueuedEmailById(model.GoDirectlyToNumber);
            if (queuedEmail == null)
                return List();
            
            return RedirectToAction("Edit", "QueuedEmail", new { id = queuedEmail.Id });
        }

		public async Task<IActionResult> Edit(string id)
        {
			var email = await _queuedEmailService.GetQueuedEmailById(id);
            if (email == null)
                //No email found with the specified id
                return RedirectToAction("List");

            var model = email.ToModel();
            model.PriorityName = email.Priority.GetLocalizedEnum(_localizationService, _workContext);
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(email.CreatedOnUtc, DateTimeKind.Utc);
            model.EmailAccountName = (await _emailAccountService.GetEmailAccountById(email.EmailAccountId)).DisplayName;
            if (email.SentOnUtc.HasValue)
                model.SentOn = _dateTimeHelper.ConvertToUserTime(email.SentOnUtc.Value, DateTimeKind.Utc);
            if (email.DontSendBeforeDateUtc.HasValue)
                model.DontSendBeforeDate = _dateTimeHelper.ConvertToUserTime(email.DontSendBeforeDateUtc.Value, DateTimeKind.Utc);
            else model.SendImmediately = true;

            return View(model);
		}

        [HttpPost, ActionName("Edit")]
        [ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public async Task<IActionResult> Edit(QueuedEmailModel model, bool continueEditing)
        {
            var email = await _queuedEmailService.GetQueuedEmailById(model.Id);
            if (email == null)
                //No email found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                email = model.ToEntity(email);
                email.DontSendBeforeDateUtc = (model.SendImmediately || !model.DontSendBeforeDate.HasValue) ?
                    null : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.DontSendBeforeDate.Value);
                await _queuedEmailService.UpdateQueuedEmail(email);

                SuccessNotification(_localizationService.GetResource("Admin.System.QueuedEmails.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = email.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            model.PriorityName = email.Priority.GetLocalizedEnum(_localizationService, _workContext);
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(email.CreatedOnUtc, DateTimeKind.Utc);
            if (email.SentOnUtc.HasValue)
                model.SentOn = _dateTimeHelper.ConvertToUserTime(email.SentOnUtc.Value, DateTimeKind.Utc);
            if (email.DontSendBeforeDateUtc.HasValue)
                model.DontSendBeforeDate = _dateTimeHelper.ConvertToUserTime(email.DontSendBeforeDateUtc.Value, DateTimeKind.Utc);

            return View(model);
		}

        [HttpPost, ActionName("Edit"), FormValueRequired("requeue")]
        public async Task<IActionResult> Requeue(QueuedEmailModel queuedEmailModel)
        {
            var queuedEmail = await _queuedEmailService.GetQueuedEmailById(queuedEmailModel.Id);
            if (queuedEmail == null)
                //No email found with the specified id
                return RedirectToAction("List");

            var requeuedEmail = new QueuedEmail
            {
                PriorityId = queuedEmail.PriorityId,
                From = queuedEmail.From,
                FromName = queuedEmail.FromName,
                To = queuedEmail.To,
                ToName = queuedEmail.ToName,
                ReplyTo = queuedEmail.ReplyTo,
                ReplyToName = queuedEmail.ReplyToName,
                CC = queuedEmail.CC,
                Bcc = queuedEmail.Bcc,
                Subject = queuedEmail.Subject,
                Body = queuedEmail.Body,
                AttachmentFilePath = queuedEmail.AttachmentFilePath,
                AttachmentFileName = queuedEmail.AttachmentFileName,
                AttachedDownloads = queuedEmail.AttachedDownloads,
                CreatedOnUtc = DateTime.UtcNow,
                EmailAccountId = queuedEmail.EmailAccountId,
                DontSendBeforeDateUtc = (queuedEmailModel.SendImmediately || !queuedEmailModel.DontSendBeforeDate.HasValue) ?
                    null : (DateTime?)_dateTimeHelper.ConvertToUtcTime(queuedEmailModel.DontSendBeforeDate.Value)
            };
            await _queuedEmailService.InsertQueuedEmail(requeuedEmail);

            SuccessNotification(_localizationService.GetResource("Admin.System.QueuedEmails.Requeued"));
            return RedirectToAction("Edit", new { id = requeuedEmail.Id });
        }

	    [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
			var email = await _queuedEmailService.GetQueuedEmailById(id);
            if (email == null)
                //No email found with the specified id
                return RedirectToAction("List");

            await _queuedEmailService.DeleteQueuedEmail(email);

            SuccessNotification(_localizationService.GetResource("Admin.System.QueuedEmails.Deleted"));
			return RedirectToAction("List");
		}

        [HttpPost]
        public async Task<IActionResult> DeleteSelected(ICollection<string> selectedIds)
        {
            if (selectedIds != null)
            {
                var queuedEmails = await _queuedEmailService.GetQueuedEmailsByIds(selectedIds.ToArray());
                foreach (var queuedEmail in queuedEmails)
                    await _queuedEmailService.DeleteQueuedEmail(queuedEmail);
            }

            return Json(new { Result = true });
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("delete-all")]
        public async Task<IActionResult> DeleteAll()
        {
            await _queuedEmailService.DeleteAllEmails();
            SuccessNotification(_localizationService.GetResource("Admin.System.QueuedEmails.DeletedAll"));
            return RedirectToAction("List");
        }
	}
}
