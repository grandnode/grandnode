using Grand.Framework.Mvc.Models;
using Grand.Web.Models.Common;

namespace Grand.Web.Models.Customer
{
    public partial class CustomerAddressEditModel : BaseGrandModel
    {
        public CustomerAddressEditModel()
        {
            Address = new AddressModel();
        }
        public AddressModel Address { get; set; }
    }
}