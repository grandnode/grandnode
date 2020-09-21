using Grand.Core.Models;

namespace Grand.Web.Models.Checkout
{
    public partial class CheckoutProgressModel : BaseModel
    {
        public CheckoutProgressStep CheckoutProgressStep { get; set; }
    }

    public enum CheckoutProgressStep
    {
        Cart,
        Address,
        Shipping,
        Payment,
        Confirm,
        Complete
    }
}