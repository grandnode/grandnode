using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Customers
{
    public partial class CustomerActionTypeModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.Customer.ActionType.Fields.Name")]
        public string Name { get; set; }
        [GrandResourceDisplayName("Admin.Customer.ActionType.Fields.Enabled")]
        public bool Enabled { get; set; }
    }
}