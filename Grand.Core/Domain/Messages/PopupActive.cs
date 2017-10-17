using System;

namespace Grand.Core.Domain.Messages
{
    public partial class PopupActive : BaseEntity
    {
        public string Name { get; set; }
        public string Body { get; set; }
        public string CustomerId { get; set; }
        public string CustomerActionId { get; set; }
        public int PopupTypeId { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }

}
