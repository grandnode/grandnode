using Grand.Framework.Mvc.Models;

namespace Grand.Web.Models.Catalog
{
    public partial class ProductSpecificationModel : BaseGrandModel
    {
        public string SpecificationAttributeId { get; set; }

        public string SpecificationAttributeName { get; set; }

        //this value is already HTML encoded
        public string ValueRaw { get; set; }
        public string ColorSquaresRgb { get; set; }
    }
}