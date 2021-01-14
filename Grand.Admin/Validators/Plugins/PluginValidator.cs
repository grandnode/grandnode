using FluentValidation;
using Grand.Core.Validators;
using Grand.Services.Localization;
using Grand.Admin.Models.Plugins;
using System.Collections.Generic;

namespace Grand.Admin.Validators.Plugins
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