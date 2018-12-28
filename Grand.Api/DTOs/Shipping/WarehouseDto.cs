using Grand.Framework.Mvc.Models;

namespace Grand.Api.DTOs.Shipping
{
    public partial class WarehouseDto : BaseApiEntityModel
    {
        public string Name { get; set; }
        public string AdminComment { get; set; }
    }
}
