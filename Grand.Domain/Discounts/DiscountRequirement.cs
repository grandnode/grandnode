namespace Grand.Domain.Discounts
{
    /// <summary>
    /// Represents a discount requirement
    /// </summary>
    public partial class DiscountRequirement : SubBaseEntity
    {
        /// <summary>
        /// Gets or sets the discount identifier
        /// </summary>
        public string DiscountId { get; set; }
        
        /// <summary>
        /// Gets or sets the discount requirement rule system name
        /// </summary>
        public string DiscountRequirementRuleSystemName { get; set; }
        
    }
}
