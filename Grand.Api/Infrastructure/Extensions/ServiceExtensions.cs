using Grand.Api.Services;
using Grand.Core;
using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Services.Authentication;
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
    }
}
