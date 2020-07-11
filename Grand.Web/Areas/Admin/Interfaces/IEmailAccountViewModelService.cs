using Grand.Domain.Messages;
using Grand.Web.Areas.Admin.Models.Messages;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface IEmailAccountViewModelService
    {
        EmailAccountModel PrepareEmailAccountModel();
        Task<EmailAccount> InsertEmailAccountModel(EmailAccountModel model);
        Task<EmailAccount> UpdateEmailAccountModel(EmailAccount emailAccount, EmailAccountModel model);
        Task<EmailAccount> ChangePasswordEmailAccountModel(EmailAccount emailAccount, EmailAccountModel model);
        Task SendTestEmail(EmailAccount emailAccount, EmailAccountModel model);
    }
}
