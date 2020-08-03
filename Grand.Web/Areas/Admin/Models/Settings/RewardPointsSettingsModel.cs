using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Models.Settings
{
    public partial class RewardPointsSettingsModel : BaseGrandModel
    {
        public RewardPointsSettingsModel()
        {
            PointsForPurchases_Awarded_OrderStatuses = new List<SelectListItem>();
        }
        public string ActiveStoreScopeConfiguration { get; set; }


        [GrandResourceDisplayName("Admin.Configuration.Settings.RewardPoints.Enabled")]
        public bool Enabled { get; set; }
        public bool Enabled_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.RewardPoints.ExchangeRate")]
        public decimal ExchangeRate { get; set; }
        public bool ExchangeRate_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.RewardPoints.MinimumRewardPointsToUse")]
        public int MinimumRewardPointsToUse { get; set; }
        public bool MinimumRewardPointsToUse_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.RewardPoints.PointsForRegistration")]
        public int PointsForRegistration { get; set; }
        public bool PointsForRegistration_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.RewardPoints.PointsForPurchases_Amount")]
        public decimal PointsForPurchases_Amount { get; set; }
        public int PointsForPurchases_Points { get; set; }
        public bool PointsForPurchases_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.RewardPoints.PointsForPurchases_Awarded")]
        public int PointsForPurchases_Awarded { get; set; }
        public bool PointsForPurchases_Awarded_OverrideForStore { get; set; }
        public IList<SelectListItem> PointsForPurchases_Awarded_OrderStatuses { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.RewardPoints.ReduceRewardPointsAfterCancelOrder")]
        public bool ReduceRewardPointsAfterCancelOrder { get; set; }
        public bool ReduceRewardPointsAfterCancelOrder_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.RewardPoints.DisplayHowMuchWillBeEarned")]
        public bool DisplayHowMuchWillBeEarned { get; set; }
        public bool DisplayHowMuchWillBeEarned_OverrideForStore { get; set; }

        public string PrimaryStoreCurrencyCode { get; set; }
        [GrandResourceDisplayName("Admin.Configuration.Settings.RewardPoints.PointsAccumulatedForAllStores")]
        public bool PointsAccumulatedForAllStores { get; set; }


    }
}