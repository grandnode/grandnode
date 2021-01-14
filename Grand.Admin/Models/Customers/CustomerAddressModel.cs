using Grand.Core.Models;
using Grand.Admin.Models.Common;

namespace Grand.Admin.Models.Customers
{
    public partial class CustomerAddressModel : BaseModel
    {
        public string CustomerId { get; set; }

        public AddressModel Address { get; set; }
    }
}