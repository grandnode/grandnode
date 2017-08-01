using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Routing;

namespace Grand.Web.Models.Checkout
{
    public partial class CheckoutPaymentInfoModel : BaseGrandModel
    {
        public string PaymentViewComponentName { get; set; }

        /// <summary>
        /// Used on one-page checkout page
        /// </summary>
        public bool DisplayOrderTotals { get; set; }
    }
}