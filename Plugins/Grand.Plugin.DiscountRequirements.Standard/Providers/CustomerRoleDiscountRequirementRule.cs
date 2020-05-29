using Grand.Services.Configuration;
using Grand.Services.Discounts;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Plugin.DiscountRequirements.CustomerRoles
{
    public partial class CustomerRoleDiscountRequirementRule : IDiscountRequirementRule
    {
        private readonly ISettingService _settingService;

        public CustomerRoleDiscountRequirementRule(ISettingService settingService)
        {
            _settingService = settingService;
        }

        /// <summary>
        /// Check discount requirement
        /// </summary>
        /// <param name="request">Object that contains all information required to check the requirement (Current customer, discount, etc)</param>
        /// <returns>true - requirement is met; otherwise, false</returns>
        public async Task<DiscountRequirementValidationResult> CheckRequirement(DiscountRequirementValidationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            //invalid by default
            var result = new DiscountRequirementValidationResult();

            if (request.Customer == null)
                return result;

            var restrictedToCustomerRoleId = _settingService.GetSettingByKey<string>(string.Format("DiscountRequirements.Standard.MustBeAssignedToCustomerRole-{0}-{1}", request.DiscountId, request.DiscountRequirementId));

            if (String.IsNullOrEmpty(restrictedToCustomerRoleId))
                return result;

            foreach (var customerRole in request.Customer.CustomerRoles.Where(cr => cr.Active).ToList())
                if (restrictedToCustomerRoleId == customerRole.Id)
                {
                    //valid
                    result.IsValid = true;
                    return result;
                }

            return await Task.FromResult(result);
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
            string result = "Admin/CustomerRoles/Configure/?discountId=" + discountId;
            if (!String.IsNullOrEmpty(discountRequirementId))
                result += string.Format("&discountRequirementId={0}", discountRequirementId);
            return result;
        }

        public string FriendlyName => "Must be assigned to customer role";

        public string SystemName => "DiscountRequirements.Standard.MustBeAssignedToCustomerRole";
    }
}