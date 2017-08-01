using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using System;
using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Logging
{
    public partial class ActivityLogModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.ActivityLogType")]
        public string ActivityLogTypeName { get; set; }
        public string ActivityLogTypeId { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.Customer")]
        public string CustomerId { get; set; }
        [GrandResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.Customer")]
        public string CustomerEmail { get; set; }
        [GrandResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.Comment")]
        public string Comment { get; set; }
        [GrandResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.IpAddress")]
        public string IpAddress { get; set; }
        [GrandResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }
    }

    public partial class ActivityStatsModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.Configuration.ActivityLog.ActivityStats.Fields.ActivityLogType")]
        public string ActivityLogTypeName { get; set; }
        public string ActivityLogTypeId { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.ActivityLog.ActivityStats.Fields.EntityKeyId")]
        public string EntityKeyId { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.ActivityLog.ActivityStats.Fields.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.ActivityLog.ActivityStats.Fields.Count")]
        public int Count { get; set; }

    }
}
