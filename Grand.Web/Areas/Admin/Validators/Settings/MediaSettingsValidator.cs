using System.Collections.Generic;
using FluentValidation;
using Grand.Core.Extensions;
using Grand.Core.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Settings;

namespace Grand.Web.Areas.Admin.Validators.Settings
{
    public class MediaSettingsValidator: BaseGrandValidator<MediaSettingsModel>
    {
        public MediaSettingsValidator(IEnumerable<IValidatorConsumer<MediaSettingsModel>> validators,
            ILocalizationService localizationService) : base(validators)
        {
            RuleFor(x => x.PhysicalPath)
                .Must(FluentValidationUtilities.IsValidPath)
                .When(x => !string.IsNullOrEmpty(x.PhysicalPath))
                .WithMessage(localizationService.GetResource("Admin.Configuration.Settings.Media.PhysicalPath.InvalidPath"));
        }
    }
}