using FluentValidation;
using Grand.Core.Validators;
using Grand.Services.Localization;
using Grand.Admin.Models.Discounts;
using System.Collections.Generic;

namespace Grand.Admin.Validators.Discounts
{
    public class DiscountValidator : BaseGrandValidator<DiscountModel>
    {
        public DiscountValidator(
            IEnumerable<IValidatorConsumer<DiscountModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
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