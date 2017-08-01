using Grand.Framework.Mvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Grand.Framework.Mvc.ModelBinding;

using Grand.Framework;
using Grand.Framework.Mvc;

namespace Grand.Web.Areas.Admin.Models.Shipping
{
    public partial class ShippingRateComputationMethodModel : BaseGrandModel
    {
        [GrandResourceDisplayName("Admin.Configuration.Shipping.Providers.Fields.FriendlyName")]
        
        public string FriendlyName { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Shipping.Providers.Fields.SystemName")]
        
        public string SystemName { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Shipping.Providers.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Shipping.Providers.Fields.IsActive")]
        public bool IsActive { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Shipping.Providers.Fields.Logo")]
        public string LogoUrl { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Shipping.Providers.Configure")]
        public string ConfigurationUrl { get; set; }
    }
}