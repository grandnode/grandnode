using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Admin.Models.Common
{
    public partial class SearchTermReportLineModel : BaseModel
    {
        [GrandResourceDisplayName("Admin.SearchTermReport.Keyword")]
        public string Keyword { get; set; }

        [GrandResourceDisplayName("Admin.SearchTermReport.Count")]
        public int Count { get; set; }
    }
}
