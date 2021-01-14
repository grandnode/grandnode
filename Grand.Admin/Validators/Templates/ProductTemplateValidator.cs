using FluentValidation;
using Grand.Core.Validators;
using Grand.Services.Localization;
using Grand.Admin.Models.Templates;
using System.Collections.Generic;

namespace Grand.Admin.Validators.Templates
{
    public class ProductTemplateValidator : BaseGrandValidator<ProductTemplateModel>
    {
        public ProductTemplateValidator(
            IEnumerable<IValidatorConsumer<ProductTemplateModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.System.Templates.Product.Name.Required"));
            RuleFor(x => x.ViewPath).NotEmpty().WithMessage(localizationService.GetResource("Admin.System.Templates.Product.ViewPath.Required"));
        }
    }
}