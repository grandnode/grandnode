using Grand.Framework.Mvc.Models;

namespace Grand.Web.Models.PushNotifications
{
    public class PublicInfoModel : BaseGrandModel
    {
        public string SenderId { get; set; }
        public string PublicApiKey { get; set; }
        public string AuthDomain { get; set; }
        public string DatabaseUrl { get; set; }
        public string ProjectId { get; set; }
        public string StorageBucket { get; set; }
    }
}
