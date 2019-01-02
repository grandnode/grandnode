using Grand.Framework.Mvc.Models;

namespace Grand.Api.DTOs.Catalog
{
    public partial class ProductManufacturerDto : BaseApiEntityModel
    {
        public string ManufacturerId { get; set; }
        public bool IsFeaturedProduct { get; set; }
    }
}
