using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.ShoppingCart
{
    public partial class ShoppingCartModel : BaseGrandModel
    {
        [GrandResourceDisplayName("Admin.CurrentCarts.Customer")]
        public string CustomerId { get; set; }
        [GrandResourceDisplayName("Admin.CurrentCarts.Customer")]
        public string CustomerEmail { get; set; }

        [GrandResourceDisplayName("Admin.CurrentCarts.TotalItems")]
        public int TotalItems { get; set; }
    }
}