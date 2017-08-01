using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using Grand.Core.Domain.Messages;
using Grand.Services.Media;
using MailKit.Net.Smtp;
using MimeKit;

namespace Grand.Services.Messages
{
    /// <summary>
    /// Email sender
    /// </summary>
    public partial class EmailSender : IEmailSender
    {
        private readonly IDownloadService _downloadService;

        public EmailSender(IDownloadService downloadService)
        {
            this._downloadService = downloadService;
        }

        /// <summary>
        /// Sends an email
        /// </summary>
        /// <param name="emailAccount">Email account to use</param>
        /// <param name="subject">Subject</param>
        /// <param name="body">Body</param>
        /// <param name="fromAddress">From address</param>
        /// <param name="fromName">From display name</param>
        /// <param name="toAddress">To address</param>
        /// <param name="toName">To display name</param>
        /// <param name="replyTo">ReplyTo address</param>
        /// <param name="replyToName">ReplyTo display name</param>
        /// <param name="bcc">BCC addresses list</param>
        /// <param name="cc">CC addresses list</param>
        /// <param name="attachmentFilePath">Attachment file path</param>
        /// <param name="attachmentFileName">Attachment file name. If specified, then this file name will be sent to a recipient. Otherwise, "AttachmentFilePath" name will be used.</param>
        /// <param name="attachedDownloadId">Attachment download ID (another attachedment)</param>
        public virtual void SendEmail(EmailAccount emailAccount, string subject, string body,
            string fromAddress, string fromName, string toAddress, string toName,
             string replyTo = null, string replyToName = null,
            IEnumerable<string> bcc = null, IEnumerable<string> cc = null,
            string attachmentFilePath = null, string attachmentFileName = null,
            string attachedDownloadId = "")
        {
            var message = new MimeMessage();
            //from
            message.From.Add(new MailboxAddress(fromName, fromAddress));
            //to
            message.To.Add(new MailboxAddress(toName, toAddress));
            //reply to list
            if (!String.IsNullOrEmpty(replyTo))
            {
                message.InReplyTo = replyTo;
            }
            //bcc
            if (bcc != null)
            {
                foreach (var address in bcc.Where(bccValue => !String.IsNullOrWhiteSpace(bccValue)))
                {
                    message.Bcc.Add(new MailboxAddress(address.Trim()));
                }
            }
            //cc
            if (cc != null)
            {
                foreach (var address in cc.Where(ccValue => !String.IsNullOrWhiteSpace(ccValue)))
                {
                    message.Cc.Add(new MailboxAddress(address.Trim()));
                }
            }
            //subject
            message.Subject = subject;
            //body
            message.Body = new TextPart("plain") { Text = body };
            //attachment
            if (!String.IsNullOrEmpty(attachmentFilePath) &&
                File.Exists(attachmentFilePath))
            {

                var builder = new BodyBuilder();
                builder.TextBody = body;
                builder.Attachments.Add(attachmentFilePath);
                message.Body = builder.ToMessageBody();
            }
            //another attachment
            if (!String.IsNullOrEmpty(attachedDownloadId))
            {
                var download = _downloadService.GetDownloadById(attachedDownloadId);
                if (download != null)
                {
                    //we do not support URLs as attachments
                    if (!download.UseDownloadUrl)
                    {
                        string fileName = !String.IsNullOrWhiteSpace(download.Filename) ? download.Filename : download.Id;
                        fileName += download.Extension;
                        var builder = new BodyBuilder()
                        {
                            TextBody = body
                        };
                        var ms = new MemoryStream(download.DownloadBinary);
                        builder.Attachments.Add(download.Filename, ms.ToArray());
                        message.Body = builder.ToMessageBody();
                    }
                }
            }
            //send email
            using (var client = new SmtpClient())
            {
                client.Connect(emailAccount.Host, emailAccount.Port, emailAccount.EnableSsl);
                client.Authenticate(emailAccount.Email, emailAccount.Password);
                client.Send(message);
                client.Disconnect(true);
            }
        }

    }
}
