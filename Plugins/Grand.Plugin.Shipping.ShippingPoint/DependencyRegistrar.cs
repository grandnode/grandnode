using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;
using Grand.Plugin.Shipping.ShippingPoint.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Grand.Plugin.Shipping.ShippingPoint
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(IServiceCollection builder, ITypeFinder typeFinder, GrandConfig config)
        {
            builder.AddScoped<ShippingPointComputationMethod>();
            builder.AddScoped<IShippingPointService,ShippingPointService>();
        }

        public int Order
        {
            get { return 10; }
        }
    }
}
