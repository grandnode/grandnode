using FluentValidation;
using Grand.Core.Validators;
using Grand.Services.Localization;
using Grand.Admin.Models.Directory;
using System.Collections.Generic;

namespace Grand.Admin.Validators.Directory
{
    public class MeasureUnitValidator : BaseGrandValidator<MeasureUnitModel>
    {
        public MeasureUnitValidator(
            IEnumerable<IValidatorConsumer<MeasureUnitModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Measures.Units.Fields.Name.Required"));
        }
    }
}