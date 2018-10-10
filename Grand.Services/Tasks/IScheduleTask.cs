using FluentScheduler;
using Grand.Core.Domain.Tasks;
using System;

namespace Grand.Services.Tasks
{
    public interface IScheduleTask : IJob
    {
        string Id { get; set; }
        string ScheduleTaskName { get; set; }
        string Type { get; set; }
        bool Enabled { get; set; }
        bool StopOnError { get; set; }
        DateTime? LastStartUtc { get; set; }
        DateTime? LastNonSuccessEndUtc { get; set; }
        DateTime? LastSuccessUtc { get; set; }
        string LeasedByMachineName { get; set; }
        DateTime? LeasedUntilUtc { get; set; }

        //Properties below are for FluentScheduler
        TimeIntervalChoice TimeIntervalChoice { get; set; }
        int TimeInterval { get; set; }
        int MinuteOfHour { get; set; }
        int HourOfDay { get; set; }
        DayOfWeek DayOfWeek { get; set; }
        MonthOptionChoice MonthOptionChoice { get; set; }
        int DayOfMonth { get; set; }
    }
}
