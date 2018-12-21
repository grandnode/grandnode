using Autofac;
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
            builder.RegisterType<TokenService>().As<ITokenService>().InstancePerLifetimeScope();
            builder.RegisterType<CategoryApiService>().As<ICategoryApiService>().InstancePerLifetimeScope();
            builder.RegisterType<ManufacturerApiService>().As<IManufacturerApiService>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerRoleApiService>().As<ICustomerRoleApiService>().InstancePerLifetimeScope();
        }
        public int Order => 5;
    }
}
