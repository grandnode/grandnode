using FluentValidation;
using Grand.Api.DTOs.Catalog;
using Grand.Framework.Validators;
using Grand.Services.Catalog;
using Grand.Services.Localization;

namespace Grand.Api.Validators.Catalog
{
    public class ProductSpecificationAttributeValidator : BaseGrandValidator<ProductSpecificationAttributeDto>
    {
        public ProductSpecificationAttributeValidator(ILocalizationService localizationService, ISpecificationAttributeService specificationAttributeService)
        {
            RuleFor(x => x).MustAsync(async (x, y, context) =>
            {
                var specification = await specificationAttributeService.GetSpecificationAttributeById(x.SpecificationAttributeId);
                if (specification == null)
                    return false;
                return true;
            }).WithMessage(localizationService.GetResource("Api.Catalog.ProductSpecificationAttribute.Fields.SpecificationAttributeId.NotExists"));

            RuleFor(x => x).MustAsync(async (x, y, context) =>
            {
                if (!string.IsNullOrEmpty(x.SpecificationAttributeOptionId))
                {
                    var sa = await specificationAttributeService.GetSpecificationAttributeByOptionId(x.SpecificationAttributeOptionId);
                    if (sa == null)
                        return false;
                }
                return true;
            }).WithMessage(localizationService.GetResource("Api.Catalog.ProductSpecificationAttribute.Fields.SpecificationAttributeOptionId.NotExists"));
        }
    }
}
