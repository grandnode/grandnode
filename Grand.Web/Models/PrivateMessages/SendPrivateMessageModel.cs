using Microsoft.AspNetCore.Mvc;
using FluentValidation.Attributes;
using Grand.Framework.Mvc.Models;
using Grand.Web.Validators.PrivateMessages;

namespace Grand.Web.Models.PrivateMessages
{
    [Validator(typeof(SendPrivateMessageValidator))]
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