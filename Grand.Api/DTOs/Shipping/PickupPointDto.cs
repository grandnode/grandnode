using Grand.Framework.Mvc.Models;

namespace Grand.Api.DTOs.Shipping
{
    public partial class PickupPointDto : BaseApiEntityModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string AdminComment { get; set; }
        public string WarehouseId { get; set; }
        public string StoreId { get; set; }
        public decimal PickupFee { get; set; }
        public int DisplayOrder { get; set; }
    }
}
