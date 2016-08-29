using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Common
{
    public partial class SearchTermReportLineModel : BaseNopModel
    {
        [NopResourceDisplayName("Admin.SearchTermReport.Keyword")]
        public string Keyword { get; set; }

        [NopResourceDisplayName("Admin.SearchTermReport.Count")]
        public int Count { get; set; }
    }
}
