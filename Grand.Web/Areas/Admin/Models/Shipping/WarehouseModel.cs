using FluentValidation.Attributes;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Grand.Web.Areas.Admin.Models.Common;
using Grand.Web.Areas.Admin.Validators.Shipping;

namespace Grand.Web.Areas.Admin.Models.Shipping
{
    [Validator(typeof(WarehouseValidator))]
    public partial class WarehouseModel : BaseGrandEntityModel
    {
        public WarehouseModel()
        {
            this.Address = new AddressModel();
        }

        [GrandResourceDisplayName("Admin.Configuration.Shipping.Warehouses.Fields.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Shipping.Warehouses.Fields.AdminComment")]
        
        public string AdminComment { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Shipping.Warehouses.Fields.Address")]
        public AddressModel Address { get; set; }
    }
}