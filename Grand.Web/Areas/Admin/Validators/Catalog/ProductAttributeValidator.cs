using FluentValidation;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Catalog
{
    public class ProductAttributeValidator : BaseGrandValidator<ProductAttributeModel>
    {
        public ProductAttributeValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Attributes.ProductAttributes.Fields.Name.Required"));
        }
    }
}