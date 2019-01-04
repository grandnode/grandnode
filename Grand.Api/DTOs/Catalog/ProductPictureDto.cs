using FluentValidation.Attributes;
using Grand.Api.Validators.Catalog;
using System.ComponentModel.DataAnnotations;

namespace Grand.Api.DTOs.Catalog
{
    [Validator(typeof(ProductPictureValidator))]
    public partial class ProductPictureDto
    {
        [Key]
        public string PictureId { get; set; }
        public int DisplayOrder { get; set; }
        public string MimeType { get; set; }
        public string SeoFilename { get; set; }
        public string AltAttribute { get; set; }
        public string TitleAttribute { get; set; }
    }
}
