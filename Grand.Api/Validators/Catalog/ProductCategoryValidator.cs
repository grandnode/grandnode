using FluentValidation;
using Grand.Api.DTOs.Catalog;
using Grand.Framework.Validators;
using Grand.Services.Catalog;
using Grand.Services.Localization;

namespace Grand.Api.Validators.Catalog
{
    public class ProductCategoryValidator : BaseGrandValidator<ProductCategoryDto>
    {
        public ProductCategoryValidator(ILocalizationService localizationService, ICategoryService categoryService)
        {
            RuleFor(x => x).Must((x, context) =>
            {
                var category = categoryService.GetCategoryById(x.CategoryId);
                if (category == null)
                    return false;
                return true;
            }).WithMessage(localizationService.GetResource("Api.Catalog.ProductCategory.Fields.CategoryId.NotExists"));
        }
    }
}
