using FluentValidation;
using Grand.Api.DTOs.Catalog;
using Grand.Framework.Validators;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using System.Collections.Generic;

namespace Grand.Api.Validators.Catalog
{
    public class ProductAttributeMappingValidator : BaseGrandValidator<ProductAttributeMappingDto>
    {
        public ProductAttributeMappingValidator(
            IEnumerable<IValidatorConsumer<ProductAttributeMappingDto>> validators,
            ILocalizationService localizationService, IProductAttributeService productAttributeService)
            : base(validators)
        {
            RuleFor(x => x).MustAsync(async (x, y, context) =>
            {
                var productattribute = await productAttributeService.GetProductAttributeById(x.ProductAttributeId);
                if (productattribute == null)
                    return false;
                return true;
            }).WithMessage(localizationService.GetResource("Api.Catalog.ProductAttributeMapping.Fields.ProductAttributeId.NotExists"));
        }
    }
}
