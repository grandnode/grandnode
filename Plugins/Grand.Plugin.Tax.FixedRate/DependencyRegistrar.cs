using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;
using Microsoft.Extensions.DependencyInjection;

namespace Grand.Plugin.Tax.FixedRate
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(IServiceCollection serviceCollection, ITypeFinder typeFinder, GrandConfig config)
        {
            serviceCollection.AddScoped<FixedRateTaxProvider>();
        }

        public int Order
        {
            get { return 10; }
        }
    }

}
