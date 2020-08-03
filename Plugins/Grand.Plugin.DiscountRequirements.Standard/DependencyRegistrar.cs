using Autofac;
using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;
using Grand.Plugin.DiscountRequirements.CustomerRoles;
using Grand.Plugin.DiscountRequirements.HasAllProducts;
using Grand.Plugin.DiscountRequirements.HasOneProduct;
using Grand.Plugin.DiscountRequirements.ShoppingCart;
using Grand.Plugin.DiscountRequirements.Standard.HadSpentAmount;
using Grand.Services.Discounts;

namespace Grand.Plugin.DiscountRequirements.Standard
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, GrandConfig config)
        {
            builder.RegisterType<DiscountRequirementsPlugin>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerRoleDiscountRequirementRule>().InstancePerLifetimeScope();
            builder.RegisterType<HadSpentAmountDiscountRequirementRule>().InstancePerLifetimeScope();
            builder.RegisterType<HasAllProductsDiscountRequirementRule>().InstancePerLifetimeScope();
            builder.RegisterType<HasOneProductDiscountRequirementRule>().InstancePerLifetimeScope();
            builder.RegisterType<ShoppingCartDiscountRequirementRule>().InstancePerLifetimeScope();
        }

        public int Order
        {
            get { return 10; }
        }
    }
}
