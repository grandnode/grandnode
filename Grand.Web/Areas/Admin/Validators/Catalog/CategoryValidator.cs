using FluentValidation;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Catalog
{
    public class CategoryValidator : BaseGrandValidator<CategoryModel>
    {
        public CategoryValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Categories.Fields.Name.Required"));
            RuleFor(x => x.PageSizeOptions).Must(ValidatorUtilities.PageSizeOptionsValidator).WithMessage(localizationService.GetResource("Admin.Catalog.Categories.Fields.PageSizeOptions.ShouldHaveUniqueItems"));
        }
    }
}