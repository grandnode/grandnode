using FluentValidation.Attributes;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using Grand.Web.Areas.Admin.Models.Common;
using Grand.Web.Areas.Admin.Validators.Shipping;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Models.Shipping
{
    [Validator(typeof(PickupPointValidator))]
    public partial class PickupPointModel : BaseGrandEntityModel
    {
        public PickupPointModel()
        {
            this.Address = new AddressModel();
            this.AvailableWarehouses = new List<SelectListItem>();
            this.AvailableStores = new List<SelectListItem>();
        }

        [GrandResourceDisplayName("Admin.Configuration.Shipping.PickupPoint.Fields.Name")]
        
        public string Name { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Shipping.PickupPoint.Fields.Description")]
        
        public string Description { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Shipping.PickupPoint.Fields.AdminComment")]
        
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