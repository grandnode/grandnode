using Grand.Core.Models;
using System;

namespace Grand.Web.Models.PrivateMessages
{
    public partial class PrivateMessageModel : BaseEntityModel
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