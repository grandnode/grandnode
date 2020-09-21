using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Web.Areas.Admin.Models.Directory
{
    public partial class MeasureWeightModel : BaseEntityModel
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