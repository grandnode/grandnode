using System.Collections.Generic;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Models.Checkout
{
    public partial class CheckoutConfirmModel : BaseGrandModel
    {
        public CheckoutConfirmModel()
        {
            Warnings = new List<string>();
        }

        public bool TermsOfServiceOnOrderConfirmPage { get; set; }
        public string MinOrderTotalWarning { get; set; }

        public IList<string> Warnings { get; set; }
    }
}