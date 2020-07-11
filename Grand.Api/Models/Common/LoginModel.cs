using FluentValidation.Attributes;
using Grand.Api.Validators.Common;

namespace Grand.Api.Models.Common
{
    [Validator(typeof(LoginValidator))]
    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
