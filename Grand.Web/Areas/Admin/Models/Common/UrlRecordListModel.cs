using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Areas.Admin.Models.Common
{
    public partial class UrlRecordListModel : BaseGrandModel
    {
        [GrandResourceDisplayName("Admin.System.SeNames.Name")]
        
        public string SeName { get; set; }
    }
}