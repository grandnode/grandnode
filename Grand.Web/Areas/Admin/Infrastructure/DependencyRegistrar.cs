using Autofac;
using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;
using Grand.Web.Areas.Admin.Services;

namespace Grand.Web.Areas.Admin.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, GrandConfig config)
        {
            builder.RegisterType<ActivityLogViewModelService>().As<IActivityLogViewModelService>().InstancePerLifetimeScope();
            builder.RegisterType<AddressAttributeViewModelService>().As<IAddressAttributeViewModelService>().InstancePerLifetimeScope();
            builder.RegisterType<AffiliateViewModelService>().As<IAffiliateViewModelService>().InstancePerLifetimeScope();
            builder.RegisterType<BlogViewModelService>().As<IBlogViewModelService>().InstancePerLifetimeScope();
            builder.RegisterType<CampaignViewModelService>().As<ICampaignViewModelService>().InstancePerLifetimeScope();
            builder.RegisterType<CategoryViewModelService>().As<ICategoryViewModelService>().InstancePerLifetimeScope();
            builder.RegisterType<CheckoutAttributeViewModelService>().As<ICheckoutAttributeViewModelService>().InstancePerLifetimeScope();
        }

        public int Order
        {
            get { return 3; }
        }
    }
}
