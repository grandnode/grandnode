using Grand.Api.Infrastructure.Extensions;
using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Services.Authentication;
using Grand.Services.Authentication.External;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Threading.Tasks;

namespace Grand.Api.Infrastructure
{
    public partial class ApiAuthenticationRegistrar : IExternalAuthenticationRegistrar
    {
        public void Configure(AuthenticationBuilder builder, IConfiguration configuration)
        {
            builder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var config = new ApiConfig();
                configuration.GetSection("Api").Bind(config);
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = config.ValidateIssuer,
                    ValidateAudience = config.ValidateAudience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config.ValidIssuer,
                    ValidAudience = config.ValidAudience,
                    IssuerSigningKey = JwtSecurityKey.Create(config.SecretKey)
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = async context =>
                    {
                        context.NoResult();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "text/plain";
                        await context.Response.WriteAsync(context.Exception.Message);
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
                            var apiAuthenticationService = context.HttpContext.RequestServices.GetRequiredService<IApiAuthenticationService>();
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
