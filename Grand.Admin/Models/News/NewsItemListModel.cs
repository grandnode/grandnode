using Grand.Core.ModelBinding;
using Grand.Core.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Grand.Admin.Models.News
{
    public partial class NewsItemListModel : BaseModel
    {
        public NewsItemListModel()
        {
            AvailableStores = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Admin.ContentManagement.News.NewsItems.List.SearchStore")]
        public string SearchStoreId { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
    }
}