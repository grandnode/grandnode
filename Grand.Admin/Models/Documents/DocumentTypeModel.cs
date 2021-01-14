using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Admin.Models.Documents
{
    public partial class DocumentTypeModel : BaseEntityModel
    {
        [GrandResourceDisplayName("Admin.Documents.Type.Fields.Name")]

        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Documents.Type.Fields.Description")]

        public string Description { get; set; }

        [GrandResourceDisplayName("Admin.Documents.Type.Fields.DisplayOrder")]

        public int DisplayOrder { get; set; }
    }
}
