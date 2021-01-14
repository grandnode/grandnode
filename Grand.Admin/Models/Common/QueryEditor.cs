
using Grand.Core.ModelBinding;

namespace Grand.Admin.Models.Common
{
    public partial class QueryEditor
    {
        [GrandResourceDisplayName("Admin.System.Field.QueryEditor")]
        public string Query { get; set; }
    }
}
