using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Installation;
using Grand.Web.Models.Install;
using System.Collections.Generic;

namespace Grand.Web.Validators.Install
{
    public class InstallValidator : BaseGrandValidator<InstallModel>
    {
        public InstallValidator(
            IEnumerable<IValidatorConsumer<InstallModel>> validators,
            IInstallationLocalizationService locService)
            : base(validators)
        {
            RuleFor(x => x.AdminEmail).NotEmpty().WithMessage(locService.GetResource("AdminEmailRequired"));
            RuleFor(x => x.AdminEmail).EmailAddress();
            RuleFor(x => x.AdminPassword).NotEmpty().WithMessage(locService.GetResource("AdminPasswordRequired"));
            RuleFor(x => x.ConfirmPassword).NotEmpty().WithMessage(locService.GetResource("ConfirmPasswordRequired"));
            RuleFor(x => x.AdminPassword).Equal(x => x.ConfirmPassword).WithMessage(locService.GetResource("PasswordsDoNotMatch"));
            
        }
    }
}