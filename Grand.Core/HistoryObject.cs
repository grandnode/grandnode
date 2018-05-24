using System;

namespace Grand.Core
{
    public class HistoryObject: BaseEntity
    {
        public DateTime CreatedOnUtc { get; set; }
        public BaseEntity Object { get; set; }
    }
}
