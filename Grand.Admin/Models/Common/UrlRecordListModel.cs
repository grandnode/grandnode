using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Admin.Models.Common
{
    public partial class UrlRecordListModel : BaseModel
    {
        [GrandResourceDisplayName("Admin.System.SeNames.Name")]
        
        public string SeName { get; set; }
    }
}