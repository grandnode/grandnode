using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Models;

namespace Grand.Plugin.Payments.PayInStore.Models
{
    public class PaymentInfoModel : BaseGrandModel
    {
        public string DescriptionText { get; set; }
    }
}