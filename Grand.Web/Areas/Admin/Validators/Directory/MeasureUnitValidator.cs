using FluentValidation;
using Grand.Web.Areas.Admin.Models.Directory;
using Grand.Services.Localization;
using Grand.Framework.Validators;

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