using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Messages
{
    public partial class MessageTemplateListModel : BaseGrandModel
    {
        public MessageTemplateListModel()
        {
            AvailableStores = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Admin.ContentManagement.MessageTemplates.List.SearchStore")]
        public string SearchStoreId { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
    }
}