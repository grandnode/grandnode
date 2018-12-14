using Grand.Api.Infrastructure.DependencyManagement;
using Grand.Api.Infrastructure.Extensions;
using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using System;
using System.Linq;

namespace Grand.Api.Infrastructure
{
    public partial class AuthenticationStartup : IGrandStartup
    {
        public int Order => 501;

        public void Configure(IApplicationBuilder application)
        {
            var apiConfig = application.ApplicationServices.GetService<ApiConfig>();
            if (apiConfig.Enabled)
            {
                //OData
                IEdmModel model = GetEdmModel(application.ApplicationServices, apiConfig);
                application.UseMvc(routeBuilder =>
                {
                    routeBuilder.Count().Filter().OrderBy().Expand().Select().MaxTop(null);
                    routeBuilder.MapODataServiceRoute("ODataRoute", "api", model);
                    routeBuilder.EnableDependencyInjection();
                });
            }
        }

        public void ConfigureServices(IServiceCollection services,
            IConfiguration configuration)
        {
            var apiConfig = services.BuildServiceProvider().GetService<ApiConfig>();

            if (apiConfig.Enabled)
            {
                //cors
                services.ConfigureCors();
                //Jwt token
                services.ConfigureJwt(apiConfig);
                //OData
                services.AddOData();
                services.AddODataQueryFilter();
            }
        }

        private IEdmModel GetEdmModel(IServiceProvider serviceProvider, ApiConfig apiConfig)
        {
            var builder = new ODataConventionModelBuilder(serviceProvider);
            builder.Namespace = "Default";
            RegisterDependencies(builder, apiConfig);
            return builder.GetEdmModel();
        }

        private void RegisterDependencies(ODataConventionModelBuilder builder, ApiConfig apiConfig)
        {
            var typeFinder = new WebAppTypeFinder();

            //find dependency registrars provided by other assemblies
            var dependencyRegistrars = typeFinder.FindClassesOfType<IDependencyEdmModel>();

            //create and sort instances of dependency registrars
            var instances = dependencyRegistrars
                .Select(dependencyRegistrar => (IDependencyEdmModel)Activator.CreateInstance(dependencyRegistrar))
                .OrderBy(dependencyRegistrar => dependencyRegistrar.Order);

            //register all provided dependencies
            foreach (var dependencyRegistrar in instances)
                dependencyRegistrar.Register(builder, apiConfig);

        }

    }
}
