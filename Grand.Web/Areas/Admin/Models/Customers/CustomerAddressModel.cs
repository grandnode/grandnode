using FluentValidation.Attributes;
using Grand.Framework.Mvc.Models;
using Grand.Web.Areas.Admin.Models.Common;
using Grand.Web.Areas.Admin.Validators.Customers;

namespace Grand.Web.Areas.Admin.Models.Customers
{
    [Validator(typeof(CustomerAddressValidator))]
    public partial class CustomerAddressModel : BaseGrandModel
    {
        public string CustomerId { get; set; }

        public AddressModel Address { get; set; }
    }
}