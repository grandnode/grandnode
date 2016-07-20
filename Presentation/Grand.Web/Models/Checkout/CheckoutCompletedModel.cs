using Grand.Web.Framework.Mvc;

namespace Grand.Web.Models.Checkout
{
    public partial class CheckoutCompletedModel : BaseNopModel
    {
        public string OrderId { get; set; }
        public int OrderNumber { get; set; }
        public bool OnePageCheckoutEnabled { get; set; }
    }
}