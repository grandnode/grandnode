using FluentValidation;
using Grand.Domain.Customers;
using Grand.Core.Validators;
using Grand.Services.Localization;
using Grand.Web.Models.Customer;
using System.Collections.Generic;

namespace Grand.Web.Validators.Customer
{
    public class PasswordRecoveryConfirmValidator : BaseGrandValidator<PasswordRecoveryConfirmModel>
    {
        public PasswordRecoveryConfirmValidator(
            IEnumerable<IValidatorConsumer<PasswordRecoveryConfirmModel>> validators,
            ILocalizationService localizationService, CustomerSettings customerSettings)
            : base(validators)
        {
            RuleFor(x => x.NewPassword).NotEmpty().WithMessage(localizationService.GetResource("Account.PasswordRecovery.NewPassword.Required"));
            RuleFor(x => x.NewPassword).Length(customerSettings.PasswordMinLength, 999).WithMessage(string.Format(localizationService.GetResource("Account.PasswordRecovery.NewPassword.LengthValidation"), customerSettings.PasswordMinLength));

            if (!string.IsNullOrEmpty(customerSettings.PasswordRegularExpression))
                RuleFor(x => x.NewPassword).Matches(customerSettings.PasswordRegularExpression).WithMessage(string.Format(localizationService.GetResource("Account.ChangePassword.Fields.NewPassword.Validation")));

            RuleFor(x => x.ConfirmNewPassword).NotEmpty().WithMessage(localizationService.GetResource("Account.PasswordRecovery.ConfirmNewPassword.Required"));
            RuleFor(x => x.ConfirmNewPassword).Equal(x => x.NewPassword).WithMessage(localizationService.GetResource("Account.PasswordRecovery.NewPassword.EnteredPasswordsDoNotMatch"));
        }
    }
}