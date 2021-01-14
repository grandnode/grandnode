using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Admin.Models.Tax
{
    public partial class TaxProviderModel : BaseModel
    {
        [GrandResourceDisplayName("Admin.Configuration.Tax.Providers.Fields.FriendlyName")]
        
        public string FriendlyName { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Tax.Providers.Fields.SystemName")]
        
        public string SystemName { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Tax.Providers.Fields.IsPrimaryTaxProvider")]
        public bool IsPrimaryTaxProvider { get; set; }

        [GrandResourceDisplayName("Admin.Configuration.Tax.Providers.Fields.Configure")]
        public string ConfigurationUrl { get; set; }
    }
}