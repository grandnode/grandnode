using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Admin.Models.Templates
{
    public partial class TopicTemplateModel : BaseEntityModel
    {
        [GrandResourceDisplayName("Admin.System.Templates.Topic.Name")]

        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.System.Templates.Topic.ViewPath")]

        public string ViewPath { get; set; }

        [GrandResourceDisplayName("Admin.System.Templates.Topic.DisplayOrder")]
        public int DisplayOrder { get; set; }
    }
}