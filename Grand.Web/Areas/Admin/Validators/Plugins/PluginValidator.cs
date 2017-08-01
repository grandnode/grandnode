using FluentValidation;
using Grand.Web.Areas.Admin.Models.Plugins;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Plugins
{
    public class PluginValidator : BaseGrandValidator<PluginModel>
    {
        public PluginValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.FriendlyName).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Plugins.Fields.FriendlyName.Required"));
        }
    }
}