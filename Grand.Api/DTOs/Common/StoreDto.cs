using Grand.Api.Models;

namespace Grand.Api.DTOs.Common
{
    public partial class StoreDto : BaseApiEntityModel
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public bool SslEnabled { get; set; }
        public string SecureUrl { get; set; }
        public string Hosts { get; set; }
        public string DefaultLanguageId { get; set; }
        public string DefaultWarehouseId { get; set; }
        public int DisplayOrder { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyPhoneNumber { get; set; }
        public string CompanyVat { get; set; }
    }
}
