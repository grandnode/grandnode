using System.Web.Mvc;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Common
{
    public partial class UrlRecordModel : BaseNopEntityModel
    {
        [GrandResourceDisplayName("Admin.System.SeNames.Name")]
        [AllowHtml]
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