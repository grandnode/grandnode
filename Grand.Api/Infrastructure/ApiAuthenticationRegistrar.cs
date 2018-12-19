using Grand.Api.Infrastructure.Extensions;
using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Services.Authentication;
using Grand.Services.Authentication.External;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Api.Infrastructure
{
    public partial class ApiAuthenticationRegistrar : IExternalAuthenticationRegistrar
    {
        public void Configure(AuthenticationBuilder builder)
        {
            builder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var apiConfig = EngineContext.Current.Resolve<ApiConfig>();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = apiConfig.ValidateIssuer,
                    ValidateAudience = apiConfig.ValidateAudience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = apiConfig.ValidIssuer,
                    ValidAudience = apiConfig.ValidAudience,
                    IssuerSigningKey = JwtSecurityKey.Create(apiConfig.SecretKey)
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        context.NoResult();
                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "text/plain";
                        context.Response.WriteAsync(context.Exception.Message).Wait();
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var apiauthenticationService = EngineContext.Current.Resolve<IApiAuthenticationService>();
                        var email = context.Principal.Claims.ToList().FirstOrDefault(x => x.Type == "Email")?.Value;
                        if (!string.IsNullOrEmpty(email))
                        {
                            //authenticate
                            apiauthenticationService.SignIn(email);
                        }
                        return Task.CompletedTask;
                    },
                };
            });

        }
        public int Order => 0;
    }
}
