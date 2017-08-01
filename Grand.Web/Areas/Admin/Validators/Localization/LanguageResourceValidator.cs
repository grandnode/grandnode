using FluentValidation;
using Grand.Web.Areas.Admin.Models.Localization;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Localization
{
    public class LanguageResourceValidator : BaseGrandValidator<LanguageResourceModel>
    {
        public LanguageResourceValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Languages.Resources.Fields.Name.Required"));
            RuleFor(x => x.Value).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Languages.Resources.Fields.Value.Required"));
        }
    }
}