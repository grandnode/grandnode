using Grand.Core.Models;

namespace Grand.Web.Areas.Admin.Models.PushNotifications
{
    public class ReceiversModel : BaseModel
    {
        public int Allowed { get; set; }

        public int Denied { get; set; }
    }
}
