using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Admin.Models.Customers
{
    public partial class CustomerTagModel : BaseEntityModel
    {
        [GrandResourceDisplayName("Admin.Customers.CustomerTags.Fields.Name")]

        public string Name { get; set; }
    }
}