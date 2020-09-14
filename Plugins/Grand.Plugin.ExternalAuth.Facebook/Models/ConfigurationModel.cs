using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Plugin.ExternalAuth.Facebook.Models
{
    public class ConfigurationModel : BaseModel
    {
        [GrandResourceDisplayName("Plugins.ExternalAuth.Facebook.ClientKeyIdentifier")]
        public string ClientId { get; set; }

        [GrandResourceDisplayName("Plugins.ExternalAuth.Facebook.ClientSecret")]
        public string ClientSecret { get; set; }
    }
}