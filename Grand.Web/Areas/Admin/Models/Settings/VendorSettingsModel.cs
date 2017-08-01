using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Settings
{
    public partial class VendorSettingsModel : BaseGrandModel
    {
        public string ActiveStoreScopeConfiguration { get; set; }


        [GrandResourceDisplayName("Admin.Configuration.Settings.Vendor.VendorsBlockItemsToDisplay")]
        public int VendorsBlockItemsToDisplay { get; set; }
        public bool VendorsBlockItemsToDisplay_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Vendor.ShowVendorOnProductDetailsPage")]
        public bool ShowVendorOnProductDetailsPage { get; set; }
        public bool ShowVendorOnProductDetailsPage_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Vendor.AllowCustomersToContactVendors")]
        public bool AllowCustomersToContactVendors { get; set; }
        public bool AllowCustomersToContactVendors_OverrideForStore { get; set; }
        [GrandResourceDisplayName("Admin.Configuration.Settings.Vendor.AllowCustomersToApplyForVendorAccount")]
        public bool AllowCustomersToApplyForVendorAccount { get; set; }
        public bool AllowCustomersToApplyForVendorAccount_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Settings.Vendor.AllowSearchByVendor")]
        public bool AllowSearchByVendor { get; set; }
        public bool AllowSearchByVendor_OverrideForStore { get; set; }

    }
}