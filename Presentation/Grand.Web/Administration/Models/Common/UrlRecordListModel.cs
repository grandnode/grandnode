using System.Web.Mvc;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Common
{
    public partial class UrlRecordListModel : BaseGrandModel
    {
        [GrandResourceDisplayName("Admin.System.SeNames.Name")]
        [AllowHtml]
        public string SeName { get; set; }
    }
}