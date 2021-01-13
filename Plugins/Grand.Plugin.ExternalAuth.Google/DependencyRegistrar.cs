using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;
using Microsoft.Extensions.DependencyInjection;

namespace Grand.Plugin.ExternalAuth.Google
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(IServiceCollection serviceCollection, ITypeFinder typeFinder, GrandConfig config)
        {
            serviceCollection.AddScoped<GoogleAuthenticationMethod>();
        }

        public int Order
        {
            get { return 10; }
        }
    }

}
