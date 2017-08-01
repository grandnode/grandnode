using System.Collections.Generic;
using Grand.Framework.Mvc;
using Grand.Web.Models.Common;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Models.Catalog
{
    public partial class CustomerBackInStockSubscriptionsModel
    {
        public CustomerBackInStockSubscriptionsModel()
        {
            this.Subscriptions = new List<BackInStockSubscriptionModel>();
        }

        public IList<BackInStockSubscriptionModel> Subscriptions { get; set; }
        public PagerModel PagerModel { get; set; }

        #region Nested classes

        public partial class BackInStockSubscriptionModel : BaseGrandEntityModel
        {
            public string ProductId { get; set; }
            public string ProductName { get; set; }
            public string SeName { get; set; }
        }

        #endregion
    }
}