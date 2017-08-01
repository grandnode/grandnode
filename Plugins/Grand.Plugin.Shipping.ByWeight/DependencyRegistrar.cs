using Autofac;
using Autofac.Core;
using Grand.Core.Configuration;
using Grand.Core.Data;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;
using Grand.Data;
using Grand.Plugin.Shipping.ByWeight.Controllers;
using Grand.Plugin.Shipping.ByWeight.Domain;
using Grand.Plugin.Shipping.ByWeight.Services;

namespace Grand.Plugin.Shipping.ByWeight
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, GrandConfig config)
        {
            builder.RegisterType<ShippingByWeightService>().As<IShippingByWeightService>().InstancePerLifetimeScope();
            //base shipping controller
            builder.RegisterType<ShippingByWeightController>();
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
