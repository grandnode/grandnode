using Grand.Core.ModelBinding;
using Grand.Core.Models;
using System;
using System.Collections.Generic;

namespace Grand.Web.Models.Orders
{
    public partial class CustomerRewardPointsModel : BaseModel
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

        public partial class RewardPointsHistoryModel : BaseEntityModel
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