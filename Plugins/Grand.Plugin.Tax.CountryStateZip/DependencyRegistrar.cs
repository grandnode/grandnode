using Autofac;
using Autofac.Core;
using Grand.Core.Configuration;
using Grand.Core.Data;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;
using Grand.Data;
using Grand.Plugin.Tax.CountryStateZip.Domain;
using Grand.Plugin.Tax.CountryStateZip.Services;
using Grand.Web.Framework.Mvc;

namespace Grand.Plugin.Tax.CountryStateZip
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<TaxRateService>().As<ITaxRateService>().InstancePerLifetimeScope();

        }

        public int Order
        {
            get { return 1; }
        }
    }
}
