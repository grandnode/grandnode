using System;

namespace Grand.Domain.Messages
{
    public partial class PopupArchive : BaseEntity
    {
        public string PopupActiveId { get; set; }
        public string Name { get; set; }
        public string Body { get; set; }
        public string CustomerId { get; set; }
        public string CustomerActionId { get; set; }
        public int PopupTypeId { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime BACreatedOnUtc { get; set; }
    }
}
