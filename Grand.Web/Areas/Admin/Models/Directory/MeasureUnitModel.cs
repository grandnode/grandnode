using FluentValidation.Attributes;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Grand.Web.Areas.Admin.Validators.Directory;

namespace Grand.Web.Areas.Admin.Models.Directory
{
    [Validator(typeof(MeasureUnitValidator))]
    public partial class MeasureUnitModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.Configuration.Measures.Units.Fields.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Measures.Units.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

    }
}