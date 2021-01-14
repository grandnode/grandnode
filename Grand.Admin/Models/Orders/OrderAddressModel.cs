using Grand.Core.Models;
using Grand.Admin.Models.Common;

namespace Grand.Admin.Models.Orders
{
    public partial class OrderAddressModel : BaseModel
    {
        public string OrderId { get; set; }
        public bool BillingAddress { get; set; }
        public AddressModel Address { get; set; }
    }
}