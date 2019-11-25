using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Plugins;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Plugins
{
    public class PluginValidator : BaseGrandValidator<PluginModel>
    {
        public PluginValidator(
            IEnumerable<IValidatorConsumer<PluginModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.FriendlyName).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Plugins.Fields.FriendlyName.Required"));
        }
    }
}