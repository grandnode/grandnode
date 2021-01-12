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
        public virtual void Register(IServiceCollection builder, ITypeFinder typeFinder, GrandConfig config)
        {
            builder.AddScoped<IShippingByWeightService,ShippingByWeightService>();
            //base shipping controller
            builder.AddScoped<ShippingByWeightController>();
            builder.AddScoped<ByWeightShippingComputationMethod>();
        }

        public int Order
        {
            get { return 10; }
        }
    }
}
