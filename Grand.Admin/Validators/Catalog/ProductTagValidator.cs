using FluentValidation;
using Grand.Core.Validators;
using Grand.Services.Localization;
using Grand.Admin.Models.Catalog;
using System.Collections.Generic;

namespace Grand.Admin.Validators.Catalog
{
    public class ProductTagValidator : BaseGrandValidator<ProductTagModel>
    {
        public ProductTagValidator(
            IEnumerable<IValidatorConsumer<ProductTagModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.ProductTags.Fields.Name.Required"));
        }
    }
}