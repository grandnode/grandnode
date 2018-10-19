using FluentValidation;
using Grand.Core.Domain.Orders;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Settings;

namespace Grand.Web.Areas.Admin.Validators.Settings
{
    public class OrderSettingsValidator : BaseGrandValidator<OrderSettingsModel>
    {
        public OrderSettingsValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.GiftCards_Activated_OrderStatusId).NotEqual((int)OrderStatus.Pending)
                .WithMessage(localizationService.GetResource("Admin.Configuration.Settings.RewardPoints.PointsForPurchases_Awarded.Pending"));
            RuleFor(x => x.GiftCards_Deactivated_OrderStatusId).NotEqual((int)OrderStatus.Pending)
                .WithMessage(localizationService.GetResource("Admin.Configuration.Settings.RewardPoints.PointsForPurchases_Canceled.Pending"));
        }
    }
}