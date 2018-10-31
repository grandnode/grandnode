using FluentValidation;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Services.Orders;
using Grand.Web.Areas.Admin.Models.Orders;

namespace Grand.Web.Areas.Admin.Validators.Orders
{
    public class GiftCardValidator : BaseGrandValidator<GiftCardModel>
    {
        public GiftCardValidator(ILocalizationService localizationService, IGiftCardService giftCardService)
        {
            RuleFor(x => x.GiftCardCouponCode).NotEmpty().WithMessage(localizationService.GetResource("Admin.GiftCards.Fields.GiftCardCouponCode.Required"));
        }
    }
}