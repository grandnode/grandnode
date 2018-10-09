using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Discounts;

namespace Grand.Web.Areas.Admin.Validators.Discounts
{
    public class DiscountValidator : BaseGrandValidator<DiscountModel>
    {
        public DiscountValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Promotions.Discounts.Fields.Name.Required"));
            RuleFor(x => x).Must((x, context) =>
            {
                if (x.CalculateByPlugin && string.IsNullOrEmpty(x.DiscountPluginName))
                {
                    return false;
                }
                return true;
            }).WithMessage(localizationService.GetResource("Admin.Promotions.Discounts.Fields.DiscountPluginName.Required"));
        }
    }
}