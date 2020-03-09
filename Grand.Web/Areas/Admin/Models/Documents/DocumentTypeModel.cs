using FluentValidation.Attributes;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Grand.Web.Areas.Admin.Validators.Documents;

namespace Grand.Web.Areas.Admin.Models.Documents
{
    [Validator(typeof(DocumentTypeValidator))]
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
