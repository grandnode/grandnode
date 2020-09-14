﻿using Grand.Core.Models;
using Grand.Web.Models.Common;
using System.Collections.Generic;

namespace Grand.Web.Models.Boards
{
    public partial class CustomerForumSubscriptionsModel
    {
        public CustomerForumSubscriptionsModel()
        {
            ForumSubscriptions = new List<ForumSubscriptionModel>();
        }

        public IList<ForumSubscriptionModel> ForumSubscriptions { get; set; }
        public PagerModel PagerModel { get; set; }

        #region Nested classes

        public partial class ForumSubscriptionModel : BaseEntityModel
        {
            public string ForumId { get; set; }
            public string ForumTopicId { get; set; }
            public bool TopicSubscription { get; set; }
            public string Title { get; set; }
            public string Slug { get; set; }
        }

        #endregion
    }
}