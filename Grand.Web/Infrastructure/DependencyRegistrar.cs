using Autofac;
using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;
using Grand.Web.Infrastructure.Installation;
using Grand.Web.Interfaces;
using Grand.Web.Services;

namespace Grand.Web.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, GrandConfig config)
        {
            //installation localization service
            builder.RegisterType<InstallationLocalizationService>().As<IInstallationLocalizationService>().InstancePerLifetimeScope();

            //address service
            builder.RegisterType<AddressViewModelService>().As<IAddressViewModelService>().InstancePerLifetimeScope();

            //catalog service
            builder.RegisterType<CatalogViewModelService>().As<ICatalogViewModelService>().InstancePerLifetimeScope();

            //product service
            builder.RegisterType<ProductViewModelService>().As<IProductViewModelService>().InstancePerLifetimeScope();

            //customer custom attributes service
            builder.RegisterType<CustomerCustomAttributes>().As<ICustomerCustomAttributes>().InstancePerLifetimeScope();           

            //widgetZone servie
            builder.RegisterType<WidgetViewModelService>().As<IWidgetViewModelService>().InstancePerLifetimeScope();

            //board service
            builder.RegisterType<BoardsViewModelService>().As<IBoardsViewModelService>().InstancePerLifetimeScope();

        }

        public int Order {
            get { return 2; }
        }
    }
}
