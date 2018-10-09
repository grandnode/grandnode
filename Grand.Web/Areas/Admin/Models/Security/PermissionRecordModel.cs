using Grand.Framework.Mvc.Models;

namespace Grand.Web.Areas.Admin.Models.Security
{
    public partial class PermissionRecordModel : BaseGrandModel
    {
        public string Name { get; set; }
        public string SystemName { get; set; }
    }
}