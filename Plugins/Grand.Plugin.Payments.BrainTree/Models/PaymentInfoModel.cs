using Grand.Core.ModelBinding;
using Grand.Core.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Grand.Plugin.Payments.BrainTree.Models
{
    public class PaymentInfoModel : BaseModel
    {
        public PaymentInfoModel()
        {
            ExpireMonths = new List<SelectListItem>();
            ExpireYears = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Payment.SelectCreditCard")]
        public string CreditCardType { get; set; }

        [GrandResourceDisplayName("Payment.CardholderName")]

        public string CardholderName { get; set; }

        [GrandResourceDisplayName("Payment.CardNumber")]
        public string CardNumber { get; set; }

        [GrandResourceDisplayName("Payment.ExpirationDate")]
        public string ExpireMonth { get; set; }

        [GrandResourceDisplayName("Payment.ExpirationDate")]
        public string ExpireYear { get; set; }

        public IList<SelectListItem> ExpireMonths { get; set; }

        public IList<SelectListItem> ExpireYears { get; set; }

        [GrandResourceDisplayName("Payment.CardCode")]
        public string CardCode { get; set; }

        public string CardNonce { get; set; }
        public string Errors { get; set; }

    }
}