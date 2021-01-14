using FluentValidation;
using Grand.Core.Validators;
using Grand.Services.Localization;
using Grand.Admin.Models.Directory;
using System.Collections.Generic;

namespace Grand.Admin.Validators.Directory
{
    public class MeasureWeightValidator : BaseGrandValidator<MeasureWeightModel>
    {
        public MeasureWeightValidator(
            IEnumerable<IValidatorConsumer<MeasureWeightModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Measures.Weights.Fields.Name.Required"));
            RuleFor(x => x.SystemKeyword).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Measures.Weights.Fields.SystemKeyword.Required"));
        }
    }
}