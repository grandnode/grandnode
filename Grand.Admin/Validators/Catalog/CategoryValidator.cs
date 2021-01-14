using FluentValidation;
using Grand.Core.Extensions;
using Grand.Core.Validators;
using Grand.Services.Localization;
using Grand.Admin.Models.Catalog;
using System.Collections.Generic;

namespace Grand.Admin.Validators.Catalog
{
    public class CategoryValidator : BaseGrandValidator<CategoryModel>
    {
        public CategoryValidator(
            IEnumerable<IValidatorConsumer<CategoryModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Categories.Fields.Name.Required"));
            RuleFor(x => x.PageSizeOptions).Must(FluentValidationUtilities.PageSizeOptionsValidator).WithMessage(localizationService.GetResource("Admin.Catalog.Categories.Fields.PageSizeOptions.ShouldHaveUniqueItems"));
        }
    }
}