using Autofac;
using Grand.Api.Interfaces;
using Grand.Api.Services;
using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;

namespace Grand.Api.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, GrandConfig config)
        {
            builder.RegisterType<ProductAttributeApiService>().As<IProductAttributeApiService>().InstancePerLifetimeScope();
        }
        public int Order => 5;
    }
}
