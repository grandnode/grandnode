using FluentValidation.Attributes;
using Grand.Api.Validators.Catalog;
using System.ComponentModel.DataAnnotations;

namespace Grand.Api.DTOs.Catalog
{
    [Validator(typeof(ProductCategoryValidator))]
    public partial class ProductCategoryDto
    {
        [Key]
        public string CategoryId { get; set; }
        public bool IsFeaturedProduct { get; set; }
    }
}
