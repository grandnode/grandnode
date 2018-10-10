using Grand.Core;
using Grand.Core.Domain.Messages;
using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc.Filters;
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

namespace Grand.Web.Areas.Admin.Controllers
{
    public partial class QueuedEmailController : BaseAdminController
	{
		private readonly IQueuedEmailService _queuedEmailService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;

		public QueuedEmailController(IQueuedEmailService queuedEmailService,
            IEmailAccountService emailAccountService,
            IDateTimeHelper dateTimeHelper, 
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IWorkContext workContext)
		{
            this._queuedEmailService = queuedEmailService;
            this._emailAccountService = emailAccountService;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._workContext = workContext;
		}

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

		public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageQueue))
                return AccessDeniedView();

		    var model = new QueuedEmailListModel
		    {
                //default value
		        SearchMaxSentTries = 10
		    };
            return View(model);
		}

		[HttpPost]
		public IActionResult QueuedEmailList(DataSourceRequest command, QueuedEmailListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageQueue))
                return AccessDeniedView();

            DateTime? startDateValue = (model.SearchStartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.SearchStartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.SearchEndDate == null) ? null 
                            :(DateTime?)_dateTimeHelper.ConvertToUtcTime(model.SearchEndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var queuedEmails = _queuedEmailService.SearchEmails(model.SearchFromEmail, model.SearchToEmail, 
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
        public IActionResult GoToEmailByNumber(QueuedEmailListModel model)
        {
            var queuedEmail = _queuedEmailService.GetQueuedEmailById(model.GoDirectlyToNumber);
            if (queuedEmail == null)
                return List();
            
            return RedirectToAction("Edit", "QueuedEmail", new { id = queuedEmail.Id });
        }

		public IActionResult Edit(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageQueue))
                return AccessDeniedView();

			var email = _queuedEmailService.GetQueuedEmailById(id);
            if (email == null)
                //No email found with the specified id
                return RedirectToAction("List");

            var model = email.ToModel();
            model.PriorityName = email.Priority.GetLocalizedEnum(_localizationService, _workContext);
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(email.CreatedOnUtc, DateTimeKind.Utc);
            model.EmailAccountName = _emailAccountService.GetEmailAccountById(email.EmailAccountId).DisplayName;
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
        public IActionResult Edit(QueuedEmailModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageQueue))
                return AccessDeniedView();

            var email = _queuedEmailService.GetQueuedEmailById(model.Id);
            if (email == null)
                //No email found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                email = model.ToEntity(email);
                email.DontSendBeforeDateUtc = (model.SendImmediately || !model.DontSendBeforeDate.HasValue) ?
                    null : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.DontSendBeforeDate.Value);
                _queuedEmailService.UpdateQueuedEmail(email);

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
        public IActionResult Requeue(QueuedEmailModel queuedEmailModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageQueue))
                return AccessDeniedView();

            var queuedEmail = _queuedEmailService.GetQueuedEmailById(queuedEmailModel.Id);
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
                AttachedDownloadId = queuedEmail.AttachedDownloadId,
                CreatedOnUtc = DateTime.UtcNow,
                EmailAccountId = queuedEmail.EmailAccountId,
                DontSendBeforeDateUtc = (queuedEmailModel.SendImmediately || !queuedEmailModel.DontSendBeforeDate.HasValue) ?
                    null : (DateTime?)_dateTimeHelper.ConvertToUtcTime(queuedEmailModel.DontSendBeforeDate.Value)
            };
            _queuedEmailService.InsertQueuedEmail(requeuedEmail);

            SuccessNotification(_localizationService.GetResource("Admin.System.QueuedEmails.Requeued"));
            return RedirectToAction("Edit", new { id = requeuedEmail.Id });
        }

	    [HttpPost]
        public IActionResult Delete(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageQueue))
                return AccessDeniedView();

			var email = _queuedEmailService.GetQueuedEmailById(id);
            if (email == null)
                //No email found with the specified id
                return RedirectToAction("List");

            _queuedEmailService.DeleteQueuedEmail(email);

            SuccessNotification(_localizationService.GetResource("Admin.System.QueuedEmails.Deleted"));
			return RedirectToAction("List");
		}

        [HttpPost]
        public IActionResult DeleteSelected(ICollection<string> selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageQueue))
                return AccessDeniedView();

            if (selectedIds != null)
            {
                var queuedEmails = _queuedEmailService.GetQueuedEmailsByIds(selectedIds.ToArray());
                foreach (var queuedEmail in queuedEmails)
                    _queuedEmailService.DeleteQueuedEmail(queuedEmail);
            }

            return Json(new { Result = true });
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("delete-all")]
        public IActionResult DeleteAll()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageQueue))
                return AccessDeniedView();

            _queuedEmailService.DeleteAllEmails();

            SuccessNotification(_localizationService.GetResource("Admin.System.QueuedEmails.DeletedAll"));
            return RedirectToAction("List");
        }

	}
}
