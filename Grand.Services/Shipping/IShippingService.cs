using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Orders;
using Grand.Domain.Shipping;
using Grand.Domain.Stores;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Shipping
{
    /// <summary>
    /// Shipping service interface
    /// </summary>
    public partial interface IShippingService
    {
        /// <summary>
        /// Load active shipping rate computation methods
        /// </summary>
        /// <param name="storeId">Load records allowed only in a specified store; pass "" to load all records</param>
        /// <returns>Shipping rate computation methods</returns>
        Task<IList<IShippingRateComputationMethod>> LoadActiveShippingRateComputationMethods(string storeId = "", IList<ShoppingCartItem> cart = null);

        /// <summary>
        /// Load shipping rate computation method by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Found Shipping rate computation method</returns>
        IShippingRateComputationMethod LoadShippingRateComputationMethodBySystemName(string systemName);

        /// <summary>
        /// Load all shipping rate computation methods
        /// </summary>
        /// <param name="storeId">Load records allowed only in a specified store; pass "" to load all records</param>
        /// <returns>Shipping rate computation methods</returns>
        IList<IShippingRateComputationMethod> LoadAllShippingRateComputationMethods(string storeId = "");

        /// <summary>
        /// Deletes a shipping method
        /// </summary>
        /// <param name="shippingMethod">The shipping method</param>
        Task DeleteShippingMethod(ShippingMethod shippingMethod);

        /// <summary>
        /// Gets a shipping method
        /// </summary>
        /// <param name="shippingMethodId">The shipping method identifier</param>
        /// <returns>Shipping method</returns>
        Task<ShippingMethod> GetShippingMethodById(string shippingMethodId);


        /// <summary>
        /// Gets all shipping methods
        /// </summary>
        /// <param name="filterByCountryId">The country indentifier to filter by</param>
        /// <returns>Shipping methods</returns>
        Task<IList<ShippingMethod>> GetAllShippingMethods(string filterByCountryId = "", Customer customer = null);

        /// <summary>
        /// Inserts a shipping method
        /// </summary>
        /// <param name="shippingMethod">Shipping method</param>
        Task InsertShippingMethod(ShippingMethod shippingMethod);

        /// <summary>
        /// Updates the shipping method
        /// </summary>
        /// <param name="shippingMethod">Shipping method</param>
        Task UpdateShippingMethod(ShippingMethod shippingMethod);



        /// <summary>
        /// Deletes a delivery date
        /// </summary>
        /// <param name="deliveryDate">The delivery date</param>
        Task DeleteDeliveryDate(DeliveryDate deliveryDate);

        /// <summary>
        /// Gets a delivery date
        /// </summary>
        /// <param name="deliveryDateId">The delivery date identifier</param>
        /// <returns>Delivery date</returns>
        Task<DeliveryDate> GetDeliveryDateById(string deliveryDateId);

        /// <summary>
        /// Gets all delivery dates
        /// </summary>
        /// <returns>Delivery dates</returns>
        Task<IList<DeliveryDate>> GetAllDeliveryDates();

        /// <summary>
        /// Inserts a delivery date
        /// </summary>
        /// <param name="deliveryDate">Delivery date</param>
        Task InsertDeliveryDate(DeliveryDate deliveryDate);

        /// <summary>
        /// Updates the delivery date
        /// </summary>
        /// <param name="deliveryDate">Delivery date</param>
        Task UpdateDeliveryDate(DeliveryDate deliveryDate);


        /// <summary>
        /// Deletes a warehouse
        /// </summary>
        /// <param name="warehouse">The warehouse</param>
        Task DeleteWarehouse(Warehouse warehouse);

        /// <summary>
        /// Gets a warehouse
        /// </summary>
        /// <param name="warehouseId">The warehouse identifier</param>
        /// <returns>Warehouse</returns>
        Task<Warehouse> GetWarehouseById(string warehouseId);

        /// <summary>
        /// Gets all warehouses
        /// </summary>
        /// <returns>Warehouses</returns>
        Task<IList<Warehouse>> GetAllWarehouses();

        /// <summary>
        /// Inserts a warehouse
        /// </summary>
        /// <param name="warehouse">Warehouse</param>
        Task InsertWarehouse(Warehouse warehouse);

        /// <summary>
        /// Updates the warehouse
        /// </summary>
        /// <param name="warehouse">Warehouse</param>
        Task UpdateWarehouse(Warehouse warehouse);


        /// <summary>
        /// Gets a warehouse
        /// </summary>
        /// <param name="pickupPointId">The pickup point identifier</param>
        /// <returns>PickupPoint</returns>
        Task<PickupPoint> GetPickupPointById(string pickupPointId);

        /// <summary>
        /// Gets all pickup points
        /// </summary>
        /// <returns>PickupPoints</returns>
        Task<IList<PickupPoint>> GetAllPickupPoints();

        /// <summary>
        /// Gets active pickup points
        /// </summary>
        /// <returns>PickupPoints</returns>
        Task<IList<PickupPoint>> LoadActivePickupPoints(string storeId = "");

        /// <summary>
        /// Inserts a pickupPoint
        /// </summary>
        /// <param name="PickupPoint">PickupPoint</param>
        Task InsertPickupPoint(PickupPoint pickuppoint);

        /// <summary>
        /// Updates the pickupPoint
        /// </summary>
        /// <param name="pickupPoint">PickupPoint</param>
        Task UpdatePickupPoint(PickupPoint pickuppoint);

        /// <summary>
        /// Deletes a pickupPoint
        /// </summary>
        /// <param name="pickupPoint">The pickupPoint</param>
        Task DeletePickupPoint(PickupPoint pickuppoint);

        /// <summary>
        /// Gets shopping cart item weight (of one item)
        /// </summary>
        /// <param name="shoppingCartItem">Shopping cart item</param>
        /// <returns>Shopping cart item weight</returns>
        Task<decimal> GetShoppingCartItemWeight(ShoppingCartItem shoppingCartItem);

        /// <summary>
        /// Gets shopping cart weight
        /// </summary>
        /// <param name="request">Request</param>
        /// <param name="includeCheckoutAttributes">A value indicating whether we should calculate weights of selected checkotu attributes</param>
        /// <returns>Total weight</returns>
        Task<decimal> GetTotalWeight(GetShippingOptionRequest request, bool includeCheckoutAttributes = true);

        /// <summary>
        /// Get dimensions of associated products (for quantity 1)
        /// </summary>
        /// <param name="shoppingCartItem">Shopping cart item</param>
        /// <returns name="width">Width</returns>
        /// <returns name="length">Length</returns>
        /// <returns name="height">Height</returns>
        Task<(decimal width, decimal length, decimal height)> GetAssociatedProductDimensions(ShoppingCartItem shoppingCartItem);

        /// <summary>
        /// Get total dimensions
        /// </summary>
        /// <param name="packageItems">Package items</param>
        /// <param name="width">Width</param>
        /// <param name="length">Length</param>
        /// <param name="height">Height</param>
        Task<(decimal width, decimal length, decimal height)> GetDimensions(IList<GetShippingOptionRequest.PackageItem> packageItems);

        /// <summary>
        /// Get the nearest warehouse for the specified address
        /// </summary>
        /// <param name="address">Address</param>
        /// <param name="warehouses">List of warehouses, if null all warehouses are used.</param>
        /// <returns></returns>
        Task<Warehouse> GetNearestWarehouse(Address address, IList<Warehouse> warehouses = null);

        /// <summary>
        /// Create shipment packages (requests) from shopping cart
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="shippingAddress">Shipping address</param>
        /// <param name="storeId">Load records allowed only in a specified store; pass "" to load all records</param>
        /// <returns>Shipment packages (requests)</returns>
        /// <param name="shippingFromMultipleLocations">Value indicating whether shipping is done from multiple locations (warehouses)</param>
        Task<(IList<GetShippingOptionRequest> shippingOptionRequest, bool shippingFromMultipleLocations)> CreateShippingOptionRequests(Customer customer,
            IList<ShoppingCartItem> cart, Address shippingAddress, Store store);

        /// <summary>
        ///  Gets available shipping options
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="shippingAddress">Shipping address</param>
        /// <param name="allowedShippingRateComputationMethodSystemName">Filter by shipping rate computation method identifier; null to load shipping options of all shipping rate computation methods</param>
        /// <param name="storeId">Load records allowed only in a specified store; pass "" to load all records</param>
        /// <returns>Shipping options</returns>
        Task<GetShippingOptionResponse> GetShippingOptions(Customer customer, IList<ShoppingCartItem> cart, Address shippingAddress,
            string allowedShippingRateComputationMethodSystemName = "", Store store = null);
    }
}
