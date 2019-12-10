using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Localization;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Localization
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