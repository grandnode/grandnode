using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Areas.Admin.Models.Common
{
    public partial class SearchTermReportLineModel : BaseGrandModel
    {
        [GrandResourceDisplayName("Admin.SearchTermReport.Keyword")]
        public string Keyword { get; set; }

        [GrandResourceDisplayName("Admin.SearchTermReport.Count")]
        public int Count { get; set; }
    }
}
