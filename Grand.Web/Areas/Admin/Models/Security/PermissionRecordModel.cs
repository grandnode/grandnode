using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Security
{
    public partial class PermissionRecordModel : BaseGrandModel
    {
        public string Name { get; set; }
        public string SystemName { get; set; }
    }
}