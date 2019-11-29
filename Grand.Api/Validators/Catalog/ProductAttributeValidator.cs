using FluentValidation;
using Grand.Api.DTOs.Catalog;
using Grand.Framework.Validators;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using System.Collections.Generic;

namespace Grand.Api.Validators.Catalog
{
    public class ProductAttributeValidator : BaseGrandValidator<ProductAttributeDto>
    {
        public ProductAttributeValidator(IEnumerable<IValidatorConsumer<ProductAttributeDto>> validators,
            ILocalizationService localizationService, IProductAttributeService productAttributeService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Api.Catalog.ProductAttribute.Fields.Name.Required"));
            RuleFor(x => x).MustAsync(async (x, y, context) =>
            {
                if (!string.IsNullOrEmpty(x.Id))
                {
                    var pa = await productAttributeService.GetProductAttributeById(x.Id);
                    if (pa == null)
                        return false;
                }
                return true;
            }).WithMessage(localizationService.GetResource("Api.Catalog.ProductAttribute.Fields.Id.NotExists"));
            RuleFor(x => x).Must((x, context) =>
            {
                foreach (var item in x.PredefinedProductAttributeValues)
                {
                    if (string.IsNullOrEmpty(item.Name))
                        return false;
                }
                return true;
            }).WithMessage(localizationService.GetResource("Api.Catalog.PredefinedProductAttributeValue.Fields.Name.Required"));
        }
    }
}
