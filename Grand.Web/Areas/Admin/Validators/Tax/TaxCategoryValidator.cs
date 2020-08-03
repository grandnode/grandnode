using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Tax;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Tax
{
    public class TaxCategoryValidator : BaseGrandValidator<TaxCategoryModel>
    {
        public TaxCategoryValidator(
            IEnumerable<IValidatorConsumer<TaxCategoryModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Configuration.Tax.Categories.Fields.Name.Required"));
        }
    }
}