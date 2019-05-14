using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.Extensions.DependencyInjection;
using Grand.Core.Infrastructure;
using Grand.Services.Authentication.External;
using System;

namespace Grand.Plugin.ExternalAuth.Google.Infrastructure
{
    /// <summary>
    /// Registration of google authentication service (plugin)
    /// </summary>
    public class GoogleAuthenticationRegistrar : IExternalAuthenticationRegistrar
    {
        /// <summary>
        /// Configure
        /// </summary>
        /// <param name="builder">Authentication builder</param>
        public void Configure(AuthenticationBuilder builder)
        {
            builder.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
            {
                var settings = EngineContext.Current.Resolve<GoogleExternalAuthSettings>();
                options.ClientId = !string.IsNullOrWhiteSpace(settings.ClientKeyIdentifier) ? settings.ClientKeyIdentifier : "000";
                options.ClientSecret = !string.IsNullOrWhiteSpace(settings.ClientSecret) ? settings.ClientSecret : "000";
                options.SaveTokens = true;                
            });

        }
        public int Order => 0;

    }
}
