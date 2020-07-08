namespace Grand.Domain.Catalog
{
    /// <summary>
    /// Represents a customer reservation helper to the product
    /// </summary>
    public partial class CustomerReservationsHelper : BaseEntity
    {
        /// <summary>
        /// Gets or sets customer identifier
        /// </summary>
        public string CustomerId { get; set; }
        /// <summary>
        /// Gets or sets shopping cart identifier
        /// </summary>
        public string ShoppingCartItemId { get; set; }
        /// <summary>
        /// Gets or sets reservation identifier
        /// </summary>
        public string ReservationId { get; set; }
    }
}
