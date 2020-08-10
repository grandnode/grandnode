using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;

namespace Grand.Plugin.Widgets.FacebookPixel.Models
{
    public class FacebookAddToCartModelModel : BaseGrandModel
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public int ItemQuantity { get; set; }
        public string Price { get; set; }
        public decimal DecimalPrice { get; set; }
        public string TotalPrice { get; set; }


    }
}