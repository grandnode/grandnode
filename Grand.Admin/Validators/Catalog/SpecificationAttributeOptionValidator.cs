using FluentValidation;
using Grand.Core.Validators;
using Grand.Services.Localization;
using Grand.Admin.Models.Catalog;
using System.Collections.Generic;

namespace Grand.Admin.Validators.Catalog
{
    public class SpecificationAttributeOptionValidator : BaseGrandValidator<SpecificationAttributeOptionModel>
    {
        public SpecificationAttributeOptionValidator(
            IEnumerable<IValidatorConsumer<SpecificationAttributeOptionModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Attributes.SpecificationAttributes.Options.Fields.Name.Required"));
        }
    }
}