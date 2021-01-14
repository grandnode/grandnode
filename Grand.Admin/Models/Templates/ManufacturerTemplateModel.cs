using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Admin.Models.Templates
{
    public partial class ManufacturerTemplateModel : BaseEntityModel
    {
        [GrandResourceDisplayName("Admin.System.Templates.Manufacturer.Name")]

        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.System.Templates.Manufacturer.ViewPath")]

        public string ViewPath { get; set; }

        [GrandResourceDisplayName("Admin.System.Templates.Manufacturer.DisplayOrder")]
        public int DisplayOrder { get; set; }
    }
}