using Autofac;
using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;
using Grand.Plugin.Tax.CountryStateZip.Services;

namespace Grand.Plugin.Tax.CountryStateZip.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, GrandConfig config)
        {
            builder.RegisterType<TaxRateService>().As<ITaxRateService>().InstancePerLifetimeScope();
            builder.RegisterType<CountryStateZipTaxProvider>().InstancePerLifetimeScope();
        }

        public int Order
        {
            get { return 20; }
        }
    }
}
