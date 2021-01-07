using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Shipping;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Catalog
{
    public interface IInventoryManageService
    {
        #region Inventory management methods

        /// <summary>
        /// Updates stock the product
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="mediator">Notification</param>
        Task UpdateStockProduct(Product product, bool mediator = true);

        /// <summary>
        /// Adjust inventory
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="quantityToChange">Quantity to increase or descrease</param>
        /// <param name="attributes">Attributes</param>
        Task AdjustInventory(Product product, int quantityToChange, IList<CustomAttribute> attributes = null, string warehouseId = "");

        /// <summary>
        /// Reserve the given quantity in the warehouses.
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="quantity">Quantity, must be negative</param>
        Task ReserveInventory(Product product, int quantity, string warehouseId);

        /// <summary>
        /// Unblocks the given quantity reserved items in the warehouses
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="quantity">Quantity, must be positive</param>
        Task UnblockReservedInventory(Product product, int quantity, string warehouseId);

        /// <summary>
        /// Book the reserved quantity
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customAttribute">Attribute</param>
        /// <param name="warehouseId">Warehouse identifier</param>
        /// <param name="quantity">Quantity, must be negative</param>
        Task BookReservedInventory(Product product, IList<CustomAttribute> customAttribute, string warehouseId, int quantity);

        /// <summary>
        /// Reverse booked inventory (if acceptable)
        /// </summary>
        /// <param name="product">product</param>
        /// <param name="shipment">Shipment</param>
        /// <param name="shipmentItem">Shipment item</param>
        /// <returns>Quantity reversed</returns>
        Task<int> ReverseBookedInventory(Product product, Shipment shipment, ShipmentItem shipmentItem);

        #endregion

    }
}
