using Grand.Framework.Mvc.Models;
using Grand.Framework.Mvc.ModelBinding;
using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Validators.Tasks;
using System;

namespace Grand.Web.Areas.Admin.Models.Tasks
{
    [Validator(typeof(ScheduleTaskValidator))]
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

        //Properties below are for FluentScheduler
        [GrandResourceDisplayName("Admin.System.ScheduleTasks.TimeIntervalChoice")]
        public int TimeIntervalChoice { get; set; }
        [GrandResourceDisplayName("Admin.System.ScheduleTasks.TimeInterval")]
        public int TimeInterval { get; set; }
        [GrandResourceDisplayName("Admin.System.ScheduleTasks.MinuteOfHour")]
        public int MinuteOfHour { get; set; }
        [GrandResourceDisplayName("Admin.System.ScheduleTasks.HourOfDay")]
        public int HourOfDay { get; set; }
        [GrandResourceDisplayName("Admin.System.ScheduleTasks.DayOfWeek")]
        public int DayOfWeek { get; set; }
        [GrandResourceDisplayName("Admin.System.ScheduleTasks.MonthOptionChoice")]
        public int MonthOptionChoice { get; set; }
        [GrandResourceDisplayName("Admin.System.ScheduleTasks.DayOfMonth")]
        public int DayOfMonth { get; set; }
    }
}