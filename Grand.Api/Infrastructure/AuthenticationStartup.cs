using Grand.Api.Infrastructure.Extensions;
using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Grand.Api.Infrastructure
{
    public partial class AuthenticationStartup : IGrandStartup
    {
        public int Order => 505;

        public void Configure(IApplicationBuilder application)
        {
            
        }

        public void ConfigureServices(IServiceCollection services,
            IConfiguration configuration)
        {
            var apiConfig = services.BuildServiceProvider().GetService<ApiConfig>();

            if (apiConfig.Enabled)
            {
                //cors
                services.ConfigureCors();

                //add authentication bearer
                //services.AddAuthenticationJwtBearer(apiConfig);

                //Add OData
                services.AddOData();
                services.AddODataQueryFilter();
            }
        }

        

    }
}
