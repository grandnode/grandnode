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
            builder.RegisterType<TokenService>().As<ITokenService>().InstancePerLifetimeScope();

            builder.RegisterType<CommonApiService>().As<ICommonApiService>().InstancePerLifetimeScope();
            builder.RegisterType<CategoryApiService>().As<ICategoryApiService>().InstancePerLifetimeScope();
            builder.RegisterType<ProductApiService>().As<IProductApiService>().InstancePerLifetimeScope();
            builder.RegisterType<ManufacturerApiService>().As<IManufacturerApiService>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerRoleApiService>().As<ICustomerRoleApiService>().InstancePerLifetimeScope();
            builder.RegisterType<ProductAttributeApiService>().As<IProductAttributeApiService>().InstancePerLifetimeScope();
            builder.RegisterType<SpecificationAttributeApiService>().As<ISpecificationAttributeApiService>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerApiService>().As<ICustomerApiService>().InstancePerLifetimeScope();
            builder.RegisterType<ShippingApiService>().As<IShippingApiService>().InstancePerLifetimeScope();
        }
        public int Order => 5;
    }
}
