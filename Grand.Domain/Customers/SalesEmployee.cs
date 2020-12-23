namespace Grand.Domain.Customers
{
    public class SalesEmployee : BaseEntity
    {
        /// <summary>
        /// Gets or sets the sales employee name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the sales employee email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the sales employee phone
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets the sales employee active
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets the commission rate
        /// </summary>
        public decimal? Commission { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }
    }
}
