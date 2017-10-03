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

namespace Grand.Plugin.DiscountRequirements.CustomerRoles
{
    public partial class DiscountRequirementsPlugin : BasePlugin, IDiscount
    {
        private readonly ISettingService _settingService;
        private readonly IOrderService _orderService;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;

        public DiscountRequirementsPlugin(ISettingService settingService, IOrderService orderService, ILocalizationService localizationService, IWebHelper webHelper)
        {
            this._settingService = settingService;
            this._orderService = orderService;
            this._localizationService = localizationService;
            this._webHelper = webHelper;
        }

        public IList<IDiscountRequirementRule> GetRequirementRules()
        {
            var rules = new List<IDiscountRequirementRule>
            {
                new CustomerRoleDiscountRequirementRule(_settingService),
                new HadSpentAmountDiscountRequirementRule(_settingService,_orderService,_localizationService,_webHelper),
                new HasAllProductsDiscountRequirementRule(_settingService),
                new HasOneProductDiscountRequirementRule(_settingService),
            };
            return rules;
        }

        public override void Install()
        {
            //CustomerRole
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRequirements.CustomerRoles.Fields.CustomerRole", "Required customer role");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRequirements.CustomerRoles.Fields.CustomerRole.Hint", "Discount will be applied if customer is in the selected customer role.");

            //HadSpentAmount
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRequirements.Standard.HadSpentAmount.Fields.Amount", "Required spent amount");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRequirements.Standard.HadSpentAmount.Fields.Amount.Hint", "Discount will be applied if customer has spent/purchased x.xx amount.");

            //HasAllProducts
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRequirements.HasAllProducts.Fields.Products", "Restricted products");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRequirements.HasAllProducts.Fields.Products.Hint", "The comma-separated list of product identifiers (e.g. 77, 123, 156). You can find a product ID on its details page. You can also specify the comma-separated list of product identifiers with quantities ({Product ID}:{Quantity}. for example, 77:1, 123:2, 156:3). And you can also specify the comma-separated list of product identifiers with quantity range ({Product ID}:{Min quantity}-{Max quantity}. for example, 77:1-3, 123:2-5, 156:3-8).");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRequirements.HasAllProducts.Fields.Products.AddNew", "Add product");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRequirements.HasAllProducts.Fields.Products.Choose", "Choose");

            //HasOneProduct
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRequirements.HasOneProduct.Fields.Products", "Restricted products");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRequirements.HasOneProduct.Fields.Products.Hint", "The comma-separated list of product identifiers (e.g. 77, 123, 156). You can find a product ID on its details page. You can also specify the comma-separated list of product identifiers with quantities ({Product ID}:{Quantity}. for example, 77:1, 123:2, 156:3). And you can also specify the comma-separated list of product identifiers with quantity range ({Product ID}:{Min quantity}-{Max quantity}. for example, 77:1-3, 123:2-5, 156:3-8).");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRequirements.HasOneProduct.Fields.Products.AddNew", "Add product");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRequirements.HasOneProduct.Fields.Products.Choose", "Choose");
            base.Install();
        }

        public override void Uninstall()
        {
            //CustomerRole
            this.DeletePluginLocaleResource("Plugins.DiscountRequirements.CustomerRoles.Fields.CustomerRole");
            this.DeletePluginLocaleResource("Plugins.DiscountRequirements.CustomerRoles.Fields.CustomerRole.Hint");

            //HadSpentAmount
            this.DeletePluginLocaleResource("Plugins.DiscountRequirements.Standard.HadSpentAmount.Fields.Amount");
            this.DeletePluginLocaleResource("Plugins.DiscountRequirements.Standard.HadSpentAmount.Fields.Amount.Hint");

            //HasAllProducts
            this.DeletePluginLocaleResource("Plugins.DiscountRequirements.HasAllProducts.Fields.Products");
            this.DeletePluginLocaleResource("Plugins.DiscountRequirements.HasAllProducts.Fields.Products.Hint");
            this.DeletePluginLocaleResource("Plugins.DiscountRequirements.HasAllProducts.Fields.Products.AddNew");
            this.DeletePluginLocaleResource("Plugins.DiscountRequirements.HasAllProducts.Fields.Products.Choose");

            //HasOneProduct
            this.DeletePluginLocaleResource("Plugins.DiscountRequirements.HasOneProduct.Fields.Products");
            this.DeletePluginLocaleResource("Plugins.DiscountRequirements.HasOneProduct.Fields.Products.Hint");
            this.DeletePluginLocaleResource("Plugins.DiscountRequirements.HasOneProduct.Fields.Products.AddNew");
            this.DeletePluginLocaleResource("Plugins.DiscountRequirements.HasOneProduct.Fields.Products.Choose");
            base.Uninstall();
        }
    }
}