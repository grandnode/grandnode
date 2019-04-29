using Autofac;
using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;
using Grand.Plugin.ExchangeRate.McExchange;

namespace Grand.Plugin.ExchangeRate.EcbExchange
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, GrandConfig config)
        {
            builder.RegisterType<McExchangeRateProvider>().InstancePerLifetimeScope();
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
