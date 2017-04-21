using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Security
{
    public partial class PermissionRecordModel : BaseGrandModel
    {
        public string Name { get; set; }
        public string SystemName { get; set; }
    }
}