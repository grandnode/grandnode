using Grand.Api.Models;

namespace Grand.Api.DTOs.Common
{
    public partial class LanguageDto : BaseApiEntityModel
    {
        public string Name { get; set; }
        public string LanguageCulture { get; set; }
        public string UniqueSeoCode { get; set; }
        public string FlagImageFileName { get; set; }
        public bool Rtl { get; set; }
        public string DefaultCurrencyId { get; set; }
        public bool Published { get; set; }
        public int DisplayOrder { get; set; }
    }
}
