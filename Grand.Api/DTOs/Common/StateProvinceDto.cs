using Grand.Api.Models;

namespace Grand.Api.DTOs.Common
{
    public partial class StateProvinceDto : BaseApiEntityModel
    {
        public string CountryId { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public bool Published { get; set; }
        public int DisplayOrder { get; set; }
    }
}
