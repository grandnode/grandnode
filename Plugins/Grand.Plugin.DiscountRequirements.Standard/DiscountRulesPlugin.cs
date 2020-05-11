using Grand.Core.Plugins;
using Grand.Plugin.DiscountRequirements.CustomerRoles;
using Grand.Plugin.DiscountRequirements.HasAllProducts;
using Grand.Plugin.DiscountRequirements.HasOneProduct;
using Grand.Plugin.DiscountRequirements.ShoppingCart;
using Grand.Plugin.DiscountRequirements.Standard.HadSpentAmount;
using Grand.Services.Discounts;
using Grand.Services.Localization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Plugin.DiscountRequirements.Standard
{
    public partial class DiscountRequirementsPlugin : BasePlugin, IDiscount
    {
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;

        private readonly CustomerRoleDiscountRequirementRule _customerRoleDiscountRequirementRule;
        private readonly HadSpentAmountDiscountRequirementRule _hadSpentAmountDiscountRequirementRule;
        private readonly HasAllProductsDiscountRequirementRule _hasAllProductsDiscountRequirementRule;
        private readonly HasOneProductDiscountRequirementRule _hasOneProductDiscountRequirementRule;
        private readonly ShoppingCartDiscountRequirementRule _shoppingCartDiscountRequirementRule;

        public DiscountRequirementsPlugin(
            ILocalizationService localizationService,
            ILanguageService languageService,
            CustomerRoleDiscountRequirementRule customerRoleDiscountRequirementRule,
            HadSpentAmountDiscountRequirementRule hadSpentAmountDiscountRequirementRule,
            HasAllProductsDiscountRequirementRule hasAllProductsDiscountRequirementRule,
            HasOneProductDiscountRequirementRule hasOneProductDiscountRequirementRule,
            ShoppingCartDiscountRequirementRule shoppingCartDiscountRequirementRule)
        {
            _localizationService = localizationService;
            _languageService = languageService;
            _customerRoleDiscountRequirementRule = customerRoleDiscountRequirementRule;
            _hadSpentAmountDiscountRequirementRule = hadSpentAmountDiscountRequirementRule;
            _hasAllProductsDiscountRequirementRule = hasAllProductsDiscountRequirementRule;
            _hasOneProductDiscountRequirementRule = hasOneProductDiscountRequirementRule;
            _shoppingCartDiscountRequirementRule = shoppingCartDiscountRequirementRule;
        }

        public IList<IDiscountRequirementRule> GetRequirementRules()
        {
            var rules = new List<IDiscountRequirementRule>
            {
                _customerRoleDiscountRequirementRule,
                _hadSpentAmountDiscountRequirementRule,
                _hasAllProductsDiscountRequirementRule,
                _hasOneProductDiscountRequirementRule,
                _shoppingCartDiscountRequirementRule
            };
            return rules;
        }

        public override async Task Install()
        {
            //CustomerRole
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.DiscountRequirements.CustomerRoles.Fields.CustomerRole", "Required customer role");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.DiscountRequirements.CustomerRoles.Fields.CustomerRole.Hint", "Discount will be applied if customer is in the selected customer role.");

            //HadSpentAmount
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.DiscountRequirements.Standard.HadSpentAmount.Fields.Amount", "Required spent amount");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.DiscountRequirements.Standard.HadSpentAmount.Fields.Amount.Hint", "Discount will be applied if customer has spent/purchased x.xx amount.");

            //HasAllProducts
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.DiscountRequirements.HasAllProducts.Fields.Products", "Restricted products");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.DiscountRequirements.HasAllProducts.Fields.Products.Hint", "The comma-separated list of product identifiers (e.g. 77, 123, 156). You can find a product ID on its details page. You can also specify the comma-separated list of product identifiers with quantities ({Product ID}:{Quantity}. for example, 77:1, 123:2, 156:3). And you can also specify the comma-separated list of product identifiers with quantity range ({Product ID}:{Min quantity}-{Max quantity}. for example, 77:1-3, 123:2-5, 156:3-8).");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.DiscountRequirements.HasAllProducts.Fields.Products.AddNew", "Add product");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.DiscountRequirements.HasAllProducts.Fields.Products.Choose", "Choose");

            //HasOneProduct
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.DiscountRequirements.HasOneProduct.Fields.Products", "Restricted products");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.DiscountRequirements.HasOneProduct.Fields.Products.Hint", "The comma-separated list of product identifiers (e.g. 77, 123, 156). You can find a product ID on its details page. You can also specify the comma-separated list of product identifiers with quantities ({Product ID}:{Quantity}. for example, 77:1, 123:2, 156:3). And you can also specify the comma-separated list of product identifiers with quantity range ({Product ID}:{Min quantity}-{Max quantity}. for example, 77:1-3, 123:2-5, 156:3-8).");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.DiscountRequirements.HasOneProduct.Fields.Products.AddNew", "Add product");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.DiscountRequirements.HasOneProduct.Fields.Products.Choose", "Choose");

            //Shipping cart
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.DiscountRules.ShoppingCart.Fields.Amount", "Required spent amount");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.DiscountRules.ShoppingCart.Fields.Amount.Hint", "Discount will be applied if the subtotal  in shopping cart is x.xx.");

            await base.Install();
        }

        public override async Task Uninstall()
        {
            //CustomerRole
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.DiscountRequirements.CustomerRoles.Fields.CustomerRole");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.DiscountRequirements.CustomerRoles.Fields.CustomerRole.Hint");

            //HadSpentAmount
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.DiscountRequirements.Standard.HadSpentAmount.Fields.Amount");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.DiscountRequirements.Standard.HadSpentAmount.Fields.Amount.Hint");

            //HasAllProducts
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.DiscountRequirements.HasAllProducts.Fields.Products");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.DiscountRequirements.HasAllProducts.Fields.Products.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.DiscountRequirements.HasAllProducts.Fields.Products.AddNew");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.DiscountRequirements.HasAllProducts.Fields.Products.Choose");

            //HasOneProduct
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.DiscountRequirements.HasOneProduct.Fields.Products");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.DiscountRequirements.HasOneProduct.Fields.Products.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.DiscountRequirements.HasOneProduct.Fields.Products.AddNew");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.DiscountRequirements.HasOneProduct.Fields.Products.Choose");

            await base.Uninstall();
        }
    }
}