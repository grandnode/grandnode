using Grand.Core.Models;
using System.Collections.Generic;

namespace Grand.Admin.Models.Security
{
    public partial class PermissionActionModel : BaseModel
    {
        public PermissionActionModel()
        {
            AvailableActions = new List<string>();
            DeniedActions = new List<string>();
        }
        public string SystemName { get; set; }
        public string PermissionName { get; set; }
        public string CustomerRoleId { get; set; }
        public string CustomerRoleName { get; set; }
        public IList<string> AvailableActions { get; set; }
        public IList<string> DeniedActions { get; set; }
    }
}