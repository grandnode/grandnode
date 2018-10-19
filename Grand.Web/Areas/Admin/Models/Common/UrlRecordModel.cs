using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Areas.Admin.Models.Common
{
    public partial class UrlRecordModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.System.SeNames.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.System.SeNames.EntityId")]
        public string EntityId { get; set; }

        [GrandResourceDisplayName("Admin.System.SeNames.EntityName")]
        public string EntityName { get; set; }

        [GrandResourceDisplayName("Admin.System.SeNames.IsActive")]
        public bool IsActive { get; set; }

        [GrandResourceDisplayName("Admin.System.SeNames.Language")]
        public string Language { get; set; }

        [GrandResourceDisplayName("Admin.System.SeNames.Details")]
        public string DetailsUrl { get; set; }
    }
}