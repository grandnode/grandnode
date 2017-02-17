using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Common
{
    public partial class SearchTermReportLineModel : BaseNopModel
    {
        [GrandResourceDisplayName("Admin.SearchTermReport.Keyword")]
        public string Keyword { get; set; }

        [GrandResourceDisplayName("Admin.SearchTermReport.Count")]
        public int Count { get; set; }
    }
}
