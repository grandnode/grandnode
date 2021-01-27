using Grand.Core.ModelBinding;
using Grand.Core.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Models.Common
{
    public partial class UrlRecordListModel : BaseModel
    {
        public UrlRecordListModel()
        {
            AvailableActiveOptions = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Admin.System.SeNames.Name")]
        public string SeName { get; set; }

        [GrandResourceDisplayName("Admin.System.SeNames.Active")]
        public int SearchActiveId { get; set; }
        public IList<SelectListItem> AvailableActiveOptions { get; set; }
    }
}