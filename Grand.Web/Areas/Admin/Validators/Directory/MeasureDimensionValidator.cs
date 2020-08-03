using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Directory;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Directory
{
    public class MeasureDimensionValidator : BaseGrandValidator<MeasureDimensionModel>
    {
        public MeasureDimensionValidator(
            IEnumerable<IValidatorConsumer<MeasureDimensionModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Measures.Dimensions.Fields.Name.Required"));
            RuleFor(x => x.SystemKeyword).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Measures.Dimensions.Fields.SystemKeyword.Required"));
        }
    }
}