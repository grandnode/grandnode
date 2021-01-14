using FluentValidation;
using Grand.Core.Validators;
using Grand.Services.Localization;
using Grand.Admin.Models.Settings;
using System.Collections.Generic;

namespace Grand.Admin.Validators.Settings
{
    public class SettingValidator : BaseGrandValidator<SettingModel>
    {
        public SettingValidator(
            IEnumerable<IValidatorConsumer<SettingModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Settings.AllSettings.Fields.Name.Required"));
        }
    }
}