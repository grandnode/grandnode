using Grand.Domain.Messages;
using Grand.Services.Media;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Messages
{
    /// <summary>
    /// Email sender
    /// </summary>
    public partial class EmailSender : IEmailSender
    {
        private readonly IDownloadService _downloadService;
        private readonly IMimeMappingService _mimeMappingService;

        public EmailSender(IDownloadService downloadService, IMimeMappingService mimeMappingService)
        {
            _downloadService = downloadService;
            _mimeMappingService = mimeMappingService;
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
        /// <param name="replyToAddress">ReplyTo address</param>
        /// <param name="replyToName">ReplyTo display name</param>
        /// <param name="bccAddresses">BCC addresses list</param>
        /// <param name="ccAddresses">CC addresses list</param>
        /// <param name="attachmentFilePath">Attachment file path</param>
        /// <param name="attachmentFileName">Attachment file name. If specified, then this file name will be sent to a recipient. Otherwise, "AttachmentFilePath" name will be used.</param>
        /// <param name="attachedDownloads">Attachment download ID (another attachedment)</param>
        public virtual async Task SendEmail(EmailAccount emailAccount, string subject, string body,
            string fromAddress, string fromName, string toAddress, string toName,
             string replyToAddress = null, string replyToName = null,
            IEnumerable<string> bccAddresses = null, IEnumerable<string> ccAddresses = null,
            string attachmentFilePath = null, string attachmentFileName = null,
            IEnumerable<string> attachedDownloads = null)

        {
            var message = new MimeMessage();
            //from, to, reply to
            message.From.Add(new MailboxAddress(fromName, fromAddress));
            message.To.Add(new MailboxAddress(toName, toAddress));
            if (!String.IsNullOrEmpty(replyToAddress))
            {
                message.ReplyTo.Add(new MailboxAddress(replyToName, replyToAddress));
            }

            //BCC
            if (bccAddresses != null && bccAddresses.Any())
            {
                foreach (var address in bccAddresses.Where(bccValue => !String.IsNullOrWhiteSpace(bccValue)))
                {
                    message.Bcc.Add(MailboxAddress.Parse(address.Trim()));
                }
            }

            //CC
            if (ccAddresses != null && ccAddresses.Any())
            {
                foreach (var address in ccAddresses.Where(ccValue => !String.IsNullOrWhiteSpace(ccValue)))
                {
                    message.Cc.Add(MailboxAddress.Parse(address.Trim()));
                }
            }

            //subject
            message.Subject = subject;

            //content
            var builder = new BodyBuilder();
            builder.HtmlBody = body;

            //create  the file attachment for this e-mail message
            if (!String.IsNullOrEmpty(attachmentFilePath) &&
                File.Exists(attachmentFilePath))
            {

                // TODO: should probably include a check for the attachmentFileName not being null or white space
                var attachment = new MimePart(_mimeMappingService.Map(attachmentFileName))
                {
                    Content = new MimeContent(File.OpenRead(attachmentFilePath), ContentEncoding.Default),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment)
                    {
                        CreationDate = File.GetCreationTime(attachmentFilePath),
                        ModificationDate = File.GetLastWriteTime(attachmentFilePath),
                        ReadDate = File.GetLastAccessTime(attachmentFilePath)
                    },
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = Path.GetFileName(attachmentFilePath),
                };
                builder.Attachments.Add(attachment);

            }
            //another attachment?
            if (attachedDownloads != null)
            {
                foreach (var attachedDownloadId in attachedDownloads)
                {
                    var download = await _downloadService.GetDownloadById(attachedDownloadId);
                    if (download != null)
                    {
                        //we do not support URLs as attachments
                        if (!download.UseDownloadUrl)
                        {
                            string fileName = !String.IsNullOrWhiteSpace(download.Filename) ? download.Filename : download.Id;
                            fileName += download.Extension;
                            var ms = new MemoryStream(download.DownloadBinary);
                            var attachment = new MimePart(download.ContentType ?? _mimeMappingService.Map(fileName))
                            {
                                Content = new MimeContent(ms, ContentEncoding.Default),
                                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment)
                                {
                                    CreationDate = DateTime.UtcNow,
                                    ModificationDate = DateTime.UtcNow,
                                    ReadDate = DateTime.UtcNow
                                },
                                ContentTransferEncoding = ContentEncoding.Base64,
                                FileName = fileName,
                            };
                            builder.Attachments.Add(attachment);
                        }
                    }
                }
            }
            message.Body = builder.ToMessageBody();

            //send email
            using (var smtpClient = new SmtpClient())
            {
                smtpClient.ServerCertificateValidationCallback = (s, c, h, e) => emailAccount.UseServerCertificateValidation;
                await smtpClient.ConnectAsync(emailAccount.Host, emailAccount.Port, (SecureSocketOptions)emailAccount.SecureSocketOptionsId);
                await smtpClient.AuthenticateAsync(emailAccount.Username, emailAccount.Password);
                await smtpClient.SendAsync(message);
                await smtpClient.DisconnectAsync(true);
            }

        }
    }
}