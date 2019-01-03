using FluentValidation.Attributes;
using Grand.Api.Validators.Catalog;
using System.ComponentModel.DataAnnotations;

namespace Grand.Api.DTOs.Catalog
{
    [Validator(typeof(ProductManufacturerValidator))]
    public partial class ProductManufacturerDto
    {
        [Key]
        public string ManufacturerId { get; set; }
        public bool IsFeaturedProduct { get; set; }
    }
}
