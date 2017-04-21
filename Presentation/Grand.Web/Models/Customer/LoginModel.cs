using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;
using Grand.Web.Validators.Customer;

namespace Grand.Web.Models.Customer
{
    [Validator(typeof(LoginValidator))]
    public partial class LoginModel : BaseGrandModel
    {
        public bool CheckoutAsGuest { get; set; }

        [GrandResourceDisplayName("Account.Login.Fields.Email")]
        [AllowHtml]
        public string Email { get; set; }

        public bool UsernamesEnabled { get; set; }
        [GrandResourceDisplayName("Account.Login.Fields.UserName")]
        [AllowHtml]
        public string Username { get; set; }

        [DataType(DataType.Password)]
        [NoTrim]
        [GrandResourceDisplayName("Account.Login.Fields.Password")]
        [AllowHtml]
        public string Password { get; set; }

        [GrandResourceDisplayName("Account.Login.Fields.RememberMe")]
        public bool RememberMe { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}