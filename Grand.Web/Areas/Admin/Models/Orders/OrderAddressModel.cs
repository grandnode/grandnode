using Grand.Framework.Mvc.Models;
using Grand.Web.Areas.Admin.Models.Common;

namespace Grand.Web.Areas.Admin.Models.Orders
{
    public partial class OrderAddressModel : BaseGrandModel
    {
        public string OrderId { get; set; }
        public AddressModel Address { get; set; }
    }
}