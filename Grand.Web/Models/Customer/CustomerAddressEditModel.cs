using FluentValidation.Attributes;
using Grand.Framework.Mvc.Models;
using Grand.Web.Models.Common;
using Grand.Web.Validators.Customer;

namespace Grand.Web.Models.Customer
{
    [Validator(typeof(CustomerAddressEditValidator))]
    public partial class CustomerAddressEditModel : BaseGrandModel
    {
        public CustomerAddressEditModel()
        {
            this.Address = new AddressModel();
        }
        public AddressModel Address { get; set; }
    }
}