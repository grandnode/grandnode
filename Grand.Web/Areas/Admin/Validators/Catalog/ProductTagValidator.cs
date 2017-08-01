using FluentValidation;
using Grand.Web.Areas.Admin.Models.Catalog;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Catalog
{
    public class ProductTagValidator : BaseGrandValidator<ProductTagModel>
    {
        public ProductTagValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.ProductTags.Fields.Name.Required"));
        }
    }
}