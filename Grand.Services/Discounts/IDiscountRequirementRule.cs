using System.Threading.Tasks;

namespace Grand.Services.Discounts
{
    /// <summary>
    /// Represents a discount requirement rule
    /// </summary>
    public partial interface IDiscountRequirementRule
    {
        /// <summary>
        /// Check discount requirement
        /// </summary>
        /// <param name="request">Object that contains all information required to check the requirement (Current customer, discount, etc)</param>
        /// <returns>Result</returns>
        Task<DiscountRequirementValidationResult> CheckRequirement(DiscountRequirementValidationRequest request);

        /// <summary>
        /// Get URL for rule configuration
        /// </summary>
        /// <param name="discountId">Discount identifier</param>
        /// <param name="discountRequirementId">Discount requirement identifier (if editing)</param>
        /// <returns>URL</returns>
        string GetConfigurationUrl(string discountId, string discountRequirementId);

        /// <summary>
        /// Gets a system name of type
        /// </summary>
        /// <returns>SystemName</returns>
        string SystemName { get; }

        /// <summary>
        /// Gets a friendly name of type
        /// </summary>
        /// <returns>FriendlyName</returns>
        string FriendlyName { get; }
    }
}
