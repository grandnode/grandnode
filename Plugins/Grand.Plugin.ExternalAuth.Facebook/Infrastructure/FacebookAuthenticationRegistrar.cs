using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.Extensions.DependencyInjection;
using Grand.Core.Infrastructure;
using Grand.Services.Authentication.External;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Linq;
using System.Net;
using Grand.Core;
using System.Threading.Tasks;

namespace Grand.Plugin.ExternalAuth.Facebook.Infrastructure
{
    /// <summary>
    /// Registration of Facebook authentication service (plugin)
    /// </summary>
    public class FacebookAuthenticationRegistrar : IExternalAuthenticationRegistrar
    {
        /// <summary>
        /// Configure
        /// </summary>
        /// <param name="builder">Authentication builder</param>
        public void Configure(AuthenticationBuilder builder)
        {
            builder.AddFacebook(FacebookDefaults.AuthenticationScheme, options =>
            {
                var settings = EngineContext.Current.Resolve<FacebookExternalAuthSettings>();

                //no empty values allowed. otherwise, an exception could be thrown on application startup
                options.AppId = !String.IsNullOrWhiteSpace(settings.ClientKeyIdentifier) ? settings.ClientKeyIdentifier : "000";
                options.AppSecret = !String.IsNullOrWhiteSpace(settings.ClientSecret) ? settings.ClientSecret : "000";
                options.SaveTokens = true;

                //handles exception thrown by external auth provider
                options.Events = new OAuthEvents()
                {
                    OnRemoteFailure = ctx =>
                    {
                        var errorCode = ctx.Request.Query["error_code"].FirstOrDefault();
                        var errorMessage = ctx.Request.Query["error_message"].FirstOrDefault();
                        var state = ctx.Request.Query["state"].FirstOrDefault();

                        var webHelper = EngineContext.Current.Resolve<IWebHelper>();
                        errorCode = WebUtility.UrlEncode(errorCode);
                        errorMessage = WebUtility.UrlEncode(errorMessage);

                        var urlToRedirect = $"{webHelper.GetStoreLocation()}signin-failed?error_code={errorCode}&error_message={errorMessage}";

                        ctx.Response.Redirect(urlToRedirect);
                        ctx.HandleResponse();

                        return Task.FromResult(0);
                    }
                };
            });
        }

        /// <summary>
        /// Gets order of this registrar implementation
        /// </summary>
        public int Order
        {
            get { return 501; }
        }
    }
}