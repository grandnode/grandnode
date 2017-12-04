using Grand.Core.Domain.Orders;
using Grand.Framework.Mvc.Models;
using Grand.Web.Models.Media;

namespace Grand.Web.Models.ShoppingCart
{
    public partial class AddToCartModel : BaseGrandModel
    {
        public AddToCartModel()
        {
            Picture = new PictureModel();
        }

        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSeName { get; set; }
        public string AttributeDescription { get; set; }
        public PictureModel Picture { get; set; }
        public int Quantity { get; set; }
        public string Price { get; set; }
        public decimal DecimalPrice { get; set; }
        public string TotalPrice { get; set; }
        public ShoppingCartType CartType { get; set; }
    }
}