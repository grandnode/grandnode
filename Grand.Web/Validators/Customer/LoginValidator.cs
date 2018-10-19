using FluentValidation;
using Grand.Core.Domain.Customers;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Models.Customer;

namespace Grand.Web.Validators.Customer
{
    public class LoginValidator : BaseGrandValidator<LoginModel>
    {
        public LoginValidator(ILocalizationService localizationService, CustomerSettings customerSettings)
        {
            if (!customerSettings.UsernamesEnabled)
            {
                //login by email
                RuleFor(x => x.Email).NotEmpty().WithMessage(localizationService.GetResource("Account.Login.Fields.Email.Required"));
                RuleFor(x => x.Email).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
            }
        }
    }
}