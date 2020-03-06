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

            //topic service
            builder.RegisterType<TopicViewModelService>().As<ITopicViewModelService>().InstancePerLifetimeScope();

            //customer service
            builder.RegisterType<CustomerViewModelService>().As<ICustomerViewModelService>().InstancePerLifetimeScope();

            //common service
            builder.RegisterType<CommonViewModelService>().As<ICommonViewModelService>().InstancePerLifetimeScope();

            //shipping service 
            builder.RegisterType<ShoppingCartViewModelService>().As<IShoppingCartViewModelService>().InstancePerLifetimeScope();

            //externalAuth service
            builder.RegisterType<ExternalAuthenticationViewModelService>().As<IExternalAuthenticationViewModelService>().InstancePerLifetimeScope();

            //widgetZone servie
            builder.RegisterType<WidgetViewModelService>().As<IWidgetViewModelService>().InstancePerLifetimeScope();

            //checkout service
            builder.RegisterType<CheckoutViewModelService>().As<ICheckoutViewModelService>().InstancePerLifetimeScope();

            //poll service
            builder.RegisterType<PollViewModelService>().As<IPollViewModelService>().InstancePerLifetimeScope();

            //poll service
            builder.RegisterType<BoardsViewModelService>().As<IBoardsViewModelService>().InstancePerLifetimeScope();

            //ReturnRequest service
            builder.RegisterType<ReturnRequestViewModelService>().As<IReturnRequestViewModelService>().InstancePerLifetimeScope();

            //course service
            builder.RegisterType<CourseViewModelService>().As<ICourseViewModelService>().InstancePerLifetimeScope();

        }

        public int Order {
            get { return 2; }
        }
    }
}
