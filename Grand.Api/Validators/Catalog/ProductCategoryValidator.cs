using FluentValidation;
using Grand.Api.DTOs.Catalog;
using Grand.Framework.Validators;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using System.Collections.Generic;

namespace Grand.Api.Validators.Catalog
{
    public class ProductCategoryValidator : BaseGrandValidator<ProductCategoryDto>
    {
        public ProductCategoryValidator(IEnumerable<IValidatorConsumer<ProductCategoryDto>> validators, ILocalizationService localizationService, ICategoryService categoryService)
            : base(validators)
        {
            RuleFor(x => x).MustAsync(async (x, y, context) =>
            {
                var category = await categoryService.GetCategoryById(x.CategoryId);
                if (category == null)
                    return false;
                return true;
            }).WithMessage(localizationService.GetResource("Api.Catalog.ProductCategory.Fields.CategoryId.NotExists"));
        }
    }
}
