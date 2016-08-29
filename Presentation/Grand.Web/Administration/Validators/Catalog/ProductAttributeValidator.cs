using FluentValidation;
using Grand.Admin.Models.Catalog;
using Grand.Services.Localization;
using Grand.Web.Framework.Validators;

namespace Grand.Admin.Validators.Catalog
{
    public class ProductAttributeValidator : BaseNopValidator<ProductAttributeModel>
    {
        public ProductAttributeValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Attributes.ProductAttributes.Fields.Name.Required"));
        }
    }
}