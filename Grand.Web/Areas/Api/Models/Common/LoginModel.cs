using FluentValidation.Attributes;
using Grand.Web.Areas.Api.Validators.Common;

namespace Grand.Web.Areas.Api.Models.Common
{
    [Validator(typeof(LoginValidator))]
    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
