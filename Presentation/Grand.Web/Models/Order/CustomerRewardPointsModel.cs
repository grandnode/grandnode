using System;
using System.Collections.Generic;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Web.Models.Order
{
    public partial class CustomerRewardPointsModel : BaseNopModel
    {
        public CustomerRewardPointsModel()
        {
            RewardPoints = new List<RewardPointsHistoryModel>();
        }

        public IList<RewardPointsHistoryModel> RewardPoints { get; set; }
        public int RewardPointsBalance { get; set; }
        public string RewardPointsAmount { get; set; }
        public int MinimumRewardPointsBalance { get; set; }
        public string MinimumRewardPointsAmount { get; set; }

        #region Nested classes

        public partial class RewardPointsHistoryModel : BaseNopEntityModel
        {
            [GrandResourceDisplayName("RewardPoints.Fields.Points")]
            public int Points { get; set; }

            [GrandResourceDisplayName("RewardPoints.Fields.PointsBalance")]
            public int PointsBalance { get; set; }

            [GrandResourceDisplayName("RewardPoints.Fields.Message")]
            public string Message { get; set; }

            [GrandResourceDisplayName("RewardPoints.Fields.Date")]
            public DateTime CreatedOn { get; set; }
        }

        #endregion
    }
}