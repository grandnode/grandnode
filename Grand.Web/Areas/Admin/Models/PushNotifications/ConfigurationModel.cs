using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Web.Areas.Admin.Models.PushNotifications
{
    public class ConfigurationModel : BaseModel
    {
        [GrandResourceDisplayName("Admin.Configuration.Settings.PushNotifications.NotificationsEnabled")]
        public bool Enabled { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.PushNotifications.PrivateApiKey")]
        public string PrivateApiKey { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.PushNotifications.PushApiKey")]
        public string PushApiKey { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.PushNotifications.SenderId")]
        public string SenderId { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.PushNotifications.AuthDomain")]
        public string AuthDomain { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.PushNotifications.DatabaseUrl")]
        public string DatabaseUrl { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.PushNotifications.ProjectId")]
        public string ProjectId { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.PushNotifications.StorageBucket")]
        public string StorageBucket { get; set; }
        [GrandResourceDisplayName("Admin.Configuration.Settings.PushNotifications.AppId")]
        public string AppId { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.PushNotifications.AllowGuestNotifications")]
        public bool AllowGuestNotifications { get; set; }
    }
}
