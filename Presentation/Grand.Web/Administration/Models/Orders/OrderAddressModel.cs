using Grand.Admin.Models.Common;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Orders
{
    public partial class OrderAddressModel : BaseGrandModel
    {
        public string OrderId { get; set; }
        public AddressModel Address { get; set; }
    }
}