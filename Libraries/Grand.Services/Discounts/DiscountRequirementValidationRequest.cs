using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Stores;

namespace Grand.Services.Discounts
{
    /// <summary>
    /// Represents a request of discount requirement validation
    /// </summary>
    public partial class DiscountRequirementValidationRequest
    {
        /// <summary>
        /// Gets or sets the appropriate discount requirement ID (identifier)
        /// </summary>
        public string DiscountRequirementId { get; set; }

        /// <summary>
        /// Gets or sets the discount ID (identifier)
        /// </summary>
        public string DiscountId { get; set; }

        /// <summary>
        /// Gets or sets the customer
        /// </summary>
        public Customer Customer { get; set; }

        /// <summary>
        /// Gets or sets the store
        /// </summary>
        public Store Store { get; set; }
    }
}
