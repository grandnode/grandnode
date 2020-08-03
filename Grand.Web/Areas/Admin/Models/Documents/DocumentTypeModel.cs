using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Areas.Admin.Models.Documents
{
    public partial class DocumentTypeModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.Documents.Type.Fields.Name")]

        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Documents.Type.Fields.Description")]

        public string Description { get; set; }

        [GrandResourceDisplayName("Admin.Documents.Type.Fields.DisplayOrder")]

        public int DisplayOrder { get; set; }
    }
}
