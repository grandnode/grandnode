using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using System.Collections.Generic;

namespace Grand.Framework.Mapping
{
    public interface IAclMappingModel
    {
        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.SubjectToAcl")]
        bool SubjectToAcl { get; set; }
        [GrandResourceDisplayName("Admin.Catalog.Categories.Fields.AclCustomerRoles")]
        List<CustomerRoleModel> AvailableCustomerRoles { get; set; }
        string[] SelectedCustomerRoleIds { get; set; }
    }
}
