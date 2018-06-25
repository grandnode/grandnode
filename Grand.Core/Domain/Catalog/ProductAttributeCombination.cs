
using System.Collections.Generic;

namespace Grand.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product attribute combination
    /// </summary>
    public partial class ProductAttributeCombination : SubBaseEntity
    {
        private ICollection<ProductCombinationWarehouseInventory> _warehouseInventory;
        private ICollection<ProductCombinationTierPrices> _tierPrices;

        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// Gets or sets the attributes
        /// </summary>
        public string AttributesXml { get; set; }

        /// <summary>
        /// Gets or sets the stock quantity
        /// </summary>
        public int StockQuantity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to allow orders when out of stock
        /// </summary>
        public bool AllowOutOfStockOrders { get; set; }

        /// <summary>
        /// Gets or sets the text
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the SKU
        /// </summary>
        public string Sku { get; set; }

        /// <summary>
        /// Gets or sets the manufacturer part number
        /// </summary>
        public string ManufacturerPartNumber { get; set; }

        /// <summary>
        /// Gets or sets the Global Trade Item Number (GTIN). These identifiers include UPC (in North America), EAN (in Europe), JAN (in Japan), and ISBN (for books).
        /// </summary>
        public string Gtin { get; set; }

        /// <summary>
        /// Gets or sets the attribute combination price. This way a store owner can override the default product price when this attribute combination is added to the cart. For example, you can give a discount this way.
        /// </summary>
        public decimal? OverriddenPrice { get; set; }

        /// <summary>
        /// Gets or sets the quantity when admin should be notified
        /// </summary>
        public int NotifyAdminForQuantityBelow { get; set; }

        /// <summary>
        /// Gets or sets the identifier of picture associated with this combination
        /// </summary>
        public string PictureId { get; set; }

        /// <summary>
        /// Gets or sets the collection of "ProductCombinationWarehouseInventory" records. We use it only when "UseMultipleWarehouses" is set to "true"
        /// </summary>
        public virtual ICollection<ProductCombinationWarehouseInventory> WarehouseInventory
        {
            get { return _warehouseInventory ?? (_warehouseInventory = new List<ProductCombinationWarehouseInventory>()); }
            protected set { _warehouseInventory = value; }
        }

        /// <summary>
        /// Gets or sets the collection of "ProductCombinationTierPrices" records. 
        /// </summary>
        public virtual ICollection<ProductCombinationTierPrices> TierPrices
        {
            get { return _tierPrices ?? (_tierPrices = new List<ProductCombinationTierPrices>()); }
            protected set { _tierPrices = value; }
        }

    }
}
