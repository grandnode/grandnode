using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using FluentValidation.Attributes;
using Grand.Web.Areas.Admin.Validators.Directory;
using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Directory
{
    [Validator(typeof(MeasureDimensionValidator))]
    public partial class MeasureDimensionModel : BaseGrandEntityModel
    {
        [GrandResourceDisplayName("Admin.Configuration.Measures.Dimensions.Fields.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Measures.Dimensions.Fields.SystemKeyword")]
        
        public string SystemKeyword { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Measures.Dimensions.Fields.Ratio")]
        public decimal Ratio { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Measures.Dimensions.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Measures.Dimensions.Fields.IsPrimaryDimension")]
        public bool IsPrimaryDimension { get; set; }
    }
}