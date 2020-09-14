using Grand.Core.Models;

namespace Grand.Web.Models.Catalog
{
    public partial class ProductTagModel : BaseEntityModel
    {
        public string Name { get; set; }

        public string SeName { get; set; }

        public int ProductCount { get; set; }
    }
}