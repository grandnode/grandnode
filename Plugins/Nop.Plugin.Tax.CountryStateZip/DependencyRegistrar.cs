using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Tax.CountryStateZip.Data;
using Nop.Plugin.Tax.CountryStateZip.Domain;
using Nop.Plugin.Tax.CountryStateZip.Services;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Tax.CountryStateZip
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
