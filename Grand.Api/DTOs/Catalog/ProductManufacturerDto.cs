using System.ComponentModel.DataAnnotations;

namespace Grand.Api.DTOs.Catalog
{
    public partial class ProductManufacturerDto
    {
        [Key]
        public string ManufacturerId { get; set; }
        public bool IsFeaturedProduct { get; set; }
    }
}
