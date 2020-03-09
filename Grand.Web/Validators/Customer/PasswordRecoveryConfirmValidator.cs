﻿using FluentValidation;
using Grand.Core.Domain.Customers;
using Grand.Framework.Validators;
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
            RuleFor(x => x.ConfirmNewPassword).NotEmpty().WithMessage(localizationService.GetResource("Account.PasswordRecovery.ConfirmNewPassword.Required"));
            RuleFor(x => x.ConfirmNewPassword).Equal(x => x.NewPassword).WithMessage(localizationService.GetResource("Account.PasswordRecovery.NewPassword.EnteredPasswordsDoNotMatch"));
        }}
}