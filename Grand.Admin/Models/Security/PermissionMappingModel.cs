using Grand.Core.Models;
using Grand.Framework.Mvc.Models;
using System.Collections.Generic;

namespace Grand.Admin.Models.Security
{
    public partial class PermissionMappingModel : BaseModel
    {
        public PermissionMappingModel()
        {
            AvailablePermissions = new List<PermissionRecordModel>();
            AvailableCustomerRoles = new List<CustomerRoleModel>();
            Allowed = new Dictionary<string, IDictionary<string, bool>>();
        }
        public IList<PermissionRecordModel> AvailablePermissions { get; set; }
        public IList<CustomerRoleModel> AvailableCustomerRoles { get; set; }

        //[permission system name] / [customer role id] / [allowed]
        public IDictionary<string, IDictionary<string, bool>> Allowed { get; set; }
    }
}