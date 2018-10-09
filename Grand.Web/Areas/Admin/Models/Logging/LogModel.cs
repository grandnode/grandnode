using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using System;

namespace Grand.Web.Areas.Admin.Models.Logging
{
    public partial class LogModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.System.Log.Fields.LogLevel")]
        public string LogLevel { get; set; }

        [GrandResourceDisplayName("Admin.System.Log.Fields.ShortMessage")]
        
        public string ShortMessage { get; set; }

        [GrandResourceDisplayName("Admin.System.Log.Fields.FullMessage")]
        
        public string FullMessage { get; set; }

        [GrandResourceDisplayName("Admin.System.Log.Fields.IPAddress")]
        
        public string IpAddress { get; set; }

        [GrandResourceDisplayName("Admin.System.Log.Fields.Customer")]
        public string CustomerId { get; set; }
        [GrandResourceDisplayName("Admin.System.Log.Fields.Customer")]
        public string CustomerEmail { get; set; }

        [GrandResourceDisplayName("Admin.System.Log.Fields.PageURL")]
        
        public string PageUrl { get; set; }

        [GrandResourceDisplayName("Admin.System.Log.Fields.ReferrerURL")]
        
        public string ReferrerUrl { get; set; }

        [GrandResourceDisplayName("Admin.System.Log.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }
    }
}