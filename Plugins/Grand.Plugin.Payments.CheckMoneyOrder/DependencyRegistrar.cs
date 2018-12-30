using Autofac;
using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;

namespace Grand.Plugin.Payments.CheckMoneyOrder
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, GrandConfig config)
        {
            builder.RegisterType<CheckMoneyOrderPaymentProcessor>().InstancePerLifetimeScope();
        }

        public int Order
        {
            get { return 10; }
        }
    }

}
