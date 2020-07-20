using Grand.Framework.Mvc.Models;

namespace Grand.Web.Models.PrivateMessages
{
    public partial class SendPrivateMessageModel : BaseGrandEntityModel
    {
        public string ToCustomerId { get; set; }
        public string CustomerToName { get; set; }
        public bool AllowViewingToProfile { get; set; }

        public string ReplyToMessageId { get; set; }

        public string Subject { get; set; }

        public string Message { get; set; }
    }
}