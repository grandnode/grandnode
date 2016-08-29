﻿using Grand.Web.Framework.Mvc;

namespace Grand.Web.Models.Catalog
{
    public partial class BackInStockSubscribeModel : BaseNopModel
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSeName { get; set; }

        public bool IsCurrentCustomerRegistered { get; set; }
        public bool SubscriptionAllowed { get; set; }
        public bool AlreadySubscribed { get; set; }

        public int MaximumBackInStockSubscriptions { get; set; }
        public int CurrentNumberOfBackInStockSubscriptions { get; set; }
    }
}