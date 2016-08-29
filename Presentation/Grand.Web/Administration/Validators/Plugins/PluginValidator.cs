using FluentValidation;
using Grand.Admin.Models.Plugins;
using Grand.Services.Localization;
using Grand.Web.Framework.Validators;

namespace Grand.Admin.Validators.Plugins
{
    public class PluginValidator : BaseNopValidator<PluginModel>
    {
        public PluginValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.FriendlyName).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Plugins.Fields.FriendlyName.Required"));
        }
    }
}