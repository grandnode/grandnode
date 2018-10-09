using Grand.Framework.Mvc.Models;

namespace Grand.Web.Areas.Admin.Models.PushNotifications
{
    public class ReceiversModel : BaseGrandModel
    {
        public int Allowed { get; set; }

        public int Denied { get; set; }
    }
}
