using FluentValidation;
using Grand.Web.Areas.Admin.Models.Discounts;
using Grand.Services.Localization;
using Grand.Framework.Validators;

namespace Grand.Web.Areas.Admin.Validators.Discounts
{
    public class DiscountValidator : BaseGrandValidator<DiscountModel>
    {
        public DiscountValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Promotions.Discounts.Fields.Name.Required"));
        }
    }
}