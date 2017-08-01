using FluentValidation;
using Grand.Services.Localization;
using Grand.Framework.Validators;
using Grand.Web.Models.Customer;

namespace Grand.Web.Validators.Customer
{
    public class PasswordRecoveryValidator : BaseGrandValidator<PasswordRecoveryModel>
    {
        public PasswordRecoveryValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage(localizationService.GetResource("Account.PasswordRecovery.Email.Required"));
            RuleFor(x => x.Email).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
        }}
}