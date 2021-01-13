using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;
using Grand.Plugin.Shipping.ShippingPoint.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Grand.Plugin.Shipping.ShippingPoint
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(IServiceCollection serviceCollection, ITypeFinder typeFinder, GrandConfig config)
        {
            serviceCollection.AddScoped<ShippingPointComputationMethod>();
            serviceCollection.AddScoped<IShippingPointService,ShippingPointService>();
        }

        public int Order
        {
            get { return 10; }
        }
    }
}
