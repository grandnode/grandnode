using FluentValidation;
using Grand.Admin.Models.Settings;
using Grand.Core.Domain.Orders;
using Grand.Services.Localization;
using Grand.Web.Framework.Validators;

namespace Grand.Admin.Validators.Settings
{
    public class RewardPointsSettingsValidator : BaseNopValidator<RewardPointsSettingsModel>
    {
        public RewardPointsSettingsValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.PointsForPurchases_Awarded).NotEqual((int)OrderStatus.Pending).WithMessage(localizationService.GetResource("Admin.Configuration.Settings.RewardPoints.PointsForPurchases_Awarded.Pending"));
            RuleFor(x => x.PointsForPurchases_Canceled).NotEqual((int)OrderStatus.Pending).WithMessage(localizationService.GetResource("Admin.Configuration.Settings.RewardPoints.PointsForPurchases_Canceled.Pending"));
        }
    }
}