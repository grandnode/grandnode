using Grand.Core.Models;

namespace Grand.Admin.Models.PushNotifications
{
    public class ReceiversModel : BaseModel
    {
        public int Allowed { get; set; }

        public int Denied { get; set; }
    }
}
