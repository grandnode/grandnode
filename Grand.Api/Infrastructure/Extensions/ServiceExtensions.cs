using Grand.Core;
using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Services.Customers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Api.Infrastructure.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });
        }
        public static void ConfigureJwt(this IServiceCollection services, ApiConfig config)
        {
            //Token Jwt
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
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
                        OnAuthenticationFailed = context =>
                        {
                            var log = EngineContext.Current.Resolve<Grand.Services.Logging.ILogger>();
                            log.InsertLog(Core.Domain.Logging.LogLevel.Error, "OData - OnAuthenticationFailed", context.Exception.Message);
                            context.Response.StatusCode = 401;
                            context.Response.ContentType = "text/plain";
                            context.Fail(context.Exception.Message);
                            context.Exception = context.Exception;
                            return Task.FromResult(0);
                        },
                        OnTokenValidated = context =>
                        {
                            var customer = context.Principal.Claims.ToList().FirstOrDefault(x => x.Type == "customerGuid").Value;
                            var customerService = EngineContext.Current.Resolve<ICustomerService>();
                            var currentCustomer = customerService.GetCustomerByGuid(new Guid(customer));
                            var workContext = EngineContext.Current.Resolve<IWorkContext>();
                            workContext.CurrentCustomer = currentCustomer;
                            return Task.CompletedTask;
                        },
                    };

                });
        }
    }
}
