using Grand.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Core.Domain.PushNotifications
{
    public class PushMessage : BaseEntity
    {
        public string Title { get; set; }

        public string Text { get; set; }

        public DateTime SentOn { get; set; }

        public int NumberOfReceivers { get; set; }
    }
}
