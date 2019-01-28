using Grand.Core.Domain.Tasks;
using Grand.Services.Logging;
using Grand.Services.Messages;
using System;

namespace Grand.Services.Tasks
{
    /// <summary>
    /// Represents a task for sending queued message 
    /// </summary>
    public partial class QueuedMessagesSendScheduleTask : ScheduleTask, IScheduleTask
    {
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly IEmailAccountService _emailAccountService;
        private readonly object _lock = new object();


        public QueuedMessagesSendScheduleTask(IQueuedEmailService queuedEmailService,
            IEmailSender emailSender, ILogger logger, IEmailAccountService emailAccountService)
        {
            this._queuedEmailService = queuedEmailService;
            this._emailSender = emailSender;
            this._logger = logger;
            this._emailAccountService = emailAccountService;
        }

        /// <summary>
        /// Executes a task
        /// </summary>
        public virtual void Execute()
        {
            lock (_lock)
            {
                var maxTries = 3;
                var queuedEmails = _queuedEmailService.SearchEmails(null, null, null, null, true, true, maxTries, false, 0, 500);
                foreach (var queuedEmail in queuedEmails)
                {
                    var bcc = String.IsNullOrWhiteSpace(queuedEmail.Bcc)
                                ? null
                                : queuedEmail.Bcc.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    var cc = String.IsNullOrWhiteSpace(queuedEmail.CC)
                                ? null
                                : queuedEmail.CC.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                    try
                    {
                        var emailAccount = _emailAccountService.GetEmailAccountById(queuedEmail.EmailAccountId);
                        _emailSender.SendEmail(emailAccount,
                            queuedEmail.Subject,
                            queuedEmail.Body,
                           queuedEmail.From,
                           queuedEmail.FromName,
                           queuedEmail.To,
                           queuedEmail.ToName,
                           queuedEmail.ReplyTo,
                           queuedEmail.ReplyToName,
                           bcc,
                           cc,
                           queuedEmail.AttachmentFilePath,
                           queuedEmail.AttachmentFileName,
                           queuedEmail.AttachedDownloads);

                        queuedEmail.SentOnUtc = DateTime.UtcNow;
                    }
                    catch (Exception exc)
                    {
                        _logger.Error(string.Format("Error sending e-mail. {0}", exc.Message), exc);
                    }
                    finally
                    {
                        queuedEmail.SentTries = queuedEmail.SentTries + 1;
                        _queuedEmailService.UpdateQueuedEmail(queuedEmail);
                    }
                }
            }
        }
    }
}
