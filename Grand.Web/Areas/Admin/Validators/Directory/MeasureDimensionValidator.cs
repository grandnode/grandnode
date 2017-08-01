using FluentValidation;
using Grand.Web.Areas.Admin.Models.Directory;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Directory
{
    public class MeasureDimensionValidator : BaseGrandValidator<MeasureDimensionModel>
    {
        public MeasureDimensionValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Measures.Dimensions.Fields.Name.Required"));
            RuleFor(x => x.SystemKeyword).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Measures.Dimensions.Fields.SystemKeyword.Required"));
        }
    }
}