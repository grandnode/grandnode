using Grand.Framework.Mvc.Models;

namespace Grand.Api.DTOs.Common
{
    public partial class MessageTemplateDto : BaseApiEntityModel
    {
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
        public string ViewPath { get; set; }
    }
}
