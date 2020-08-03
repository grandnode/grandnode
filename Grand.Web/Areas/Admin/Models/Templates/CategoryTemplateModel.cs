using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Areas.Admin.Models.Templates
{
    public partial class CategoryTemplateModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.System.Templates.Category.Name")]

        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.System.Templates.Category.ViewPath")]

        public string ViewPath { get; set; }

        [GrandResourceDisplayName("Admin.System.Templates.Category.DisplayOrder")]
        public int DisplayOrder { get; set; }
    }
}