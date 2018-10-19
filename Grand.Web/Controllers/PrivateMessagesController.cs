using Grand.Core;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Forums;
using Grand.Framework.Controllers;
using Grand.Framework.Security;
using Grand.Services.Customers;
using Grand.Services.Forums;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Web.Models.PrivateMessages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Grand.Web.Controllers
{
    public partial class PrivateMessagesController : BasePublicController
    {
        #region Fields

        private readonly IForumService _forumService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ForumSettings _forumSettings;
        private readonly CustomerSettings _customerSettings;

        #endregion

        #region Constructors

        public PrivateMessagesController(IForumService forumService,
            ICustomerService customerService, ICustomerActivityService customerActivityService,
            ILocalizationService localizationService, IWorkContext workContext, 
            IStoreContext storeContext, IDateTimeHelper dateTimeHelper,
            ForumSettings forumSettings, CustomerSettings customerSettings)
        {
            this._forumService = forumService;
            this._customerService = customerService;
            this._customerActivityService = customerActivityService;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._dateTimeHelper = dateTimeHelper;
            this._forumSettings = forumSettings;
            this._customerSettings = customerSettings;
        }

        #endregion

        #region Methods

        public virtual IActionResult Index(int? pageNumber, string tab)
        {
            if (!_forumSettings.AllowPrivateMessages)
            {
                return RedirectToRoute("HomePage");
            }

            if (_workContext.CurrentCustomer.IsGuest())
            {
                return Challenge();
            }

            int inboxPage = 0;
            int sentItemsPage = 0;
            bool sentItemsTabSelected = false;

            switch (tab)
            {
                case "inbox":
                    if (pageNumber.HasValue)
                    {
                        inboxPage = pageNumber.Value;
                    }
                    break;
                case "sent":
                    if (pageNumber.HasValue)
                    {
                        sentItemsPage = pageNumber.Value;
                    }
                    sentItemsTabSelected = true;
                    break;
                default:
                    break;
            }

            var model = new PrivateMessageIndexModel
            {
                InboxPage = inboxPage,
                SentItemsPage = sentItemsPage,
                SentItemsTabSelected = sentItemsTabSelected
            };

            return View(model);
        }

        [HttpPost, FormValueRequired("delete-inbox"), ActionName("InboxUpdate")]
        [PublicAntiForgery]
        public virtual IActionResult DeleteInboxPM(IFormCollection formCollection)
        {
            foreach (var key in formCollection.Keys)
            {
                var value = formCollection[key];

                if (value.Equals("on") && key.StartsWith("pm", StringComparison.OrdinalIgnoreCase))
                {
                    var id = key.Replace("pm", "").Trim();
                    var pm = _forumService.GetPrivateMessageById(id);
                    if (pm != null)
                    {
                        if (pm.ToCustomerId == _workContext.CurrentCustomer.Id)
                        {
                            pm.IsDeletedByRecipient = true;
                            _forumService.UpdatePrivateMessage(pm);
                        }
                    }
                }
            }
            return RedirectToRoute("PrivateMessages");
        }

        [HttpPost, FormValueRequired("mark-unread"), ActionName("InboxUpdate")]
        [PublicAntiForgery]
        public virtual IActionResult MarkUnread(IFormCollection formCollection)
        {
            foreach (var key in formCollection.Keys)
            {
                var value = formCollection[key];

                if (value.Equals("on") && key.StartsWith("pm", StringComparison.OrdinalIgnoreCase))
                {
                    var id = key.Replace("pm", "").Trim();
                    var pm = _forumService.GetPrivateMessageById(id);
                    if (pm != null)
                    {
                        if (pm.ToCustomerId == _workContext.CurrentCustomer.Id)
                        {
                            pm.IsRead = false;
                            _forumService.UpdatePrivateMessage(pm);
                        }
                    }
                }
            }
            return RedirectToRoute("PrivateMessages");
        }

        //updates sent items (deletes PrivateMessages)
        [HttpPost, FormValueRequired("delete-sent"), ActionName("SentUpdate")]
        [PublicAntiForgery]
        public virtual IActionResult DeleteSentPM(IFormCollection formCollection)
        {
            foreach (var key in formCollection.Keys)
            {
                var value = formCollection[key];

                if (value.Equals("on") && key.StartsWith("si", StringComparison.OrdinalIgnoreCase))
                {
                    var id = key.Replace("si", "").Trim();
                    PrivateMessage pm = _forumService.GetPrivateMessageById(id);
                    if (pm != null)
                    {
                        if (pm.FromCustomerId == _workContext.CurrentCustomer.Id)
                        {
                            pm.IsDeletedByAuthor = true;
                            _forumService.UpdatePrivateMessage(pm);
                        }
                    }
                }

            }
            return RedirectToRoute("PrivateMessages", new {tab = "sent"});
        }

        public virtual IActionResult SendPM(string toCustomerId, string replyToMessageId)
        {
            if (!_forumSettings.AllowPrivateMessages)
            {
                return RedirectToRoute("HomePage");
            }

            if (_workContext.CurrentCustomer.IsGuest())
            {
                return Challenge();
            }

            var customerTo = _customerService.GetCustomerById(toCustomerId);

            if (customerTo == null || customerTo.IsGuest())
            {
                return RedirectToRoute("PrivateMessages");
            }

            var model = new SendPrivateMessageModel();
            model.ToCustomerId = customerTo.Id;
            model.CustomerToName = customerTo.FormatUserName();
            model.AllowViewingToProfile = _customerSettings.AllowViewingProfiles && !customerTo.IsGuest();

            if (!String.IsNullOrEmpty(replyToMessageId))
            {
                var replyToPM = _forumService.GetPrivateMessageById(replyToMessageId);
                if (replyToPM == null)
                {
                    return RedirectToRoute("PrivateMessages");
                }

                if (replyToPM.ToCustomerId == _workContext.CurrentCustomer.Id || replyToPM.FromCustomerId == _workContext.CurrentCustomer.Id)
                {
                    model.ReplyToMessageId = replyToPM.Id;
                    model.Subject = string.Format("Re: {0}", replyToPM.Subject);
                }
                else
                {
                    return RedirectToRoute("PrivateMessages");
                }
            }
            return View(model);
        }

        [HttpPost]
        [PublicAntiForgery]
        public virtual IActionResult SendPM(SendPrivateMessageModel model)
        {
            if (!_forumSettings.AllowPrivateMessages)
            {
                return RedirectToRoute("HomePage");
            }

            if (_workContext.CurrentCustomer.IsGuest())
            {
                return Challenge();
            }

            Customer toCustomer = null;
            var replyToPM = _forumService.GetPrivateMessageById(model.ReplyToMessageId);
            if (replyToPM != null)
            {
                if (replyToPM.ToCustomerId == _workContext.CurrentCustomer.Id || replyToPM.FromCustomerId == _workContext.CurrentCustomer.Id)
                {
                    toCustomer = replyToPM.FromCustomerId == _workContext.CurrentCustomer.Id
                                ? _customerService.GetCustomerById(replyToPM.ToCustomerId)
                                : _customerService.GetCustomerById(replyToPM.FromCustomerId);

                }
                else
                {
                    return RedirectToRoute("PrivateMessages");
                }
            }
            else
            {
                toCustomer = _customerService.GetCustomerById(model.ToCustomerId);
            }

            if (toCustomer == null || toCustomer.IsGuest())
            {
                return RedirectToRoute("PrivateMessages");
            }
            model.ToCustomerId = toCustomer.Id;
            model.CustomerToName = toCustomer.FormatUserName();
            model.AllowViewingToProfile = _customerSettings.AllowViewingProfiles && !toCustomer.IsGuest();

            if (ModelState.IsValid)
            {
                try
                {
                    string subject = model.Subject;
                    if (_forumSettings.PMSubjectMaxLength > 0 && subject.Length > _forumSettings.PMSubjectMaxLength)
                    {
                        subject = subject.Substring(0, _forumSettings.PMSubjectMaxLength);
                    }

                    var text = model.Message;
                    if (_forumSettings.PMTextMaxLength > 0 && text.Length > _forumSettings.PMTextMaxLength)
                    {
                        text = text.Substring(0, _forumSettings.PMTextMaxLength);
                    }

                    var nowUtc = DateTime.UtcNow;

                    var privateMessage = new PrivateMessage
                    {
                        StoreId = _storeContext.CurrentStore.Id,
                        ToCustomerId = toCustomer.Id,
                        FromCustomerId = _workContext.CurrentCustomer.Id,
                        Subject = subject,
                        Text = text,
                        IsDeletedByAuthor = false,
                        IsDeletedByRecipient = false,
                        IsRead = false,
                        CreatedOnUtc = nowUtc
                    };

                    _forumService.InsertPrivateMessage(privateMessage);

                    //activity log
                    _customerActivityService.InsertActivity("PublicStore.SendPM", "", _localizationService.GetResource("ActivityLog.PublicStore.SendPM"), toCustomer.Email);

                    return RedirectToRoute("PrivateMessages", new { tab = "sent" });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            return View(model);
        }

        public virtual IActionResult ViewPM(string privateMessageId)
        {
            if (!_forumSettings.AllowPrivateMessages)
            {
                return RedirectToRoute("HomePage");
            }

            if (_workContext.CurrentCustomer.IsGuest())
            {
                return Challenge();
            }

            var pm = _forumService.GetPrivateMessageById(privateMessageId);
            if (pm != null)
            {
                if (pm.ToCustomerId != _workContext.CurrentCustomer.Id && pm.FromCustomerId != _workContext.CurrentCustomer.Id)
                {
                    return RedirectToRoute("PrivateMessages");
                }

                if (!pm.IsRead && pm.ToCustomerId == _workContext.CurrentCustomer.Id)
                {
                    pm.IsRead = true;
                    _forumService.UpdatePrivateMessage(pm);
                }
            }
            else
            {
                return RedirectToRoute("PrivateMessages");
            }

            var fromCustomer = _customerService.GetCustomerById(pm.FromCustomerId);
            var toCustomer = _customerService.GetCustomerById(pm.ToCustomerId);


            var model = new PrivateMessageModel
            {
                Id = pm.Id,
                FromCustomerId = fromCustomer.Id,
                CustomerFromName = fromCustomer.FormatUserName(),
                AllowViewingFromProfile = _customerSettings.AllowViewingProfiles && fromCustomer != null && !fromCustomer.IsGuest(),
                ToCustomerId = toCustomer.Id,
                CustomerToName = toCustomer.FormatUserName(),
                AllowViewingToProfile = _customerSettings.AllowViewingProfiles && toCustomer != null && !toCustomer.IsGuest(),
                Subject = pm.Subject,
                Message = pm.FormatPrivateMessageText(),
                CreatedOn = _dateTimeHelper.ConvertToUserTime(pm.CreatedOnUtc, DateTimeKind.Utc),
                IsRead = pm.IsRead,
            };

            return View(model);
        }

        public virtual IActionResult DeletePM(string privateMessageId)
        {
            if (!_forumSettings.AllowPrivateMessages)
            {
                return RedirectToRoute("HomePage");
            }

            if (_workContext.CurrentCustomer.IsGuest())
            {
                return Challenge();
            }

            var pm = _forumService.GetPrivateMessageById(privateMessageId);
            if (pm != null)
            {
                if (pm.FromCustomerId == _workContext.CurrentCustomer.Id)
                {
                    pm.IsDeletedByAuthor = true;
                    _forumService.UpdatePrivateMessage(pm);
                }

                if (pm.ToCustomerId == _workContext.CurrentCustomer.Id)
                {
                    pm.IsDeletedByRecipient = true;
                    _forumService.UpdatePrivateMessage(pm);
                }
            }
            return RedirectToRoute("PrivateMessages");
        }

        #endregion
    }
}
