using Grand.Core.ModelBinding;
using Grand.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Models.Customer
{
    public partial class PasswordRecoveryModel : BaseModel
    {
        [DataType(DataType.EmailAddress)]
        [GrandResourceDisplayName("Account.PasswordRecovery.Email")]
        public string Email { get; set; }
        public string Result { get; set; }
        public bool Send { get; set; }
        public bool DisplayCaptcha { get; set; }
    }
}