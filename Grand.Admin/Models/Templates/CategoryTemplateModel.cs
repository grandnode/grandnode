using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Admin.Models.Templates
{
    public partial class CategoryTemplateModel : BaseEntityModel
    {
        [GrandResourceDisplayName("Admin.System.Templates.Category.Name")]

        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.System.Templates.Category.ViewPath")]

        public string ViewPath { get; set; }

        [GrandResourceDisplayName("Admin.System.Templates.Category.DisplayOrder")]
        public int DisplayOrder { get; set; }
    }
}