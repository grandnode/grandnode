using Grand.Core.ModelBinding;
using Grand.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Areas.Admin.Models.PushNotifications
{
    public partial class PushModel : BaseModel
    {
        [GrandResourceDisplayName("Admin.PushNotifications.Fields.PushTitle")]
        public string Title { get; set; }

        [GrandResourceDisplayName("Admin.PushNotifications.Fields.PushMessageText")]
        public string MessageText { get; set; }

        [UIHint("Picture")]
        [GrandResourceDisplayName("Admin.PushNotifications.Fields.Picture")]
        public string PictureId { get; set; }

        [GrandResourceDisplayName("Admin.PushNotifications.Fields.ClickUrl")]
        public string ClickUrl { get; set; }
    }
}
