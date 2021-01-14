using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Admin.Models.Shipping
{
    public partial class ShippingRateComputationMethodModel : BaseModel
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