using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Admin.Models.Tax
{
    public partial class TaxCategoryModel : BaseEntityModel
    {
        [GrandResourceDisplayName("Admin.Configuration.Tax.Categories.Fields.Name")]

        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Tax.Categories.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }
    }
}