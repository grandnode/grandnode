using System.Collections.Generic;
using System.Web.Mvc;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Messages
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