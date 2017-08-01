using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Common
{
    public partial class UrlRecordListModel : BaseGrandModel
    {
        [GrandResourceDisplayName("Admin.System.SeNames.Name")]
        
        public string SeName { get; set; }
    }
}