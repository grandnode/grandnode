using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;
using Grand.Plugin.Shipping.ByWeight.Controllers;
using Grand.Plugin.Shipping.ByWeight.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Grand.Plugin.Shipping.ByWeight
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(IServiceCollection serviceCollection, ITypeFinder typeFinder, GrandConfig config)
        {
            serviceCollection.AddScoped<IShippingByWeightService,ShippingByWeightService>();
            //base shipping controller
            serviceCollection.AddScoped<ShippingByWeightController>();
            serviceCollection.AddScoped<ByWeightShippingComputationMethod>();
        }

        public int Order
        {
            get { return 10; }
        }
    }
}
