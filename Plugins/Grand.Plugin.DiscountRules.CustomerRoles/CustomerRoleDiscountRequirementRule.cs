using Grand.Core.Plugins;
using Grand.Services.Configuration;
using Grand.Services.Discounts;
using Grand.Services.Localization;
using System;
using System.Linq;

namespace Grand.Plugin.DiscountRules.CustomerRoles
{
    public partial class CustomerRoleDiscountRequirementRule : BasePlugin, IDiscountRequirementRule
    {
        private readonly ISettingService _settingService;

        public CustomerRoleDiscountRequirementRule(ISettingService settingService)
        {
            this._settingService = settingService;
        }

        /// <summary>
        /// Check discount requirement
        /// </summary>
        /// <param name="request">Object that contains all information required to check the requirement (Current customer, discount, etc)</param>
        /// <returns>true - requirement is met; otherwise, false</returns>
        public DiscountRequirementValidationResult CheckRequirement(DiscountRequirementValidationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            //invalid by default
            var result = new DiscountRequirementValidationResult();

            if (request.Customer == null)
                return result;


            var restrictedToCustomerRoleId = _settingService.GetSettingByKey<string>(string.Format("DiscountRequirement.MustBeAssignedToCustomerRole-{0}-{1}", request.DiscountId, request.DiscountRequirementId));

            if (String.IsNullOrEmpty(restrictedToCustomerRoleId))
                return result;

            foreach (var customerRole in request.Customer.CustomerRoles.Where(cr => cr.Active).ToList())
                if (restrictedToCustomerRoleId == customerRole.Id)
                {
                    //valid
                    result.IsValid = true;
                    return result;
                }

            return result;
        }

        /// <summary>
        /// Get URL for rule configuration
        /// </summary>
        /// <param name="discountId">Discount identifier</param>
        /// <param name="discountRequirementId">Discount requirement identifier (if editing)</param>
        /// <returns>URL</returns>
        public string GetConfigurationUrl(string discountId, string discountRequirementId)
        {
            //configured in RouteProvider.cs
            string result = "Plugins/DiscountRulesCustomerRoles/Configure/?discountId=" + discountId;
            if (!String.IsNullOrEmpty(discountRequirementId))
                result += string.Format("&discountRequirementId={0}", discountRequirementId);
            return result;
        }

        public override void Install()
        {
            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.CustomerRoles.Fields.CustomerRole", "Required customer role");
            this.AddOrUpdatePluginLocaleResource("Plugins.DiscountRules.CustomerRoles.Fields.CustomerRole.Hint", "Discount will be applied if customer is in the selected customer role.");
            base.Install();
        }

        public override void Uninstall()
        {
            //locales
            this.DeletePluginLocaleResource("Plugins.DiscountRules.CustomerRoles.Fields.CustomerRole");
            this.DeletePluginLocaleResource("Plugins.DiscountRules.CustomerRoles.Fields.CustomerRole.Hint");
            base.Uninstall();
        }
    }
}