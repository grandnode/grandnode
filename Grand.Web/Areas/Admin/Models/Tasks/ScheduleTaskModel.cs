using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using System;

namespace Grand.Web.Areas.Admin.Models.Tasks
{
    public partial class ScheduleTaskModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.System.ScheduleTasks.ScheduleTaskName")]
        public string ScheduleTaskName { get; set; }

        [GrandResourceDisplayName("Admin.System.ScheduleTasks.LeasedByMachineName")]
        public string LeasedByMachineName { get; set; }

        [GrandResourceDisplayName("Admin.System.ScheduleTasks.Type")]
        public string Type { get; set; }
        [GrandResourceDisplayName("Admin.System.ScheduleTasks.Enabled")]
        public bool Enabled { get; set; }
        [GrandResourceDisplayName("Admin.System.ScheduleTasks.StopOnError")]
        public bool StopOnError { get; set; }
        [GrandResourceDisplayName("Admin.System.ScheduleTasks.LastStartUtc")]
        public DateTime? LastStartUtc { get; set; }
        [GrandResourceDisplayName("Admin.System.ScheduleTasks.LastEndUtc")]
        public DateTime? LastEndUtc { get; set; }
        [GrandResourceDisplayName("Admin.System.ScheduleTasks.LastSuccessUtc")]
        public DateTime? LastSuccessUtc { get; set; }
        [GrandResourceDisplayName("Admin.System.ScheduleTasks.TimeInterval")]
        public int TimeInterval { get; set; }
    }
}