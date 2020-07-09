using System;

namespace Grand.Domain.History
{
    public class HistoryObject: BaseEntity
    {
        public DateTime CreatedOnUtc { get; set; }
        public BaseEntity Object { get; set; }
    }
}
