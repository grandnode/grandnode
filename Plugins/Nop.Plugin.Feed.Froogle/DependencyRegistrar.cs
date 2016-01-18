using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Feed.Froogle.Data;
using Nop.Plugin.Feed.Froogle.Domain;
using Nop.Plugin.Feed.Froogle.Services;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Feed.Froogle
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<GoogleService>().As<IGoogleService>().InstancePerLifetimeScope();
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
