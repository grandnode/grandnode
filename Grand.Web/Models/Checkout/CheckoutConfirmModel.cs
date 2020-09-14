using Grand.Core.Models;
using System.Collections.Generic;

namespace Grand.Web.Models.Checkout
{
    public partial class CheckoutConfirmModel : BaseModel
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