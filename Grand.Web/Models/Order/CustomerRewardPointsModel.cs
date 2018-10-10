using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using System;
using System.Collections.Generic;

namespace Grand.Web.Models.Order
{
    public partial class CustomerRewardPointsModel : BaseGrandModel
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

        public partial class RewardPointsHistoryModel : BaseGrandEntityModel
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