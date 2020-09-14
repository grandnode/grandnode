using Autofac;
using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;
using Grand.Plugin.Shipping.ShippingPoint.Services;

namespace Grand.Plugin.Shipping.ShippingPoint
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, GrandConfig config)
        {
            builder.RegisterType<ShippingPointComputationMethod>().InstancePerLifetimeScope();
            builder.RegisterType<ShippingPointService>().As<IShippingPointService>().InstancePerLifetimeScope();
        }

        public int Order
        {
            get { return 10; }
        }
    }
}
