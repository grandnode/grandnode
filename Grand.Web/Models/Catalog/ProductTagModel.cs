using Grand.Framework.Mvc.Models;

namespace Grand.Web.Models.Catalog
{
    public partial class ProductTagModel : BaseGrandEntityModel
    {
        public string Name { get; set; }

        public string SeName { get; set; }

        public int ProductCount { get; set; }
    }
}