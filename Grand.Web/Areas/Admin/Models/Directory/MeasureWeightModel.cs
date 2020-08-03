using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Areas.Admin.Models.Directory
{
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