using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Grand.Plugin.Tax.FixedRate
{
    public class DependencyInjection : IDependencyInjection
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
