using FluentValidation;
using Grand.Core.Validators;
using Grand.Services.Localization;
using Grand.Admin.Models.Localization;
using System.Collections.Generic;

namespace Grand.Admin.Validators.Localization
{
    public class LanguageResourceValidator : BaseGrandValidator<LanguageResourceModel>
    {
        public LanguageResourceValidator(
            IEnumerable<IValidatorConsumer<LanguageResourceModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Languages.Resources.Fields.Name.Required"));
            RuleFor(x => x.Value).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Languages.Resources.Fields.Value.Required"));
        }
    }
}