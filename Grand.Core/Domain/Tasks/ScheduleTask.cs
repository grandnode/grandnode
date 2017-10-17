using System;

namespace Grand.Core.Domain.Tasks
{
    public partial class ScheduleTask : BaseEntity
    {
        public string ScheduleTaskName { get; set; }
        public string Type { get; set; }
        public bool Enabled { get; set; }
        public bool StopOnError { get; set; }
        public DateTime? LastStartUtc { get; set; }
        public DateTime? LastNonSuccessEndUtc { get; set; }
        public DateTime? LastSuccessUtc { get; set; }
        public string LeasedByMachineName { get; set; }
        public DateTime? LeasedUntilUtc { get; set; }

        //Properties below are for FluentScheduler
        public TimeIntervalChoice TimeIntervalChoice { get; set; }
        public int TimeInterval { get; set; }
        public int MinuteOfHour { get; set; }
        public int HourOfDay { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public MonthOptionChoice MonthOptionChoice { get; set; }
        public int DayOfMonth { get; set; }
    }
}
