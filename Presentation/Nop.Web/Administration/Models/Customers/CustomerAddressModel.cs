using Nop.Admin.Models.Common;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Customers
{
    public partial class CustomerAddressModel : BaseNopModel
    {
        public string CustomerId { get; set; }

        public AddressModel Address { get; set; }
    }
}