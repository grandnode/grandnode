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
            builder.RegisterType<CommonApiService>().As<ICommonApiService>().InstancePerLifetimeScope();
            builder.RegisterType<TokenService>().As<ITokenService>().InstancePerLifetimeScope();
            builder.RegisterType<CategoryApiService>().As<ICategoryApiService>().InstancePerLifetimeScope();
            builder.RegisterType<ManufacturerApiService>().As<IManufacturerApiService>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerRoleApiService>().As<ICustomerRoleApiService>().InstancePerLifetimeScope();
            builder.RegisterType<ProductAttributeApiService>().As<IProductAttributeApiService>().InstancePerLifetimeScope();
            builder.RegisterType<SpecificationAttributeApiService>().As<ISpecificationAttributeApiService>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerApiService>().As<ICustomerApiService>().InstancePerLifetimeScope();
        }
        public int Order => 5;
    }
}
