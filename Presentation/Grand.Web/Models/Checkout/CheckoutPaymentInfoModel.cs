using System.Web.Routing;
using Grand.Web.Framework.Mvc;

namespace Grand.Web.Models.Checkout
{
    public partial class CheckoutPaymentInfoModel : BaseGrandModel
    {
        public string PaymentInfoActionName { get; set; }
        public string PaymentInfoControllerName { get; set; }
        public RouteValueDictionary PaymentInfoRouteValues { get; set; }

        /// <summary>
        /// Used on one-page checkout page
        /// </summary>
        public bool DisplayOrderTotals { get; set; }
    }
}