using Grand.Domain.Configuration;

namespace Grand.Plugin.ExternalAuth.Google
{
    public class GoogleExternalAuthSettings : ISettings
    {
        public string ClientKeyIdentifier { get; set; }
        public string ClientSecret { get; set; }
    }
}
