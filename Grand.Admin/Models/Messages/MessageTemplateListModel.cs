using Grand.Core.ModelBinding;
using Grand.Core.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Grand.Admin.Models.Messages
{
    public partial class MessageTemplateListModel : BaseModel
    {
        public MessageTemplateListModel()
        {
            AvailableStores = new List<SelectListItem>();
        }
        [GrandResourceDisplayName("Admin.ContentManagement.MessageTemplates.List.Name")]
        public string Name { get; set; }
        [GrandResourceDisplayName("Admin.ContentManagement.MessageTemplates.List.SearchStore")]
        public string SearchStoreId { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
    }
}