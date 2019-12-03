using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Templates;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Templates
{
    public class CategoryTemplateValidator : BaseGrandValidator<CategoryTemplateModel>
    {
        public CategoryTemplateValidator(
            IEnumerable<IValidatorConsumer<CategoryTemplateModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.System.Templates.Category.Name.Required"));
            RuleFor(x => x.ViewPath).NotEmpty().WithMessage(localizationService.GetResource("Admin.System.Templates.Category.ViewPath.Required"));
        }
    }
}