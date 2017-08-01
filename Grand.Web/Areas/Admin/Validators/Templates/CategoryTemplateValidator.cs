using FluentValidation;
using Grand.Web.Areas.Admin.Models.Templates;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Templates
{
    public class CategoryTemplateValidator : BaseGrandValidator<CategoryTemplateModel>
    {
        public CategoryTemplateValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.System.Templates.Category.Name.Required"));
            RuleFor(x => x.ViewPath).NotEmpty().WithMessage(localizationService.GetResource("Admin.System.Templates.Category.ViewPath.Required"));
        }
    }
}