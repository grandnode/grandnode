using Grand.Core;
using Grand.Core.Plugins;
using Grand.Plugin.DiscountRequirements.Standard.HadSpentAmount;
using Grand.Plugin.DiscountRequirements.HasAllProducts;
using Grand.Plugin.DiscountRequirements.HasOneProduct;
using Grand.Services.Configuration;
using Grand.Services.Discounts;
using Grand.Services.Localization;
using Grand.Services.Orders;
using System.Collections.Generic;
using Grand.Plugin.DiscountRequirements.ShoppingCart;
using Grand.Plugin.DiscountRequirements.CustomerRoles;
using System.Threading.Tasks;
using System;

namespace Grand.Plugin.DiscountRequirements.Standard
{
    public partial class DiscountRequirementsPlugin : BasePlugin, IDiscount
    {
        private readonly ISettingService _settingService;
        private readonly IOrderService _orderService;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly IServiceProvider _serviceProvider;

        public DiscountRequirementsPlugin(ISettingService settingService, IOrderService orderService, ILocalizationService localizationService, IWebHelper webHelper,
            IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
            this._settingService = settingService;
            this._orderService = orderService;
            this._localizationService = localizationService;
            this._webHelper = webHelper;
        }

        public IList<IDiscountRequirementRule> GetRequirementRules()
        {
            var rules = new List<IDiscountRequirementRule>
            {
                new CustomerRoleDiscountRequirementRule(_serviceProvider),
                new HadSpentAmountDiscountRequirementRule(_serviceProvider),
                new HasAllProductsDiscountRequirementRule(_serviceProvider),
                new HasOneProductDiscountRequirementRule(_serviceProvider),
                new ShoppingCartDiscountRequirementRule(_serviceProvider)
            };
            return rules;
        }

        public override async Task Install()
        {
            //CustomerRole
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.DiscountRequirements.CustomerRoles.Fields.CustomerRole", "Required customer role");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.DiscountRequirements.CustomerRoles.Fields.CustomerRole.Hint", "Discount will be applied if customer is in the selected customer role.");

            //HadSpentAmount
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.DiscountRequirements.Standard.HadSpentAmount.Fields.Amount", "Required spent amount");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.DiscountRequirements.Standard.HadSpentAmount.Fields.Amount.Hint", "Discount will be applied if customer has spent/purchased x.xx amount.");

            //HasAllProducts
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.DiscountRequirements.HasAllProducts.Fields.Products", "Restricted products");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.DiscountRequirements.HasAllProducts.Fields.Products.Hint", "The comma-separated list of product identifiers (e.g. 77, 123, 156). You can find a product ID on its details page. You can also specify the comma-separated list of product identifiers with quantities ({Product ID}:{Quantity}. for example, 77:1, 123:2, 156:3). And you can also specify the comma-separated list of product identifiers with quantity range ({Product ID}:{Min quantity}-{Max quantity}. for example, 77:1-3, 123:2-5, 156:3-8).");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.DiscountRequirements.HasAllProducts.Fields.Products.AddNew", "Add product");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.DiscountRequirements.HasAllProducts.Fields.Products.Choose", "Choose");

            //HasOneProduct
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.DiscountRequirements.HasOneProduct.Fields.Products", "Restricted products");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.DiscountRequirements.HasOneProduct.Fields.Products.Hint", "The comma-separated list of product identifiers (e.g. 77, 123, 156). You can find a product ID on its details page. You can also specify the comma-separated list of product identifiers with quantities ({Product ID}:{Quantity}. for example, 77:1, 123:2, 156:3). And you can also specify the comma-separated list of product identifiers with quantity range ({Product ID}:{Min quantity}-{Max quantity}. for example, 77:1-3, 123:2-5, 156:3-8).");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.DiscountRequirements.HasOneProduct.Fields.Products.AddNew", "Add product");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.DiscountRequirements.HasOneProduct.Fields.Products.Choose", "Choose");

            //Shipping cart
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.DiscountRules.ShoppingCart.Fields.Amount", "Required spent amount");
            await this.AddOrUpdatePluginLocaleResource(_serviceProvider, "Plugins.DiscountRules.ShoppingCart.Fields.Amount.Hint", "Discount will be applied if the subtotal  in shopping cart is x.xx.");

            await base.Install();
        }

        public override async Task Uninstall()
        {
            //CustomerRole
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.DiscountRequirements.CustomerRoles.Fields.CustomerRole");
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.DiscountRequirements.CustomerRoles.Fields.CustomerRole.Hint");

            //HadSpentAmount
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.DiscountRequirements.Standard.HadSpentAmount.Fields.Amount");
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.DiscountRequirements.Standard.HadSpentAmount.Fields.Amount.Hint");

            //HasAllProducts
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.DiscountRequirements.HasAllProducts.Fields.Products");
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.DiscountRequirements.HasAllProducts.Fields.Products.Hint");
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.DiscountRequirements.HasAllProducts.Fields.Products.AddNew");
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.DiscountRequirements.HasAllProducts.Fields.Products.Choose");

            //HasOneProduct
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.DiscountRequirements.HasOneProduct.Fields.Products");
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.DiscountRequirements.HasOneProduct.Fields.Products.Hint");
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.DiscountRequirements.HasOneProduct.Fields.Products.AddNew");
            await this.DeletePluginLocaleResource(_serviceProvider, "Plugins.DiscountRequirements.HasOneProduct.Fields.Products.Choose");
            await base.Uninstall();
        }
    }
}