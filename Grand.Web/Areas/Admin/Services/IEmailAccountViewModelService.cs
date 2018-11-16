using Grand.Core.Domain.Messages;
using Grand.Web.Areas.Admin.Models.Messages;

namespace Grand.Web.Areas.Admin.Services
{
    public interface IEmailAccountViewModelService
    {
        EmailAccountModel PrepareEmailAccountModel();
        EmailAccount InsertEmailAccountModel(EmailAccountModel model);
        EmailAccount UpdateEmailAccountModel(EmailAccount emailAccount, EmailAccountModel model);
        EmailAccount ChangePasswordEmailAccountModel(EmailAccount emailAccount, EmailAccountModel model);
        void SenTestEmail(EmailAccount emailAccount, EmailAccountModel model);
    }
}
