using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Catalog;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Catalog
{
    public class PredefinedProductAttributeValueModelValidator : BaseGrandValidator<PredefinedProductAttributeValueModel>
    {
        public PredefinedProductAttributeValueModelValidator(
            IEnumerable<IValidatorConsumer<PredefinedProductAttributeValueModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Admin.Catalog.Attributes.ProductAttributes.PredefinedValues.Fields.Name.Required"));
        }
    }
}