using FluentValidation.Attributes;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Grand.Web.Areas.Admin.Validators.Customers;

namespace Grand.Web.Areas.Admin.Models.Customers
{
    [Validator(typeof(CustomerTagValidator))]
    public partial class CustomerTagModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.Customers.CustomerTags.Fields.Name")]
        
        public string Name { get; set; }
    }
}