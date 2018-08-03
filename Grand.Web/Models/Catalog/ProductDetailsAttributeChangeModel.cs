using Grand.Framework.Mvc.Models;
using System.Collections.Generic;

namespace Grand.Web.Models.Catalog
{
    public partial class ProductDetailsAttributeChangeModel : BaseGrandEntityModel
    {
        public ProductDetailsAttributeChangeModel()
        {
            EnabledAttributeMappingIds = new List<string>();
            DisabledAttributeMappingids = new List<string>();
        }
        public string Gtin { get; set; }
        public string Mpn { get; set; }
        public string Sku { get; set; }
        public string Price { get; set; }
        public string StockAvailability { get; set; }
        public IList<string> EnabledAttributeMappingIds { get; set; }
        public IList<string> DisabledAttributeMappingids { get; set; }
        public string PictureFullSizeUrl { get; set; }
        public string PictureDefaultSizeUrl { get; set; }

    }
}
