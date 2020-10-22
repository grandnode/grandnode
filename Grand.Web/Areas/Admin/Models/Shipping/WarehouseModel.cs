using Grand.Core.ModelBinding;
using Grand.Core.Models;
using Grand.Web.Areas.Admin.Models.Common;

namespace Grand.Web.Areas.Admin.Models.Shipping
{
    public partial class WarehouseModel : BaseEntityModel
    {
        public WarehouseModel()
        {
            Address = new AddressModel();
        }

        [GrandResourceDisplayName("Admin.Configuration.Shipping.Warehouses.Fields.Name")]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Shipping.Warehouses.Fields.AdminComment")]
        public string AdminComment { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Shipping.Warehouses.Fields.Latitude")]
        public double? Latitude { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Shipping.Warehouses.Fields.Longitude")]
        public double? Longitude { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Shipping.Warehouses.Fields.Address")]
        public AddressModel Address { get; set; }
    }
}