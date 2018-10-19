using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Directory;

namespace Grand.Web.Areas.Admin.Validators.Directory
{
    public class MeasureUnitValidator : BaseGrandValidator<MeasureUnitModel>
    {
        public MeasureUnitValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Measures.Units.Fields.Name.Required"));
        }
    }
}