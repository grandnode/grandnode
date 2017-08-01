using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Validators.Directory;
using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Directory
{
    [Validator(typeof(MeasureWeightValidator))]
    public partial class MeasureWeightModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.Configuration.Measures.Weights.Fields.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Measures.Weights.Fields.SystemKeyword")]
        
        public string SystemKeyword { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Measures.Weights.Fields.Ratio")]
        public decimal Ratio { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Measures.Weights.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Measures.Weights.Fields.IsPrimaryWeight")]
        public bool IsPrimaryWeight { get; set; }
    }
}