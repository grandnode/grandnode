using Microsoft.AspNetCore.Mvc;
using FluentValidation.Attributes;
using Grand.Framework;
using Grand.Framework.Mvc.Models;
using Grand.Web.Validators.Customer;
using Grand.Framework.Mvc.ModelBinding;

namespace Grand.Web.Models.Customer
{
    [Validator(typeof(PasswordRecoveryValidator))]
    public partial class PasswordRecoveryModel : BaseGrandModel
    {
        [GrandResourceDisplayName("Account.PasswordRecovery.Email")]
        public string Email { get; set; }

        public string Result { get; set; }
    }
}