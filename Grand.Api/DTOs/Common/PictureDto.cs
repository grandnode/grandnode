using Grand.Framework.Mvc.Models;

namespace Grand.Api.DTOs.Common
{
    public partial class PictureDto : BaseApiEntityModel
    {
        public byte[] PictureBinary { get; set; }
        public string MimeType { get; set; }
        public string SeoFilename { get; set; }
        public string AltAttribute { get; set; }
        public string TitleAttribute { get; set; }
        public bool IsNew { get; set; }
    }
}
