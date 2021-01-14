using FluentValidation;
using Grand.Core.Validators;
using Grand.Services.Localization;
using Grand.Services.Orders;
using Grand.Admin.Models.Orders;
using System.Collections.Generic;

namespace Grand.Admin.Validators.Orders
{
    public class GiftCardValidator : BaseGrandValidator<GiftCardModel>
    {
        public GiftCardValidator(
            IEnumerable<IValidatorConsumer<GiftCardModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.GiftCardCouponCode).NotEmpty().WithMessage(localizationService.GetResource("Admin.GiftCards.Fields.GiftCardCouponCode.Required"));
        }
    }
}