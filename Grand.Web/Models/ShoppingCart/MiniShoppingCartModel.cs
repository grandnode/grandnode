using System.Collections.Generic;
using Grand.Framework.Mvc.Models;
using Grand.Web.Models.Media;

namespace Grand.Web.Models.ShoppingCart
{
    public partial class MiniShoppingCartModel : BaseGrandModel
    {
        public MiniShoppingCartModel()
        {
            Items = new List<ShoppingCartItemModel>();
        }

        public IList<ShoppingCartItemModel> Items { get; set; }
        public int TotalProducts { get; set; }
        public string SubTotal { get; set; }
        public bool DisplayShoppingCartButton { get; set; }
        public bool DisplayCheckoutButton { get; set; }
        public bool CurrentCustomerIsGuest { get; set; }
        public bool AnonymousCheckoutAllowed { get; set; }
        public bool ShowProductImages { get; set; }


        #region Nested Classes

        public partial class ShoppingCartItemModel : BaseGrandEntityModel
        {
            public ShoppingCartItemModel()
            {
                Picture = new PictureModel();
            }

            public string ProductId { get; set; }

            public string ProductName { get; set; }

            public string ProductSeName { get; set; }

            public int Quantity { get; set; }

            public string UnitPrice { get; set; }

            public string AttributeInfo { get; set; }

            public PictureModel Picture { get; set; }
        }

        #endregion
    }
}