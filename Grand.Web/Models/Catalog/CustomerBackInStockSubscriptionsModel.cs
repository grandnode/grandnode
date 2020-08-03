using Grand.Framework.Mvc.Models;
using Grand.Web.Models.Common;
using System.Collections.Generic;

namespace Grand.Web.Models.Catalog
{
    public partial class CustomerBackInStockSubscriptionsModel
    {
        public CustomerBackInStockSubscriptionsModel()
        {
            Subscriptions = new List<BackInStockSubscriptionModel>();
        }

        public IList<BackInStockSubscriptionModel> Subscriptions { get; set; }
        public PagerModel PagerModel { get; set; }

        #region Nested classes

        public partial class BackInStockSubscriptionModel : BaseGrandEntityModel
        {
            public string ProductId { get; set; }
            public string ProductName { get; set; }
            public string AttributeDescription { get; set; }
            public string SeName { get; set; }
        }

        #endregion
    }
}