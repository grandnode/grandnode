using Grand.Core;
using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Services.Customers;
using Grand.Services.Stores;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
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
        public static void AddAuthenticationJwtBearer(this IServiceCollection services, ApiConfig config)
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
                            var email = context.Principal.Claims.ToList().FirstOrDefault(x => x.Type == "Email")?.Value;
                            if (!string.IsNullOrEmpty(email))
                            {
                                var customerService = EngineContext.Current.Resolve<ICustomerService>();
                                var workContext = EngineContext.Current.Resolve<IWorkContext>();
                                var currentCustomer = customerService.GetCustomerByEmail(email);
                                if (currentCustomer != null)
                                    workContext.CurrentCustomer = currentCustomer;
                            }
                            var storeId = context.Principal.Claims.ToList().FirstOrDefault(x => x.Type == "StoreId")?.Value;
                            if (!string.IsNullOrEmpty(storeId))
                            {
                                var storeService = EngineContext.Current.Resolve<IStoreService>();
                                var storeContext = EngineContext.Current.Resolve<IStoreContext>();
                                var store = storeService.GetStoreById(storeId);
                                if (store != null)
                                    storeContext.CurrentStore = store;
                            }
                            return Task.CompletedTask;
                        },
                    };
                });
        }
    }
}
