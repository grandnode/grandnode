using Grand.Core.Models;
using Grand.Web.Areas.Admin.Models.Common;

namespace Grand.Web.Areas.Admin.Models.Customers
{
    public partial class CustomerAddressModel : BaseModel
    {
        public string CustomerId { get; set; }

        public AddressModel Address { get; set; }
    }
}