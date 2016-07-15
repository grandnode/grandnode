using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Admin.Models.Common;
using Nop.Admin.Validators.Shipping;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using System.Collections.Generic;

namespace Nop.Admin.Models.Shipping
{
    [Validator(typeof(PickupPointValidator))]
    public partial class PickupPointModel : BaseNopEntityModel
    {
        public PickupPointModel()
        {
            this.Address = new AddressModel();
            this.AvailableWarehouses = new List<SelectListItem>();
            this.AvailableStores = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Configuration.Shipping.PickupPoint.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Shipping.PickupPoint.Fields.Description")]
        [AllowHtml]
        public string Description { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Shipping.PickupPoint.Fields.AdminComment")]
        [AllowHtml]
        public string AdminComment { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Shipping.PickupPoint.Fields.Address")]
        public AddressModel Address { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Shipping.PickupPoint.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Shipping.PickupPoint.Fields.Warehouses")]
        public IList<SelectListItem> AvailableWarehouses { get; set; }

        public string WarehouseId { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Shipping.PickupPoint.Fields.Stores")]
        public IList<SelectListItem> AvailableStores { get; set; }
        public string StoreId { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Shipping.PickupPoint.Fields.PickupFee")]
        public decimal PickupFee { get; set; }

    }
}