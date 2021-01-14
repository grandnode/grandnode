using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Admin.Models.Templates
{
    public partial class ProductTemplateModel : BaseEntityModel
    {
        [GrandResourceDisplayName("Admin.System.Templates.Product.Name")]

        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.System.Templates.Product.ViewPath")]

        public string ViewPath { get; set; }

        [GrandResourceDisplayName("Admin.System.Templates.Product.DisplayOrder")]
        public int DisplayOrder { get; set; }
    }
}