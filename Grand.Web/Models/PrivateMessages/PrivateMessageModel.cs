using System;
using FluentValidation.Attributes;
using Grand.Framework.Mvc.Models;
using Grand.Web.Validators.PrivateMessages;

namespace Grand.Web.Models.PrivateMessages
{
    [Validator(typeof(SendPrivateMessageValidator))]
    public partial class PrivateMessageModel : BaseGrandEntityModel
    {
        public string FromCustomerId { get; set; }
        public string CustomerFromName { get; set; }
        public bool AllowViewingFromProfile { get; set; }

        public string ToCustomerId { get; set; }
        public string CustomerToName { get; set; }
        public bool AllowViewingToProfile { get; set; }

        public string Subject { get; set; }

        public string Message { get; set; }
        
        public DateTime CreatedOn { get; set; }

        public bool IsRead { get; set; }
    }
}