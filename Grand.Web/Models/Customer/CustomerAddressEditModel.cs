using Grand.Core.Models;
using Grand.Web.Models.Common;

namespace Grand.Web.Models.Customer
{
    public partial class CustomerAddressEditModel : BaseModel
    {
        public CustomerAddressEditModel()
        {
            Address = new AddressModel();
        }
        public AddressModel Address { get; set; }
    }
}