using Grand.Core.ModelBinding;
using Grand.Core.Models;
using System.Collections.Generic;

namespace Grand.Admin.Models.Customers
{
    public class CustomerRolePermissionModel : BaseEntityModel
    {
        public CustomerRolePermissionModel()
        {
            Actions = new List<string>();
        }
        [GrandResourceDisplayName("Admin.Customers.CustomerRoles.Acl.Fields.Name")]
        public string Name { get; set; }

        public string SystemName { get; set; }

        [GrandResourceDisplayName("Admin.Customers.CustomerRoles.Acl.Fields.Access")]
        public bool Access { get; set; }

        [GrandResourceDisplayName("Admin.Customers.CustomerRoles.Acl.Fields.Actions")]
        public IList<string> Actions { get; set; }
    }
}
