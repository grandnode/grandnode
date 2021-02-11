using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Core.DependencyInjection;
using Grand.Plugin.DiscountRequirements.CustomerRoles;
using Grand.Plugin.DiscountRequirements.HasAllProducts;
using Grand.Plugin.DiscountRequirements.HasOneProduct;
using Grand.Plugin.DiscountRequirements.ShoppingCart;
using Grand.Plugin.DiscountRequirements.Standard.HadSpentAmount;
using Microsoft.Extensions.DependencyInjection;

namespace Grand.Plugin.DiscountRequirements.Standard
{
    public class DependencyInjection : IDependencyInjection
    {
        public virtual void Register(IServiceCollection serviceCollection, ITypeFinder typeFinder, GrandConfig config)
        {
            serviceCollection.AddScoped<DiscountRequirementsPlugin>();
            serviceCollection.AddScoped<CustomerRoleDiscountRequirementRule>();
            serviceCollection.AddScoped<HadSpentAmountDiscountRequirementRule>();
            serviceCollection.AddScoped<HasAllProductsDiscountRequirementRule>();
            serviceCollection.AddScoped<HasOneProductDiscountRequirementRule>();
            serviceCollection.AddScoped<ShoppingCartDiscountRequirementRule>();
        }

        public int Order {
            get { return 10; }
        }
    }
}
