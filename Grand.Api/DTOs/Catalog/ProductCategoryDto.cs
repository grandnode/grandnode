using Grand.Framework.Mvc.Models;

namespace Grand.Api.DTOs.Catalog
{
    public partial class ProductCategoryDto : BaseApiEntityModel
    {
        public string CategoryId { get; set; }
        public bool IsFeaturedProduct { get; set; }
    }
}
