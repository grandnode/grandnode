using Grand.Core.Models;

namespace Grand.Web.Models.Common
{
    public partial class ShoppingCartLinksModel : BaseModel
    {
        public bool MiniShoppingCartEnabled { get; set; }
        public bool ShoppingCartEnabled { get; set; }
        public int ShoppingCartItems { get; set; }
        public bool WishlistEnabled { get; set; }
        public int WishlistItems { get; set; }
    }
}
