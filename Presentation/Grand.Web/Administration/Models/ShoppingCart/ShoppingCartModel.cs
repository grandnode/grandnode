using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.ShoppingCart
{
    public partial class ShoppingCartModel : BaseNopModel
    {
        [GrandResourceDisplayName("Admin.CurrentCarts.Customer")]
        public string CustomerId { get; set; }
        [GrandResourceDisplayName("Admin.CurrentCarts.Customer")]
        public string CustomerEmail { get; set; }

        [GrandResourceDisplayName("Admin.CurrentCarts.TotalItems")]
        public int TotalItems { get; set; }
    }
}