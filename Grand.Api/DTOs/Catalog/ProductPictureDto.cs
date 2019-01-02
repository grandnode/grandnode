using Grand.Framework.Mvc.Models;

namespace Grand.Api.DTOs.Catalog
{
    public partial class ProductPictureDto : BaseApiEntityModel
    {
        public string PictureId { get; set; }
        public int DisplayOrder { get; set; }
        public string MimeType { get; set; }
        public string SeoFilename { get; set; }
        public string AltAttribute { get; set; }
        public string TitleAttribute { get; set; }
    }
}
