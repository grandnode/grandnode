using System;

namespace Grand.Domain.Tasks
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
        public int TimeInterval { get; set; }
        public string StoreId { get; set; }
    }
}
