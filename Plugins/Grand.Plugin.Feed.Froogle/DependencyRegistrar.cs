using Autofac;
using Autofac.Core;
using Grand.Core.Configuration;
using Grand.Core.Data;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;
using Grand.Data;
using Grand.Plugin.Feed.Froogle.Domain;
using Grand.Plugin.Feed.Froogle.Services;
using Grand.Web.Framework.Mvc;

namespace Grand.Plugin.Feed.Froogle
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, GrandConfig config)
        {
            builder.RegisterType<GoogleService>().As<IGoogleService>().InstancePerLifetimeScope();
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
