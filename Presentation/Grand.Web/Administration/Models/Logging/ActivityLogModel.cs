﻿using System;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Logging
{
    public partial class ActivityLogModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.ActivityLogType")]
        public string ActivityLogTypeName { get; set; }
        [NopResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.Customer")]
        public string CustomerId { get; set; }
        [NopResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.Customer")]
        public string CustomerEmail { get; set; }
        [NopResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.Comment")]
        public string Comment { get; set; }
        [NopResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.IpAddress")]
        public string IpAddress { get; set; }
        [NopResourceDisplayName("Admin.Configuration.ActivityLog.ActivityLog.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }
    }

    public partial class ActivityStatsModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.Configuration.ActivityLog.ActivityStats.Fields.ActivityLogType")]
        public string ActivityLogTypeName { get; set; }
        public string ActivityLogTypeId { get; set; }

        [NopResourceDisplayName("Admin.Configuration.ActivityLog.ActivityStats.Fields.EntityKeyId")]
        public string EntityKeyId { get; set; }

        [NopResourceDisplayName("Admin.Configuration.ActivityLog.ActivityStats.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Configuration.ActivityLog.ActivityStats.Fields.Count")]
        public int Count { get; set; }

    }
}
