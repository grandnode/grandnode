using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;

namespace Grand.Web.Areas.Admin.Models.Tax
{
    public partial class TaxProviderModel : BaseGrandModel
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