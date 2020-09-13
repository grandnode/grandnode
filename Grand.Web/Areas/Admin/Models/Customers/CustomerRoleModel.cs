using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Areas.Admin.Models.Customers
{
    public partial class CustomerRoleModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.Customers.CustomerRoles.Fields.Name")]

        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Customers.CustomerRoles.Fields.FreeShipping")]

        public bool FreeShipping { get; set; }

        [GrandResourceDisplayName("Admin.Customers.CustomerRoles.Fields.TaxExempt")]
        public bool TaxExempt { get; set; }

        [GrandResourceDisplayName("Admin.Customers.CustomerRoles.Fields.Active")]
        public bool Active { get; set; }

        [GrandResourceDisplayName("Admin.Customers.CustomerRoles.Fields.IsSystemRole")]
        public bool IsSystemRole { get; set; }

        [GrandResourceDisplayName("Admin.Customers.CustomerRoles.Fields.SystemName")]
        public string SystemName { get; set; }

        [GrandResourceDisplayName("Admin.Customers.CustomerRoles.Fields.EnablePasswordLifetime")]
        public bool EnablePasswordLifetime { get; set; }

        [GrandResourceDisplayName("Admin.Customers.CustomerRoles.Fields.MinOrderAmount")]
        [UIHint("DecimalNullable")]
        public decimal? MinOrderAmount { get; set; }

        [GrandResourceDisplayName("Admin.Customers.CustomerRoles.Fields.MaxOrderAmount")]
        [UIHint("DecimalNullable")]
        public decimal? MaxOrderAmount { get; set; }

    }
}