using FluentValidation;
using Grand.Domain.Orders;
using Grand.Framework.Validators;
using Grand.Services.Localization;
using Grand.Web.Areas.Admin.Models.Settings;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Validators.Settings
{
    public class RewardPointsSettingsValidator : BaseGrandValidator<RewardPointsSettingsModel>
    {
        public RewardPointsSettingsValidator(
            IEnumerable<IValidatorConsumer<RewardPointsSettingsModel>> validators,
            ILocalizationService localizationService)
            : base(validators)
        {
            RuleFor(x => x.PointsForPurchases_Awarded).NotEqual((int)OrderStatus.Pending).WithMessage(localizationService.GetResource("Admin.Configuration.Settings.RewardPoints.PointsForPurchases_Awarded.Pending"));
        }
    }
}