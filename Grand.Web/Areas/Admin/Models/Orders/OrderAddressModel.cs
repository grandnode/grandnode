using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Web.Areas.Admin.Models.Common;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Orders
{
    public partial class OrderAddressModel : BaseGrandModel
    {
        public string OrderId { get; set; }
        public AddressModel Address { get; set; }
    }
}