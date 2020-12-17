using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Web.Areas.Admin.Models.Customers
{
    public partial class SalesEmployeeModel : BaseEntityModel
    {
        [GrandResourceDisplayName("Admin.Customers.SalesEmployees.Fields.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Customers.SalesEmployees.Fields.Email")]
        public string Email { get; set; }

        [GrandResourceDisplayName("Admin.Customers.SalesEmployees.Fields.Phone")]
        public string Phone { get; set; }

        [GrandResourceDisplayName("Admin.Customers.SalesEmployees.Fields.Active")]
        public bool Active { get; set; }

        [GrandResourceDisplayName("Admin.Customers.SalesEmployees.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }
    }
}