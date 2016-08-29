﻿using System.Collections.Generic;
using System.Web.Mvc;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Topics
{
    public partial class TopicListModel : BaseNopModel
    {
        public TopicListModel()
        {
            AvailableStores = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.ContentManagement.Topics.List.SearchStore")]
        public string SearchStoreId { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
    }
}