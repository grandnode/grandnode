using Autofac;
using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;
using Grand.Services.Tax;

namespace Grand.Plugin.Misc.EuropaCheckVat
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, GrandConfig config)
        {
            builder.RegisterType<EuropaCheckVatPlugin>().InstancePerLifetimeScope();
            builder.RegisterType<EuropaVatService>().As<IVatService>().InstancePerLifetimeScope();
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
