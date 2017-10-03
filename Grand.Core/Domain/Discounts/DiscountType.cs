namespace Grand.Core.Domain.Discounts
{
    /// <summary>
    /// Represents a discount type
    /// </summary>
    public enum DiscountType
    {
        /// <summary>
        /// Assigned to order total 
        /// </summary>
        AssignedToOrderTotal = 1,
        /// <summary>
        /// Assigned to products (SKUs)
        /// </summary>
        AssignedToSkus = 2,
        /// <summary>
        /// Assigned to categories (all products in a category)
        /// </summary>
        AssignedToCategories = 5,
        /// <summary>
        /// Assigned to manufacturers (all products of a manufacturer)
        /// </summary>
        AssignedToManufacturers = 6,
        /// <summary>
        /// Assigned to vendors (all products of a vendor)
        /// </summary>
        AssignedToVendors = 7,
        /// <summary>
        /// Assigned to stores (all products of a store)
        /// </summary>
        AssignedToStores = 8,
        /// <summary>
        /// Assigned to shipping
        /// </summary>
        AssignedToShipping = 10,
        /// <summary>
        /// Assigned to order subtotal
        /// </summary>
        AssignedToOrderSubTotal = 20,
        /// <summary>
        /// Assigned to all product
        /// </summary>
        AssignedToAllProducts = 30,
    }
}
