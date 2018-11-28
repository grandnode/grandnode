
using Grand.Framework.Mvc.ModelBinding;

namespace Grand.Web.Areas.Admin.Models.Common
{
    public partial class QueryEditor
    {
        [GrandResourceDisplayName("Admin.System.Field.QueryEditor")]
        public string Query { get; set; }
    }
}
