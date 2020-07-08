using Grand.Core.Data;
using Grand.Domain.Configuration;
using Grand.Domain.Data;
using Grand.Services.Authentication.External;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

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
        public void Configure(AuthenticationBuilder builder, IConfiguration configuration)
        {
            builder.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
            {
                var settings = new GoogleExternalAuthSettings();
                try
                {
                    var goSettings = new Repository<Setting>(DataSettingsHelper.ConnectionString()).Table.Where(x => x.Name.StartsWith("googleexternalauthsettings"));
                    settings.ClientKeyIdentifier = goSettings.FirstOrDefault(x => x.Name == "googleexternalauthsettings.clientkeyidentifier")?.Value;
                    settings.ClientSecret = goSettings.FirstOrDefault(x => x.Name == "googleexternalauthsettings.clientsecret")?.Value;
                }
                catch { };

                options.ClientId = !string.IsNullOrWhiteSpace(settings.ClientKeyIdentifier) ? settings.ClientKeyIdentifier : "000";
                options.ClientSecret = !string.IsNullOrWhiteSpace(settings.ClientSecret) ? settings.ClientSecret : "000";
                options.SaveTokens = true;

                //handles exception thrown by external auth provider
                options.Events = new OAuthEvents() {
                    OnRemoteFailure = ctx =>
                    {
                        ctx.HandleResponse();
                        var errorMessage = ctx.Failure.Message;
                        var state = ctx.Request.Query["state"].FirstOrDefault();
                        errorMessage = WebUtility.UrlEncode(errorMessage);
                        ctx.Response.Redirect($"/google-signin-failed?error_message={errorMessage}");

                        return Task.FromResult(0);
                    }
                };
            });

        }
        public int Order => 502;

    }
}
