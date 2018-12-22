using FluentValidation;
using Grand.Api.DTOs.Catalog;
using Grand.Framework.Validators;
using Grand.Services.Catalog;
using Grand.Services.Localization;

namespace Grand.Api.Validators.Catalog
{
    public class ProductAttributeValidator : BaseGrandValidator<ProductAttributeDto>
    {
        public ProductAttributeValidator(ILocalizationService localizationService, IProductAttributeService productAttributeService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Api.Catalog.ProductAttribute.Fields.Name.Required"));
            RuleFor(x => x).Must((x, context) =>
            {
                if (!string.IsNullOrEmpty(x.Id))
                {
                    var pa = productAttributeService.GetProductAttributeById(x.Id);
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
