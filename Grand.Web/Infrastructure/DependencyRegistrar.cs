using Autofac;
using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;
using Grand.Framework.Controllers;
using Grand.Web.Infrastructure.Installation;
using Grand.Web.Services;

namespace Grand.Web.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, GrandConfig config)
        {
            //installation localization service
            builder.RegisterType<InstallationLocalizationService>().As<IInstallationLocalizationService>().InstancePerLifetimeScope();

            //blog service
            builder.RegisterType<BlogWebService>().As<IBlogWebService>().InstancePerLifetimeScope();

            //address service
            builder.RegisterType<AddressWebService>().As<IAddressWebService>().InstancePerLifetimeScope();

            //catalog service
            builder.RegisterType<CatalogWebService>().As<ICatalogWebService>().InstancePerLifetimeScope();

            //product service
            builder.RegisterType<ProductWebService>().As<IProductWebService>().InstancePerLifetimeScope();

            //news service
            builder.RegisterType<NewsWebService>().As<INewsWebService>().InstancePerLifetimeScope();

            //topic service
            builder.RegisterType<TopicWebService>().As<ITopicWebService>().InstancePerLifetimeScope();

            //customer service
            builder.RegisterType<CustomerWebService>().As<ICustomerWebService>().InstancePerLifetimeScope();

            //common service
            builder.RegisterType<CommonWebService>().As<ICommonWebService>().InstancePerLifetimeScope();

            //shipping service 
            builder.RegisterType<ShoppingCartWebService>().As<IShoppingCartWebService>().InstancePerLifetimeScope();

            //externalAuth service
            builder.RegisterType<ExternalAuthenticationWebService>().As<IExternalAuthenticationWebService>().InstancePerLifetimeScope();

            //widgetZone servie
            builder.RegisterType<WidgetWebService>().As<IWidgetWebService>().InstancePerLifetimeScope();

            //order service
            builder.RegisterType<OrderWebService>().As<IOrderWebService>().InstancePerLifetimeScope();

            //country service
            builder.RegisterType<CountryWebService>().As<ICountryWebService>().InstancePerLifetimeScope();

            //checkout service
            builder.RegisterType<CheckoutWebService>().As<ICheckoutWebService>().InstancePerLifetimeScope();

            //poll service
            builder.RegisterType<PollWebService>().As<IPollWebService>().InstancePerLifetimeScope();

            //poll service
            builder.RegisterType<BoardsWebService>().As<IBoardsWebService>().InstancePerLifetimeScope();

            //ReturnRequest service
            builder.RegisterType<ReturnRequestWebService>().As<IReturnRequestWebService>().InstancePerLifetimeScope();

            //Newsletter service
            builder.RegisterType<NewsletterWebService>().As<INewsletterWebService>().InstancePerLifetimeScope();

            //vendor service
            builder.RegisterType<VendorWebService>().As<IVendorWebService>().InstancePerLifetimeScope();
            //base shipping controller
            //builder.RegisterType<BaseShippingController>();
        }

        public int Order
        {
            get { return 2; }
        }
    }
}
