using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.Areas.Admin.Models.PushNotifications
{
    public partial class PushModel : BaseGrandModel
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
