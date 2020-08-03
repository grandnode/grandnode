using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Catalog;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Catalog
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