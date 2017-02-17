using System.Web.Mvc;
using FluentValidation.Attributes;
using Grand.Admin.Models.Common;
using Grand.Admin.Validators.Shipping;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;
using System.Collections.Generic;

namespace Grand.Admin.Models.Shipping
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

        [GrandResourceDisplayName("Admin.Configuration.Shipping.PickupPoint.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Shipping.PickupPoint.Fields.Description")]
        [AllowHtml]
        public string Description { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Shipping.PickupPoint.Fields.AdminComment")]
        [AllowHtml]
        public string AdminComment { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Shipping.PickupPoint.Fields.Address")]
        public AddressModel Address { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Shipping.PickupPoint.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Shipping.PickupPoint.Fields.Warehouses")]
        public IList<SelectListItem> AvailableWarehouses { get; set; }

        public string WarehouseId { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Shipping.PickupPoint.Fields.Stores")]
        public IList<SelectListItem> AvailableStores { get; set; }
        public string StoreId { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Shipping.PickupPoint.Fields.PickupFee")]
        public decimal PickupFee { get; set; }

    }
}