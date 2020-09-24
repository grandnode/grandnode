using Grand.Domain.Configuration;

namespace Grand.Domain.PushNotifications
{
    public class PushNotificationsSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether push notifications are enabled
        /// </summary>
        public bool Enabled { get; set; }
        public string PrivateApiKey { get; set; }
        public string PublicApiKey { get; set; }
        public string SenderId { get; set; }
        public string AuthDomain { get; set; }
        public string DatabaseUrl { get; set; }
        public string ProjectId { get; set; }
        public string StorageBucket { get; set; }
        public string PictureId { get; set; }
        public bool AllowGuestNotifications { get; set; }
        public string ClickUrl { get; set; }
        public string AppId { get; set; }
    }
}
