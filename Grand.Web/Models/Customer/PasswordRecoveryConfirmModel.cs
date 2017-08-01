using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using FluentValidation.Attributes;
using Grand.Framework;
using Grand.Framework.Mvc.Models;
using Grand.Web.Validators.Customer;
using Grand.Framework.Mvc.ModelBinding;

namespace Grand.Web.Models.Customer
{
    [Validator(typeof(PasswordRecoveryConfirmValidator))]
    public partial class PasswordRecoveryConfirmModel : BaseGrandModel
    {
        [NoTrim]
        [DataType(DataType.Password)]
        [GrandResourceDisplayName("Account.PasswordRecovery.NewPassword")]
        public string NewPassword { get; set; }

        [NoTrim]
        [DataType(DataType.Password)]
        [GrandResourceDisplayName("Account.PasswordRecovery.ConfirmNewPassword")]
        public string ConfirmNewPassword { get; set; }

        public bool DisablePasswordChanging { get; set; }
        public string Result { get; set; }
    }
}