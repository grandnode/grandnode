using Grand.Core.ModelBinding;
using Grand.Core.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace Grand.Admin.Models.Tasks
{
    public partial class ScheduleTaskModel : BaseEntityModel
    {
        public ScheduleTaskModel()
        {
            AvailableStores = new List<SelectListItem>();
        }
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
        [GrandResourceDisplayName("Admin.System.ScheduleTasks.StoreId")]
        public string StoreId { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
    }
}