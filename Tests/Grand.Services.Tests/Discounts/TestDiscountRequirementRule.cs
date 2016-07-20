using System;
using Grand.Core.Plugins;
using Grand.Services.Discounts;

namespace Grand.Services.Tests.Discounts {
    public partial class TestDiscountRequirementRule : BasePlugin, IDiscountRequirementRule {
        /// <summary>
        /// Check discount requirement
        /// </summary>
        /// <param name="request">Object that contains all information required to check the requirement (Current customer, discount, etc)</param>
        /// <returns>Result</returns>.
        public DiscountRequirementValidationResult CheckRequirement(DiscountRequirementValidationRequest request) {
            return new DiscountRequirementValidationResult {
                IsValid = true
            };
        }

        /// <summary>
        /// Get URL for rule configuration
        /// </summary>
        /// <param name="discountId">Discount identifier</param>
        /// <param name="discountRequirementId">Discount requirement identifier (if editing)</param>
        /// <returns>URL</returns>
        public string GetConfigurationUrl(/*int*/string discountId, /*int?*/string discountRequirementId) {
            throw new NotImplementedException();
        }
    }
}
