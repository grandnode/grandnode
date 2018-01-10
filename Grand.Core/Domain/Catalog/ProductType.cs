namespace Grand.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product type
    /// </summary>
    public enum ProductType
    {
        /// <summary>
        /// Simple
        /// </summary>
        SimpleProduct = 5,
        /// <summary>
        /// Grouped (product with variants)
        /// </summary>
        GroupedProduct = 10,

        /// <summary>
        /// Reservation product
        /// </summary>
        Reservation = 15,
    }
}
