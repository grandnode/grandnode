using FluentValidation;
using Grand.Admin.Models.Directory;
using Grand.Services.Localization;
using Grand.Web.Framework.Validators;

namespace Grand.Admin.Validators.Directory
{
    public class MeasureUnitValidator : BaseNopValidator<MeasureUnitModel>
    {
        public MeasureUnitValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Measures.Units.Fields.Name.Required"));
        }
    }
}