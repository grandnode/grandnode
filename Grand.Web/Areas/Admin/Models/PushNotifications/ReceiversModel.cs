using Grand.Framework.Mvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Models.PushNotifications
{
    public class ReceiversModel : BaseGrandModel
    {
        public int Allowed { get; set; }

        public int Denied { get; set; }
    }
}
