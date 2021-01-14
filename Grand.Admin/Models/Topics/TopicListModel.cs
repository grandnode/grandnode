using Grand.Core.ModelBinding;
using Grand.Core.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Grand.Admin.Models.Topics
{
    public partial class TopicListModel : BaseModel
    {
        public TopicListModel()
        {
            AvailableStores = new List<SelectListItem>();
        }
        [GrandResourceDisplayName("Admin.ContentManagement.Topics.List.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.ContentManagement.Topics.List.SearchStore")]
        public string SearchStoreId { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
    }
}