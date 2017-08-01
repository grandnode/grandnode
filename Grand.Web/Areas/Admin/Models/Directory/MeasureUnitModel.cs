using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Validators.Directory;
using Grand.Framework;
using Grand.Framework.Mvc;

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