using Grand.Framework.Mvc.Models;
using Grand.Web.Areas.Admin.Models.Common;

namespace Grand.Web.Areas.Admin.Models.Customers
{
    public partial class CustomerAddressModel : BaseGrandModel
    {
        public string CustomerId { get; set; }

        public AddressModel Address { get; set; }
    }
}