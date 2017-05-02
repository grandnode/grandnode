namespace Grand.Core.Domain.Customers
{
    /// <summary>
    /// Represents a customer role
    /// </summary>
    public partial class CustomerRole : BaseEntity
    {

        /// <summary>
        /// Gets or sets the customer role name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the customer Id
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer role is marked as free shiping
        /// </summary>
        public bool FreeShipping { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer role is marked as tax exempt
        /// </summary>
        public bool TaxExempt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer role is active
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer role is system
        /// </summary>
        public bool IsSystemRole { get; set; }

        /// <summary>
        /// Gets or sets the customer role system name
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customers must change passwords after a specified time
        /// </summary>
        public bool EnablePasswordLifetime { get; set; }
        /// <summary>
        /// Gets or sets a product identifier that is required by this customer role. 
        /// A customer is added to this customer role once a specified product is purchased.
        /// </summary>
        public string PurchasedWithProductId { get; set; }
        
    }

}