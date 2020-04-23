using FluentValidation.Attributes;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Grand.Web.Validators.Customer;

namespace Grand.Web.Models.Customer
{
    [Validator(typeof(PasswordRecoveryValidator))]
    public partial class PasswordRecoveryModel : BaseGrandModel
    {
        [GrandResourceDisplayName("Account.PasswordRecovery.Email")]
        public string Email { get; set; }
        public string Result { get; set; }
        public bool Send { get; set; }
        public bool DisplayCaptcha { get; set; }
    }
}