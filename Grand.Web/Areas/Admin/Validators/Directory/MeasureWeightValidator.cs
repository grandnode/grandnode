using FluentValidation;
using Grand.Web.Areas.Admin.Models.Directory;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Directory
{
    public class MeasureWeightValidator : BaseGrandValidator<MeasureWeightModel>
    {
        public MeasureWeightValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Measures.Weights.Fields.Name.Required"));
            RuleFor(x => x.SystemKeyword).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Measures.Weights.Fields.SystemKeyword.Required"));
        }
    }
}