using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Customers
{
    public partial class CustomerActionTypeModel : BaseNopEntityModel
    {
        [GrandResourceDisplayName("Admin.Customer.ActionType.Fields.Name")]
        public string Name { get; set; }
        [GrandResourceDisplayName("Admin.Customer.ActionType.Fields.Enabled")]
        public bool Enabled { get; set; }
    }
}