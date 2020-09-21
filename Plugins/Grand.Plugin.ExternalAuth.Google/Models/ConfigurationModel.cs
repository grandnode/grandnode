using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Plugin.ExternalAuth.Google.Models
{
    public class ConfigurationModel : BaseModel
    {
        public string ActiveStoreScopeConfiguration { get; set; }

        [GrandResourceDisplayName("Plugins.ExternalAuth.Google.ClientKeyIdentifier")]
        public string ClientKeyIdentifier { get; set; }
        public bool ClientKeyIdentifier_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Plugins.ExternalAuth.Google.ClientSecret")]
        public string ClientSecret { get; set; }
        public bool ClientSecret_OverrideForStore { get; set; }
    }
}
