namespace Grand.Domain.Customers
{
   
    /// <summary>
    /// Represents a customer tag product
    /// </summary>
    public partial class CustomerTagProduct : BaseEntity
    {

        /// <summary>
        /// Gets or sets the customer tag id
        /// </summary>
        public string CustomerTagId { get; set; }

        /// <summary>
        /// Gets or sets the product Id
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }

    }

}