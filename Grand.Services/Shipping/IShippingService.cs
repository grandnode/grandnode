using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Shipping;
using System.Collections.Generic;

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
        IList<IShippingRateComputationMethod> LoadActiveShippingRateComputationMethods(string storeId = "", IList<ShoppingCartItem> cart = null);

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
        void DeleteShippingMethod(ShippingMethod shippingMethod);

        /// <summary>
        /// Gets a shipping method
        /// </summary>
        /// <param name="shippingMethodId">The shipping method identifier</param>
        /// <returns>Shipping method</returns>
        ShippingMethod GetShippingMethodById(string shippingMethodId);


        /// <summary>
        /// Gets all shipping methods
        /// </summary>
        /// <param name="filterByCountryId">The country indentifier to filter by</param>
        /// <returns>Shipping methods</returns>
        IList<ShippingMethod> GetAllShippingMethods(string filterByCountryId = "", Customer customer = null);

        /// <summary>
        /// Inserts a shipping method
        /// </summary>
        /// <param name="shippingMethod">Shipping method</param>
        void InsertShippingMethod(ShippingMethod shippingMethod);

        /// <summary>
        /// Updates the shipping method
        /// </summary>
        /// <param name="shippingMethod">Shipping method</param>
        void UpdateShippingMethod(ShippingMethod shippingMethod);



        /// <summary>
        /// Deletes a delivery date
        /// </summary>
        /// <param name="deliveryDate">The delivery date</param>
        void DeleteDeliveryDate(DeliveryDate deliveryDate);

        /// <summary>
        /// Gets a delivery date
        /// </summary>
        /// <param name="deliveryDateId">The delivery date identifier</param>
        /// <returns>Delivery date</returns>
        DeliveryDate GetDeliveryDateById(string deliveryDateId);

        /// <summary>
        /// Gets all delivery dates
        /// </summary>
        /// <returns>Delivery dates</returns>
        IList<DeliveryDate> GetAllDeliveryDates();

        /// <summary>
        /// Inserts a delivery date
        /// </summary>
        /// <param name="deliveryDate">Delivery date</param>
        void InsertDeliveryDate(DeliveryDate deliveryDate);

        /// <summary>
        /// Updates the delivery date
        /// </summary>
        /// <param name="deliveryDate">Delivery date</param>
        void UpdateDeliveryDate(DeliveryDate deliveryDate);

        
        /// <summary>
        /// Deletes a warehouse
        /// </summary>
        /// <param name="warehouse">The warehouse</param>
        void DeleteWarehouse(Warehouse warehouse);

        /// <summary>
        /// Gets a warehouse
        /// </summary>
        /// <param name="warehouseId">The warehouse identifier</param>
        /// <returns>Warehouse</returns>
        Warehouse GetWarehouseById(string warehouseId);

        /// <summary>
        /// Gets all warehouses
        /// </summary>
        /// <returns>Warehouses</returns>
        IList<Warehouse> GetAllWarehouses();

        /// <summary>
        /// Inserts a warehouse
        /// </summary>
        /// <param name="warehouse">Warehouse</param>
        void InsertWarehouse(Warehouse warehouse);

        /// <summary>
        /// Updates the warehouse
        /// </summary>
        /// <param name="warehouse">Warehouse</param>
        void UpdateWarehouse(Warehouse warehouse);


        /// <summary>
        /// Gets a warehouse
        /// </summary>
        /// <param name="pickupPointId">The pickup point identifier</param>
        /// <returns>PickupPoint</returns>
        PickupPoint GetPickupPointById(string pickupPointId);

        /// <summary>
        /// Gets all pickup points
        /// </summary>
        /// <returns>PickupPoints</returns>
        IList<PickupPoint> GetAllPickupPoints();

        /// <summary>
        /// Gets active pickup points
        /// </summary>
        /// <returns>PickupPoints</returns>
        IList<PickupPoint> LoadActivePickupPoints(string storeId = "");

        /// <summary>
        /// Inserts a pickupPoint
        /// </summary>
        /// <param name="PickupPoint">PickupPoint</param>
        void InsertPickupPoint(PickupPoint pickuppoint);

        /// <summary>
        /// Updates the pickupPoint
        /// </summary>
        /// <param name="pickupPoint">PickupPoint</param>
        void UpdatePickupPoint(PickupPoint pickuppoint);

        /// <summary>
        /// Deletes a pickupPoint
        /// </summary>
        /// <param name="pickupPoint">The pickupPoint</param>
        void DeletePickupPoint(PickupPoint pickuppoint);

        /// <summary>
        /// Gets shopping cart item weight (of one item)
        /// </summary>
        /// <param name="shoppingCartItem">Shopping cart item</param>
        /// <returns>Shopping cart item weight</returns>
        decimal GetShoppingCartItemWeight(ShoppingCartItem shoppingCartItem);

        /// <summary>
        /// Gets shopping cart weight
        /// </summary>
        /// <param name="request">Request</param>
        /// <param name="includeCheckoutAttributes">A value indicating whether we should calculate weights of selected checkotu attributes</param>
        /// <returns>Total weight</returns>
        decimal GetTotalWeight(GetShippingOptionRequest request, bool includeCheckoutAttributes = true);

        /// <summary>
        /// Get dimensions of associated products (for quantity 1)
        /// </summary>
        /// <param name="shoppingCartItem">Shopping cart item</param>
        /// <param name="width">Width</param>
        /// <param name="length">Length</param>
        /// <param name="height">Height</param>
        void GetAssociatedProductDimensions(ShoppingCartItem shoppingCartItem,
            out decimal width, out decimal length, out decimal height);

        /// <summary>
        /// Get total dimensions
        /// </summary>
        /// <param name="packageItems">Package items</param>
        /// <param name="width">Width</param>
        /// <param name="length">Length</param>
        /// <param name="height">Height</param>
        void GetDimensions(IList<GetShippingOptionRequest.PackageItem> packageItems,
            out decimal width, out decimal length, out decimal height);

        /// <summary>
        /// Get the nearest warehouse for the specified address
        /// </summary>
        /// <param name="address">Address</param>
        /// <param name="warehouses">List of warehouses, if null all warehouses are used.</param>
        /// <returns></returns>
        Warehouse GetNearestWarehouse(Address address, IList<Warehouse> warehouses = null);

        /// <summary>
        /// Create shipment packages (requests) from shopping cart
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="shippingAddress">Shipping address</param>
        /// <param name="storeId">Load records allowed only in a specified store; pass "" to load all records</param>
        /// <returns>Shipment packages (requests)</returns>
        /// <param name="shippingFromMultipleLocations">Value indicating whether shipping is done from multiple locations (warehouses)</param>
        IList<GetShippingOptionRequest> CreateShippingOptionRequests(Customer customer,
            IList<ShoppingCartItem> cart,
            Address shippingAddress, string storeId, out bool shippingFromMultipleLocations);

        /// <summary>
        ///  Gets available shipping options
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="shippingAddress">Shipping address</param>
        /// <param name="allowedShippingRateComputationMethodSystemName">Filter by shipping rate computation method identifier; null to load shipping options of all shipping rate computation methods</param>
        /// <param name="storeId">Load records allowed only in a specified store; pass "" to load all records</param>
        /// <returns>Shipping options</returns>
        GetShippingOptionResponse GetShippingOptions(Customer customer, IList<ShoppingCartItem> cart, Address shippingAddress,
            string allowedShippingRateComputationMethodSystemName = "", string storeId = "");
    }
}
