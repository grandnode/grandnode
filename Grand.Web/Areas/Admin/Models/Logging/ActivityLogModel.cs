﻿using Grand.Core.ModelBinding;
using Grand.Core.Models;
using System;

namespace Grand.Web.Areas.Admin.Models.Logging
{
    public partial class ActivityLogModel : BaseEntityModel
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

    public partial class ActivityStatsModel : BaseEntityModel
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
