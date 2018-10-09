using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Areas.Admin.Models.PushNotifications
{
    public class ConfigurationModel : BaseGrandModel
    {
        [GrandResourceDisplayName("PushNotifications.Fields.NotificationsEnabled")]
        public bool Enabled { get; set; }

        [GrandResourceDisplayName("PushNotifications.Fields.PrivateApiKey")]
        public string PrivateApiKey { get; set; }

        [GrandResourceDisplayName("PushNotifications.Fields.PushApiKey")]
        public string PushApiKey { get; set; }

        [GrandResourceDisplayName("PushNotifications.Fields.SenderId")]
        public string SenderId { get; set; }

        [GrandResourceDisplayName("PushNotifications.Fields.AuthDomain")]
        public string AuthDomain { get; set; }

        [GrandResourceDisplayName("PushNotifications.Fields.DatabaseUrl")]
        public string DatabaseUrl { get; set; }

        [GrandResourceDisplayName("PushNotifications.Fields.ProjectId")]
        public string ProjectId { get; set; }

        [GrandResourceDisplayName("PushNotifications.Fields.StorageBucket")]
        public string StorageBucket { get; set; }

        [GrandResourceDisplayName("PushNotifications.Fields.AllowGuestNotifications")]
        public bool AllowGuestNotifications { get; set; }
    }
}
