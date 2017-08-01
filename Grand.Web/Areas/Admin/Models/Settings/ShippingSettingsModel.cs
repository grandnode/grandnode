using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Web.Areas.Admin.Models.Common;
using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Settings
{
    public partial class ShippingSettingsModel : BaseGrandModel
    {
        public string ActiveStoreScopeConfiguration { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Shipping.ShipToSameAddress")]
        public bool ShipToSameAddress { get; set; }
        public bool ShipToSameAddress_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Shipping.AllowPickUpInStore")]
        public bool AllowPickUpInStore { get; set; }
        public bool AllowPickUpInStore_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Shipping.UseWarehouseLocation")]
        public bool UseWarehouseLocation { get; set; }
        public bool UseWarehouseLocation_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Shipping.NotifyCustomerAboutShippingFromMultipleLocations")]
        public bool NotifyCustomerAboutShippingFromMultipleLocations { get; set; }
        public bool NotifyCustomerAboutShippingFromMultipleLocations_OverrideForStore { get; set; }
        
        [GrandResourceDisplayName("Admin.Configuration.Settings.Shipping.FreeShippingOverXEnabled")]
        public bool FreeShippingOverXEnabled { get; set; }
        public bool FreeShippingOverXEnabled_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Shipping.FreeShippingOverXValue")]
        public decimal FreeShippingOverXValue { get; set; }
        public bool FreeShippingOverXValue_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Shipping.FreeShippingOverXIncludingTax")]
        public bool FreeShippingOverXIncludingTax { get; set; }
        public bool FreeShippingOverXIncludingTax_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Shipping.EstimateShippingEnabled")]
        public bool EstimateShippingEnabled { get; set; }
        public bool EstimateShippingEnabled_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Shipping.DisplayShipmentEventsToCustomers")]
        public bool DisplayShipmentEventsToCustomers { get; set; }
        public bool DisplayShipmentEventsToCustomers_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Shipping.DisplayShipmentEventsToStoreOwner")]
        public bool DisplayShipmentEventsToStoreOwner { get; set; }
        public bool DisplayShipmentEventsToStoreOwner_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Shipping.BypassShippingMethodSelectionIfOnlyOne")]
        public bool BypassShippingMethodSelectionIfOnlyOne { get; set; }
        public bool BypassShippingMethodSelectionIfOnlyOne_OverrideForStore { get; set; }
        
        [GrandResourceDisplayName("Admin.Configuration.Settings.Shipping.ShippingOriginAddress")]
        public AddressModel ShippingOriginAddress { get; set; }
        public bool ShippingOriginAddress_OverrideForStore { get; set; }
    }
}