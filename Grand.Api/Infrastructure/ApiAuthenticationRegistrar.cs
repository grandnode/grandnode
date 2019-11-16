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
using System;
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
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "text/plain";
                        context.Response.WriteAsync(context.Exception.Message).Wait();
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async context =>
                    {
                        try
                        {
                            var apiAuthenticationService = EngineContext.Current.Resolve<IApiAuthenticationService>();
                            if (await apiAuthenticationService.Valid(context))
                                await apiAuthenticationService.SignIn();
                            else
                                throw new Exception(await apiAuthenticationService.ErrorMessage());
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }
                    },
                };
            });

        }
        public int Order => 0;
    }
}
