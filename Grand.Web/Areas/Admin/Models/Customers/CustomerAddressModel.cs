using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Web.Areas.Admin.Models.Common;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Customers
{
    public partial class CustomerAddressModel : BaseGrandModel
    {
        public string CustomerId { get; set; }

        public AddressModel Address { get; set; }
    }
}