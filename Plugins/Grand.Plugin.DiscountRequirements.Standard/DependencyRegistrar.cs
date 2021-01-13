using Grand.Core.Configuration;
using Grand.Core.Infrastructure;
using Grand.Core.Infrastructure.DependencyManagement;
using Grand.Plugin.DiscountRequirements.CustomerRoles;
using Grand.Plugin.DiscountRequirements.HasAllProducts;
using Grand.Plugin.DiscountRequirements.HasOneProduct;
using Grand.Plugin.DiscountRequirements.ShoppingCart;
using Grand.Plugin.DiscountRequirements.Standard.HadSpentAmount;
using Grand.Services.Discounts;
using Microsoft.Extensions.DependencyInjection;

namespace Grand.Plugin.DiscountRequirements.Standard
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(IServiceCollection builder, ITypeFinder typeFinder, GrandConfig config)
        {
            builder.AddScoped<DiscountRequirementsPlugin>();
            builder.AddScoped<CustomerRoleDiscountRequirementRule>();
            builder.AddScoped<HadSpentAmountDiscountRequirementRule>();
            builder.AddScoped<HasAllProductsDiscountRequirementRule>();
            builder.AddScoped<HasOneProductDiscountRequirementRule>();
            builder.AddScoped<ShoppingCartDiscountRequirementRule>();
        }

        public int Order
        {
            get { return 10; }
        }
    }
}
