using FluentValidation;
using Grand.Web.Areas.Admin.Models.Tax;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Tax
{
    public class TaxCategoryValidator : BaseGrandValidator<TaxCategoryModel>
    {
        public TaxCategoryValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Tax.Categories.Fields.Name.Required"));
        }
    }
}