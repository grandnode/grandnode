using FluentValidation;
using Grand.Web.Areas.Admin.Models.Settings;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Settings
{
    public class SettingValidator : BaseGrandValidator<SettingModel>
    {
        public SettingValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Settings.AllSettings.Fields.Name.Required"));
        }
    }
}