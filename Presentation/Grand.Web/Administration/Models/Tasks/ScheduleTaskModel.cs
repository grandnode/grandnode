using System.Web.Mvc;
using FluentValidation.Attributes;
using Grand.Admin.Validators.Tasks;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Tasks
{
    [Validator(typeof(ScheduleTaskValidator))]
    public partial class ScheduleTaskModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.System.ScheduleTasks.ScheduleTaskName")]
        [AllowHtml]
        public string ScheduleTaskName { get; set; }
        [NopResourceDisplayName("Admin.System.ScheduleTasks.Type")]
        public string Type { get; set; }
        [NopResourceDisplayName("Admin.System.ScheduleTasks.Enabled")]
        public bool Enabled { get; set; }
        [NopResourceDisplayName("Admin.System.ScheduleTasks.StopOnError")]
        public bool StopOnError { get; set; }
        [NopResourceDisplayName("Admin.System.ScheduleTasks.LastStartUtc")]
        public string LastStartUtc { get; set; }
        [NopResourceDisplayName("Admin.System.ScheduleTasks.LastEndUtc")]
        public string LastEndUtc { get; set; }
        [NopResourceDisplayName("Admin.System.ScheduleTasks.LastSuccessUtc")]
        public string LastSuccessUtc { get; set; }

        //Properties below are for FluentScheduler
        [NopResourceDisplayName("Admin.System.ScheduleTasks.TimeIntervalChoice")]
        public int TimeIntervalChoice { get; set; }
        [NopResourceDisplayName("Admin.System.ScheduleTasks.TimeInterval")]
        public int TimeInterval { get; set; }
        [NopResourceDisplayName("Admin.System.ScheduleTasks.MinuteOfHour")]
        public int MinuteOfHour { get; set; }
        [NopResourceDisplayName("Admin.System.ScheduleTasks.HourOfDay")]
        public int HourOfDay { get; set; }
        [NopResourceDisplayName("Admin.System.ScheduleTasks.DayOfWeek")]
        public int DayOfWeek { get; set; }
        [NopResourceDisplayName("Admin.System.ScheduleTasks.MonthOptionChoice")]
        public int MonthOptionChoice { get; set; }
        [NopResourceDisplayName("Admin.System.ScheduleTasks.DayOfMonth")]
        public int DayOfMonth { get; set; }
    }
}