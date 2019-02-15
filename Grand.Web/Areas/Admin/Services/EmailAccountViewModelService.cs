using Grand.Core.Domain.Messages;
using Grand.Services.Messages;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Messages;

namespace Grand.Web.Areas.Admin.Services
{
    public partial class EmailAccountViewModelService: IEmailAccountViewModelService
    {
        private readonly IEmailAccountService _emailAccountService;
        private readonly IEmailSender _emailSender;

        public EmailAccountViewModelService(IEmailAccountService emailAccountService, IEmailSender emailSender)
        {
            _emailAccountService = emailAccountService;
            _emailSender = emailSender;
        }
        public virtual EmailAccountModel PrepareEmailAccountModel()
        {
            var model = new EmailAccountModel();
            //default values
            model.Port = 25;
            return model;
        }
        public virtual EmailAccount InsertEmailAccountModel(EmailAccountModel model)
        {
            var emailAccount = model.ToEntity();
            //set password manually
            emailAccount.Password = model.Password;
            _emailAccountService.InsertEmailAccount(emailAccount);
            return emailAccount;
        }
        public virtual EmailAccount UpdateEmailAccountModel(EmailAccount emailAccount, EmailAccountModel model)
        {
            emailAccount = model.ToEntity(emailAccount);
            _emailAccountService.UpdateEmailAccount(emailAccount);
            return emailAccount;
        }
        public virtual EmailAccount ChangePasswordEmailAccountModel(EmailAccount emailAccount, EmailAccountModel model)
        {
            emailAccount.Password = model.Password;
            _emailAccountService.UpdateEmailAccount(emailAccount);
            return emailAccount;
        }
        public virtual void SendTestEmail(EmailAccount emailAccount, EmailAccountModel model)
        {
            string subject = "Testing email functionality.";
            string body = "Email works fine.";
            _emailSender.SendEmail(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, model.SendTestEmailTo, null);
        }
    }
}
