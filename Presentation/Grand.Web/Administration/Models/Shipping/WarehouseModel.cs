using System.Web.Mvc;
using FluentValidation.Attributes;
using Grand.Admin.Models.Common;
using Grand.Admin.Validators.Shipping;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Admin.Models.Shipping
{
    [Validator(typeof(WarehouseValidator))]
    public partial class WarehouseModel : BaseNopEntityModel
    {
        public WarehouseModel()
        {
            this.Address = new AddressModel();
        }

        [GrandResourceDisplayName("Admin.Configuration.Shipping.Warehouses.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Shipping.Warehouses.Fields.AdminComment")]
        [AllowHtml]
        public string AdminComment { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Shipping.Warehouses.Fields.Address")]
        public AddressModel Address { get; set; }
    }
}