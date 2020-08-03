using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Areas.Admin.Models.Templates
{
    public partial class TopicTemplateModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.System.Templates.Topic.Name")]

        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.System.Templates.Topic.ViewPath")]

        public string ViewPath { get; set; }

        [GrandResourceDisplayName("Admin.System.Templates.Topic.DisplayOrder")]
        public int DisplayOrder { get; set; }
    }
}