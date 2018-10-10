namespace Grand.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a record to manage product inventory per warehouse
    /// </summary>
    public partial class ProductCombinationWarehouseInventory : SubBaseEntity
    {

        /// <summary>
        /// Gets or sets the warehouse identifier
        /// </summary>
        public string WarehouseId { get; set; }

        /// <summary>
        /// Gets or sets the stock quantity
        /// </summary>
        public int StockQuantity { get; set; }

        /// <summary>
        /// Gets or sets the reserved quantity (ordered but not shipped yet)
        /// </summary>
        public int ReservedQuantity { get; set; }

    }
}
