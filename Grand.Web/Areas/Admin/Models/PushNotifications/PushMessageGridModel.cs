using Grand.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Models.PushNotifications
{
    public class PushMessageGridModel : BaseEntity
    {
        public string Title { get; set; }

        public string Text { get; set; }

        public DateTime SentOn { get; set; }

        public int NumberOfReceivers { get; set; }
    }
}
