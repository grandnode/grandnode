using Grand.Core;
using Grand.Core.Caching;
using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Orders;
using Grand.Domain.Shipping;
using Grand.Domain.Stores;
using Grand.Core.Plugins;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Directory;
using Grand.Services.Events;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Orders;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Shipping
{
    /// <summary>
    /// Shipping service
    /// </summary>
    public partial class ShippingService : IShippingService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : warehouse ID
        /// </remarks>
        private const string WAREHOUSES_BY_ID_KEY = "Grand.warehouse.id-{0}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string WAREHOUSES_PATTERN_KEY = "Grand.warehouse.";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string SHIPPINGMETHOD_PATTERN_KEY = "Grand.shippingmethod.";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PICKUPPOINTS_PATTERN_KEY = "Grand.pickuppoint.";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : delivery date ID
        /// </remarks>
        private const string DELIVERYDATE_BY_ID_KEY = "Grand.deliverydate.id-{0}";

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// </remarks>
        private const string DELIVERYDATE_PATTERN_KEY = "Grand.deliverydate.";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PRODUCTS_PATTERN_KEY = "Grand.product.";

        #endregion

        #region Fields

        private readonly IRepository<ShippingMethod> _shippingMethodRepository;
        private readonly IRepository<DeliveryDate> _deliveryDateRepository;
        private readonly IRepository<Warehouse> _warehouseRepository;
        private readonly IRepository<PickupPoint> _pickupPointsRepository;
        private readonly ILogger _logger;
        private readonly IProductService _productService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ILocalizationService _localizationService;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IPluginFinder _pluginFinder;
        private readonly IMediator _mediator;
        private readonly ICacheManager _cacheManager;
        private readonly ShippingSettings _shippingSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        public ShippingService(
            IRepository<ShippingMethod> shippingMethodRepository,
            IRepository<DeliveryDate> deliveryDateRepository,
            IRepository<Warehouse> warehouseRepository,
            IRepository<PickupPoint> pickupPointsRepository,
            ILogger logger,
            IProductService productService,
            IProductAttributeParser productAttributeParser,
            ICheckoutAttributeParser checkoutAttributeParser,
            ILocalizationService localizationService,
            IAddressService addressService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IPluginFinder pluginFinder,
            IMediator mediator,
            ICurrencyService currencyService,
            ICacheManager cacheManager,
            ShoppingCartSettings shoppingCartSettings,
            ShippingSettings shippingSettings)
        {
            _shippingMethodRepository = shippingMethodRepository;
            _deliveryDateRepository = deliveryDateRepository;
            _warehouseRepository = warehouseRepository;
            _pickupPointsRepository = pickupPointsRepository;
            _logger = logger;
            _productService = productService;
            _productAttributeParser = productAttributeParser;
            _checkoutAttributeParser = checkoutAttributeParser;
            _localizationService = localizationService;
            _addressService = addressService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _pluginFinder = pluginFinder;
            _currencyService = currencyService;
            _mediator = mediator;
            _cacheManager = cacheManager;
            _shoppingCartSettings = shoppingCartSettings;
            _shippingSettings = shippingSettings;
        }

        #endregion

        #region Methods

        #region Shipping rate computation methods

        /// <summary>
        /// Load active shipping rate computation methods
        /// </summary>
        /// <param name="storeId">Load records allowed only in a specified store; pass "" to load all records</param>
        /// <returns>Shipping rate computation methods</returns>
        public virtual async Task<IList<IShippingRateComputationMethod>> LoadActiveShippingRateComputationMethods(string storeId = "", IList<ShoppingCartItem> cart = null)
        {
            var shippingMethods = LoadAllShippingRateComputationMethods(storeId)
                   .Where(provider => _shippingSettings.ActiveShippingRateComputationMethodSystemNames.Contains(provider.PluginDescriptor.SystemName, StringComparer.OrdinalIgnoreCase))
                   .ToList();

            var availableShippingMethods = new List<IShippingRateComputationMethod>();
            foreach (var sm in shippingMethods)
            {
                if (!await sm.HideShipmentMethods(cart))
                    availableShippingMethods.Add(sm);
            }
            return availableShippingMethods;
        }

        /// <summary>
        /// Load shipping rate computation method by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Found Shipping rate computation method</returns>
        public virtual IShippingRateComputationMethod LoadShippingRateComputationMethodBySystemName(string systemName)
        {
            var descriptor = _pluginFinder.GetPluginDescriptorBySystemName<IShippingRateComputationMethod>(systemName);
            if (descriptor != null)
                return descriptor.Instance<IShippingRateComputationMethod>(_pluginFinder.ServiceProvider);

            return null;
        }

        /// <summary>
        /// Load all shipping rate computation methods
        /// </summary>
        /// <param name="storeId">Load records allowed only in a specified store; pass "" to load all records</param>
        /// <returns>Shipping rate computation methods</returns>
        public virtual IList<IShippingRateComputationMethod> LoadAllShippingRateComputationMethods(string storeId = "")
        {
            return _pluginFinder.GetPlugins<IShippingRateComputationMethod>(storeId: storeId).ToList();
        }

        #endregion

        #region Shipping methods


        /// <summary>
        /// Deletes a shipping method
        /// </summary>
        /// <param name="shippingMethod">The shipping method</param>
        public virtual async Task DeleteShippingMethod(ShippingMethod shippingMethod)
        {
            if (shippingMethod == null)
                throw new ArgumentNullException("shippingMethod");

            await _shippingMethodRepository.DeleteAsync(shippingMethod);

            //clear cache
            await _cacheManager.RemoveByPrefix(SHIPPINGMETHOD_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(shippingMethod);
        }

        /// <summary>
        /// Gets a shipping method
        /// </summary>
        /// <param name="shippingMethodId">The shipping method identifier</param>
        /// <returns>Shipping method</returns>
        public virtual Task<ShippingMethod> GetShippingMethodById(string shippingMethodId)
        {
            return _shippingMethodRepository.GetByIdAsync(shippingMethodId);
        }

        /// <summary>
        /// Gets all shipping methods
        /// </summary>
        /// <param name="filterByCountryId">The country indentifier to filter by</param>
        /// <returns>Shipping methods</returns>
        public virtual async Task<IList<ShippingMethod>> GetAllShippingMethods(string filterByCountryId = "", Customer customer = null)
        {
            var shippingMethods = new List<ShippingMethod>();

            shippingMethods = await _cacheManager.GetAsync(SHIPPINGMETHOD_PATTERN_KEY, () =>
            {
                var query = from sm in _shippingMethodRepository.Table
                            orderby sm.DisplayOrder
                            select sm;
                return query.ToListAsync();
            });

            if (!String.IsNullOrEmpty(filterByCountryId))
            {
                shippingMethods = shippingMethods.Where(x => !x.CountryRestrictionExists(filterByCountryId)).ToList();
            }
            if (customer != null)
            {
                shippingMethods = shippingMethods.Where(x => !x.CustomerRoleRestrictionExists(customer.CustomerRoles.Select(y => y.Id).ToList())).ToList();
            }

            return shippingMethods;
        }

        /// <summary>
        /// Inserts a shipping method
        /// </summary>
        /// <param name="shippingMethod">Shipping method</param>
        public virtual async Task InsertShippingMethod(ShippingMethod shippingMethod)
        {
            if (shippingMethod == null)
                throw new ArgumentNullException("shippingMethod");

            await _shippingMethodRepository.InsertAsync(shippingMethod);

            //clear cache
            await _cacheManager.RemoveByPrefix(SHIPPINGMETHOD_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(shippingMethod);
        }

        /// <summary>
        /// Updates the shipping method
        /// </summary>
        /// <param name="shippingMethod">Shipping method</param>
        public virtual async Task UpdateShippingMethod(ShippingMethod shippingMethod)
        {
            if (shippingMethod == null)
                throw new ArgumentNullException("shippingMethod");

            await _shippingMethodRepository.UpdateAsync(shippingMethod);

            //clear cache
            await _cacheManager.RemoveByPrefix(SHIPPINGMETHOD_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(shippingMethod);
        }

        #endregion

        #region Delivery dates

        /// <summary>
        /// Deletes a delivery date
        /// </summary>
        /// <param name="deliveryDate">The delivery date</param>
        public virtual async Task DeleteDeliveryDate(DeliveryDate deliveryDate)
        {
            if (deliveryDate == null)
                throw new ArgumentNullException("deliveryDate");

            await _deliveryDateRepository.DeleteAsync(deliveryDate);

            //clear cache
            await _cacheManager.RemoveByPrefix(DELIVERYDATE_PATTERN_KEY);

            //clear product cache
            await _cacheManager.RemoveByPrefix(PRODUCTS_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(deliveryDate);
        }

        /// <summary>
        /// Gets a delivery date
        /// </summary>
        /// <param name="deliveryDateId">The delivery date identifier</param>
        /// <returns>Delivery date</returns>
        public virtual Task<DeliveryDate> GetDeliveryDateById(string deliveryDateId)
        {
            string key = string.Format(DELIVERYDATE_BY_ID_KEY, deliveryDateId);
            return _cacheManager.GetAsync(key, () => _deliveryDateRepository.GetByIdAsync(deliveryDateId));
        }

        /// <summary>
        /// Gets all delivery dates
        /// </summary>
        /// <returns>Delivery dates</returns>
        public virtual async Task<IList<DeliveryDate>> GetAllDeliveryDates()
        {
            var query = from dd in _deliveryDateRepository.Table
                        orderby dd.DisplayOrder
                        select dd;
            return await query.ToListAsync();
        }

        /// <summary>
        /// Inserts a delivery date
        /// </summary>
        /// <param name="deliveryDate">Delivery date</param>
        public virtual async Task InsertDeliveryDate(DeliveryDate deliveryDate)
        {
            if (deliveryDate == null)
                throw new ArgumentNullException("deliveryDate");

            await _deliveryDateRepository.InsertAsync(deliveryDate);

            //event notification
            await _mediator.EntityInserted(deliveryDate);
        }

        /// <summary>
        /// Updates the delivery date
        /// </summary>
        /// <param name="deliveryDate">Delivery date</param>
        public virtual async Task UpdateDeliveryDate(DeliveryDate deliveryDate)
        {
            if (deliveryDate == null)
                throw new ArgumentNullException("deliveryDate");

            await _deliveryDateRepository.UpdateAsync(deliveryDate);

            //clear cache
            await _cacheManager.RemoveByPrefix(DELIVERYDATE_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(deliveryDate);
        }

        #endregion

        #region Warehouses

        /// <summary>
        /// Deletes a warehouse
        /// </summary>
        /// <param name="warehouse">The warehouse</param>
        public virtual async Task DeleteWarehouse(Warehouse warehouse)
        {
            if (warehouse == null)
                throw new ArgumentNullException("warehouse");

            await _warehouseRepository.DeleteAsync(warehouse);

            //clear cache
            await _cacheManager.RemoveByPrefix(WAREHOUSES_PATTERN_KEY);
            //clear product cache
            await _cacheManager.RemoveByPrefix(PRODUCTS_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(warehouse);
        }

        /// <summary>
        /// Gets a warehouse
        /// </summary>
        /// <param name="warehouseId">The warehouse identifier</param>
        /// <returns>Warehouse</returns>
        public virtual Task<Warehouse> GetWarehouseById(string warehouseId)
        {
            string key = string.Format(WAREHOUSES_BY_ID_KEY, warehouseId);
            return _cacheManager.GetAsync(key, () => _warehouseRepository.GetByIdAsync(warehouseId));
        }

        /// <summary>
        /// Gets all warehouses
        /// </summary>
        /// <returns>Warehouses</returns>
        public virtual async Task<IList<Warehouse>> GetAllWarehouses()
        {
            var query = from wh in _warehouseRepository.Table
                        orderby wh.Name
                        select wh;
            return await query.ToListAsync();
        }

        /// <summary>
        /// Inserts a warehouse
        /// </summary>
        /// <param name="warehouse">Warehouse</param>
        public virtual async Task InsertWarehouse(Warehouse warehouse)
        {
            if (warehouse == null)
                throw new ArgumentNullException("warehouse");

            await _warehouseRepository.InsertAsync(warehouse);

            //clear cache
            await _cacheManager.RemoveByPrefix(WAREHOUSES_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(warehouse);
        }

        /// <summary>
        /// Updates the warehouse
        /// </summary>
        /// <param name="warehouse">Warehouse</param>
        public virtual async Task UpdateWarehouse(Warehouse warehouse)
        {
            if (warehouse == null)
                throw new ArgumentNullException("warehouse");

            await _warehouseRepository.UpdateAsync(warehouse);

            //clear cache
            await _cacheManager.RemoveByPrefix(WAREHOUSES_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(warehouse);
        }

        #endregion


        #region Pickup points


        /// <summary>
        /// Gets a pickup point
        /// </summary>
        /// <param name="pickupPointId">The pickup point identifier</param>
        /// <returns>Delivery date</returns>
        public virtual Task<PickupPoint> GetPickupPointById(string pickupPointId)
        {
            return _pickupPointsRepository.GetByIdAsync(pickupPointId);
        }

        /// <summary>
        /// Gets all pickup points
        /// </summary>
        /// <returns>Warehouses</returns>
        public virtual async Task<IList<PickupPoint>> GetAllPickupPoints()
        {
            var query = from pp in _pickupPointsRepository.Table
                        orderby pp.DisplayOrder
                        select pp;
            return await query.ToListAsync();
        }

        /// <summary>
        /// Gets all pickup points
        /// </summary>
        /// <returns>Warehouses</returns>
        public virtual async Task<IList<PickupPoint>> LoadActivePickupPoints(string storeId = "")
        {
            var query = from pp in _pickupPointsRepository.Table
                        where pp.StoreId == storeId || String.IsNullOrEmpty(pp.StoreId)
                        orderby pp.DisplayOrder
                        select pp;
            return await query.ToListAsync();
        }


        /// <summary>
        /// Inserts a warehouse
        /// </summary>
        /// <param name="warehouse">Warehouse</param>
        public virtual async Task InsertPickupPoint(PickupPoint pickupPoint)
        {
            if (pickupPoint == null)
                throw new ArgumentNullException("pickupPoint");

            await _pickupPointsRepository.InsertAsync(pickupPoint);

            //clear cache
            await _cacheManager.RemoveByPrefix(PICKUPPOINTS_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(pickupPoint);
        }

        /// <summary>
        /// Updates the warehouse
        /// </summary>
        /// <param name="warehouse">Warehouse</param>
        public virtual async Task UpdatePickupPoint(PickupPoint pickupPoint)
        {
            if (pickupPoint == null)
                throw new ArgumentNullException("pickupPoint");

            await _pickupPointsRepository.UpdateAsync(pickupPoint);

            //clear cache
            await _cacheManager.RemoveByPrefix(WAREHOUSES_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(pickupPoint);
        }

        /// <summary>
        /// Deletes a delivery date
        /// </summary>
        /// <param name="deliveryDate">The delivery date</param>
        public virtual async Task DeletePickupPoint(PickupPoint pickupPoint)
        {
            if (pickupPoint == null)
                throw new ArgumentNullException("pickupPoint");

            await _pickupPointsRepository.DeleteAsync(pickupPoint);
            await _cacheManager.RemoveByPrefix(PICKUPPOINTS_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(pickupPoint);
        }


        #endregion

        #region Workflow

        /// <summary>
        /// Gets shopping cart item weight (of one item)
        /// </summary>
        /// <param name="shoppingCartItem">Shopping cart item</param>
        /// <returns>Shopping cart item weight</returns>
        public virtual async Task<decimal> GetShoppingCartItemWeight(ShoppingCartItem shoppingCartItem)
        {
            if (shoppingCartItem == null)
                throw new ArgumentNullException("shoppingCartItem");
            var product = await _productService.GetProductById(shoppingCartItem.ProductId);
            if (product == null)
                return decimal.Zero;

            //attribute weight
            decimal attributesTotalWeight = decimal.Zero;
            if (!String.IsNullOrEmpty(shoppingCartItem.AttributesXml))
            {
                var attributeValues = _productAttributeParser.ParseProductAttributeValues(product, shoppingCartItem.AttributesXml);
                foreach (var attributeValue in attributeValues)
                {
                    switch (attributeValue.AttributeValueType)
                    {
                        case AttributeValueType.Simple:
                            {
                                //simple attribute
                                attributesTotalWeight += attributeValue.WeightAdjustment;
                            }
                            break;
                        case AttributeValueType.AssociatedToProduct:
                            {
                                //bundled product
                                var associatedProduct = await _productService.GetProductById(attributeValue.AssociatedProductId);
                                if (associatedProduct != null && associatedProduct.IsShipEnabled)
                                {
                                    attributesTotalWeight += associatedProduct.Weight * attributeValue.Quantity;
                                }
                            }
                            break;
                    }
                }
            }
            var weight = product.Weight + attributesTotalWeight;
            return weight;
        }

        /// <summary>
        /// Gets shopping cart weight
        /// </summary>
        /// <param name="request">Request</param>
        /// <param name="includeCheckoutAttributes">A value indicating whether we should calculate weights of selected checkotu attributes</param>
        /// <returns>Total weight</returns>
        public virtual async Task<decimal> GetTotalWeight(GetShippingOptionRequest request, bool includeCheckoutAttributes = true)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            Customer customer = request.Customer;

            decimal totalWeight = decimal.Zero;
            //shopping cart items
            foreach (var packageItem in request.Items)
                totalWeight += await GetShoppingCartItemWeight(packageItem.ShoppingCartItem) * packageItem.GetQuantity();

            //checkout attributes
            if (customer != null && includeCheckoutAttributes)
            {
                var checkoutAttributesXml = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.CheckoutAttributes, request.StoreId);
                if (!string.IsNullOrEmpty(checkoutAttributesXml))
                {
                    var attributeValues = await _checkoutAttributeParser.ParseCheckoutAttributeValues(checkoutAttributesXml);
                    foreach (var attributeValue in attributeValues)
                        totalWeight += attributeValue.WeightAdjustment;
                }
            }
            return totalWeight;
        }

        /// <summary>
        /// Get dimensions of associated products (for quantity 1)
        /// </summary>
        /// <param name="shoppingCartItem">Shopping cart item</param>
        /// <param name="width">Width</param>
        /// <param name="length">Length</param>
        /// <param name="height">Height</param>
        public virtual async Task<(decimal width, decimal length, decimal height)> GetAssociatedProductDimensions(ShoppingCartItem shoppingCartItem)
        {
            if (shoppingCartItem == null)
                throw new ArgumentNullException("shoppingCartItem");

            var width = decimal.Zero;
            var length = decimal.Zero;
            var height = decimal.Zero;

            //attributes
            if (String.IsNullOrEmpty(shoppingCartItem.AttributesXml))
                return (0, 0, 0);

            var product = await _productService.GetProductById(shoppingCartItem.ProductId);
            //bundled products (associated attributes)
            var attributeValues = _productAttributeParser.ParseProductAttributeValues(product, shoppingCartItem.AttributesXml)
                .Where(x => x.AttributeValueType == AttributeValueType.AssociatedToProduct)
                .ToList();
            foreach (var attributeValue in attributeValues)
            {
                var associatedProduct = await _productService.GetProductById(attributeValue.AssociatedProductId);
                if (associatedProduct != null && associatedProduct.IsShipEnabled)
                {
                    width += associatedProduct.Width * attributeValue.Quantity;
                    length += associatedProduct.Length * attributeValue.Quantity;
                    height += associatedProduct.Height * attributeValue.Quantity;
                }
            }
            return (width, length, height);
        }

        /// <summary>
        /// Get total dimensions
        /// </summary>
        /// <param name="packageItems">Package items</param>
        /// <param name="width">Width</param>
        /// <param name="length">Length</param>
        /// <param name="height">Height</param>
        public virtual async Task<(decimal width, decimal length, decimal height)> GetDimensions(IList<GetShippingOptionRequest.PackageItem> packageItems)
        {
            if (packageItems == null)
                throw new ArgumentNullException("packageItems");

            var length = decimal.Zero;
            var width = decimal.Zero;
            var height = decimal.Zero;

            if (_shippingSettings.UseCubeRootMethod)
            {
                //cube root of volume
                decimal totalVolume = 0;
                decimal maxProductWidth = 0;
                decimal maxProductLength = 0;
                decimal maxProductHeight = 0;
                foreach (var packageItem in packageItems)
                {
                    var shoppingCartItem = packageItem.ShoppingCartItem;

                    var product = await _productService.GetProductById(shoppingCartItem.ProductId);
                    var qty = packageItem.GetQuantity();

                    //associated products
                    var dimenstions = await GetAssociatedProductDimensions(shoppingCartItem);
                    decimal associatedProductsWidth = dimenstions.width;
                    decimal associatedProductsLength = dimenstions.length;
                    decimal associatedProductsHeight = dimenstions.height;

                    var productWidth = product.Width + associatedProductsWidth;
                    var productLength = product.Length + associatedProductsLength;
                    var productHeight = product.Height + associatedProductsHeight;

                    //we do not use cube root method when we have only one item with "qty" set to 1
                    if (packageItems.Count == 1 && qty == 1)
                    {
                        return (productWidth, productLength, productHeight);
                    }

                    totalVolume += qty * productHeight * productWidth * productLength;

                    if (productWidth > maxProductWidth)
                        maxProductWidth = productWidth;
                    if (productLength > maxProductLength)
                        maxProductLength = productLength;
                    if (productHeight > maxProductHeight)
                        maxProductHeight = productHeight;
                }
                decimal dimension = Convert.ToDecimal(Math.Pow(Convert.ToDouble(totalVolume), (double)(1.0 / 3.0)));
                length = dimension;
                width = dimension;
                height = dimension;
                //sometimes we have products with sizes like 1x1x20
                //that's why let's ensure that a maximum dimension is always preserved
                //otherwise, shipping rate computation methods can return low rates
                if (width < maxProductWidth)
                    width = maxProductWidth;
                if (length < maxProductLength)
                    length = maxProductLength;
                if (height < maxProductHeight)
                    height = maxProductHeight;
            }
            else
            {
                //summarize all values (very inaccurate with multiple items)
                width = length = height = decimal.Zero;
                foreach (var packageItem in packageItems)
                {
                    var shoppingCartItem = packageItem.ShoppingCartItem;
                    var product = await _productService.GetProductById(shoppingCartItem.ProductId);
                    var qty = packageItem.GetQuantity();
                    width += product.Width * qty;
                    length += product.Length * qty;
                    height += product.Height * qty;

                    //associated products
                    var associatedProductDimensions = await GetAssociatedProductDimensions(shoppingCartItem);

                    width += associatedProductDimensions.width;
                    length += associatedProductDimensions.length;
                    height += associatedProductDimensions.height;
                }
            }
            return (width, length, height);
        }

        /// <summary>
        /// Get the nearest warehouse for the specified address
        /// </summary>
        /// <param name="address">Address</param>
        /// <param name="warehouses">List of warehouses, if null all warehouses are used.</param>
        /// <returns></returns>
        public virtual async Task<Warehouse> GetNearestWarehouse(Address address, IList<Warehouse> warehouses = null)
        {
            warehouses = warehouses ?? await GetAllWarehouses();

            //no address specified. return any
            if (address == null)
                return warehouses.FirstOrDefault();

            //of course, we should use some better logic to find nearest warehouse
            //but we don't have a built-in geographic database which supports "distance" functionality
            //that's why we simply look for exact matches

            //find by country
            var matchedByCountry = new List<Warehouse>();
            foreach (var warehouse in warehouses)
            {
                var warehouseAddress = await _addressService.GetAddressByIdSettings(warehouse.AddressId);
                if (warehouseAddress != null)
                    if (warehouseAddress.CountryId == address.CountryId)
                        matchedByCountry.Add(warehouse);
            }
            //no country matches. return any
            if (!matchedByCountry.Any())
                return warehouses.FirstOrDefault();


            //find by state
            var matchedByState = new List<Warehouse>();
            foreach (var warehouse in matchedByCountry)
            {
                var warehouseAddress = await _addressService.GetAddressByIdSettings(warehouse.AddressId);
                if (warehouseAddress != null)
                    if (warehouseAddress.StateProvinceId == address.StateProvinceId)
                        matchedByState.Add(warehouse);
            }
            if (matchedByState.Any())
                return matchedByState.FirstOrDefault();

            //no state matches. return any
            return matchedByCountry.FirstOrDefault();
        }

        /// <summary>
        /// Create shipment packages (requests) from shopping cart
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="shippingAddress">Shipping address</param>
        /// <param name="storeId">Load records allowed only in a specified store; pass "" to load all records</param>
        /// <param name="shippingFromMultipleLocations">Value indicating whether shipping is done from multiple locations (warehouses)</param>
        /// <returns>Shipment packages (requests)</returns>
        public virtual async Task<(IList<GetShippingOptionRequest> shippingOptionRequest, bool shippingFromMultipleLocations)> CreateShippingOptionRequests(Customer customer,
            IList<ShoppingCartItem> cart, Address shippingAddress, Store store)
        {
            //if we always ship from the default shipping origin, then there's only one request
            //if we ship from warehouses ("ShippingSettings.UseWarehouseLocation" enabled),
            //then there could be several requests


            //key - warehouse identifier (0 - default shipping origin)
            //value - request
            var requests = new Dictionary<string, GetShippingOptionRequest>();

            //a list of requests with products which should be shipped separately
            var separateRequests = new List<GetShippingOptionRequest>();

            foreach (var sci in cart)
            {
                if (!sci.IsShipEnabled)
                    continue;

                var product = await _productService.GetProductById(sci.ProductId);

                //warehouses
                Warehouse warehouse = null;
                if (_shippingSettings.UseWarehouseLocation)
                {
                    if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                        product.UseMultipleWarehouses)
                    {
                        var allWarehouses = new List<Warehouse>();
                        //multiple warehouses supported
                        foreach (var pwi in product.ProductWarehouseInventory)
                        {
                            //TODO validate stock quantity when backorder is not allowed?
                            var tmpWarehouse = await GetWarehouseById(pwi.WarehouseId);
                            if (tmpWarehouse != null)
                                allWarehouses.Add(tmpWarehouse);
                        }
                        warehouse = await GetNearestWarehouse(shippingAddress, allWarehouses);
                    }
                    else
                    {
                        //multiple warehouses are not supported
                        warehouse = await GetWarehouseById(product.WarehouseId);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(sci.WarehouseId))
                        warehouse = await GetWarehouseById(sci.WarehouseId);
                    else
                    {
                        if (!string.IsNullOrEmpty(store?.DefaultWarehouseId))
                            warehouse = await GetWarehouseById(store.DefaultWarehouseId);
                    }

                }

                string warehouseId = warehouse != null ? warehouse.Id : "";

                if (requests.ContainsKey(warehouseId) && !product.ShipSeparately)
                {
                    //add item to existing request
                    requests[warehouseId].Items.Add(new GetShippingOptionRequest.PackageItem(sci));
                }
                else
                {
                    //create a new request
                    var request = new GetShippingOptionRequest();
                    //store
                    request.StoreId = store?.Id;
                    //customer
                    request.Customer = customer;
                    //add item
                    request.Items.Add(new GetShippingOptionRequest.PackageItem(sci));
                    //ship to
                    request.ShippingAddress = shippingAddress;
                    //ship from
                    Address originAddress = null;
                    if (warehouse != null)
                    {
                        //warehouse address
                        originAddress = await _addressService.GetAddressByIdSettings(warehouse.AddressId);
                        request.WarehouseFrom = warehouse;
                    }
                    if (originAddress == null)
                    {
                        //no warehouse address. in this case use the default shipping origin
                        originAddress = (await _addressService.GetAddressByIdSettings(_shippingSettings.ShippingOriginAddressId));
                    }
                    if (originAddress != null)
                    {
                        var country = await _countryService.GetCountryById(originAddress.CountryId);
                        var state = await _stateProvinceService.GetStateProvinceById(originAddress.StateProvinceId);
                        request.CountryFrom = country;
                        request.StateProvinceFrom = state;
                        request.ZipPostalCodeFrom = originAddress.ZipPostalCode;
                        request.CityFrom = originAddress.City;
                        request.AddressFrom = originAddress.Address1;
                    }

                    if (product.ShipSeparately)
                    {
                        //ship separately
                        separateRequests.Add(request);
                    }
                    else
                    {
                        //usual request
                        requests.Add(warehouseId, request);
                    }
                }
            }

            //multiple locations?
            //currently we just compare warehouses
            //but we should also consider cases when several warehouses are located in the same address
            bool shippingFromMultipleLocations = requests.Select(x => x.Key).Distinct().Count() > 1;

            var result = requests.Values.ToList();
            result.AddRange(separateRequests);

            return (result, shippingFromMultipleLocations);
        }

        /// <summary>
        ///  Gets available shipping options
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="shippingAddress">Shipping address</param>
        /// <param name="allowedShippingRateComputationMethodSystemName">Filter by shipping rate computation method identifier; null to load shipping options of all shipping rate computation methods</param>
        /// <param name="storeId">Load records allowed only in a specified store; pass "" to load all records</param>
        /// <returns>Shipping options</returns>
        public virtual async Task<GetShippingOptionResponse> GetShippingOptions(Customer customer, IList<ShoppingCartItem> cart,
            Address shippingAddress, string allowedShippingRateComputationMethodSystemName = "",
            Store store = null)
        {
            if (cart == null)
                throw new ArgumentNullException("cart");

            var result = new GetShippingOptionResponse();

            //create a package
            var shippingOptionRequests = await CreateShippingOptionRequests(customer, cart, shippingAddress, store);
            result.ShippingFromMultipleLocations = shippingOptionRequests.shippingFromMultipleLocations;

            var shippingRateComputationMethods = await LoadActiveShippingRateComputationMethods(store?.Id, cart);
            //filter by system name
            if (!String.IsNullOrWhiteSpace(allowedShippingRateComputationMethodSystemName))
            {
                shippingRateComputationMethods = shippingRateComputationMethods
                    .Where(srcm => allowedShippingRateComputationMethodSystemName.Equals(srcm.PluginDescriptor.SystemName, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            if (!shippingRateComputationMethods.Any())
                throw new GrandException("Shipping rate computation method could not be loaded");



            //request shipping options from each shipping rate computation methods
            foreach (var srcm in shippingRateComputationMethods)
            {
                //request shipping options (separately for each package-request)
                IList<ShippingOption> srcmShippingOptions = null;
                foreach (var shippingOptionRequest in shippingOptionRequests.shippingOptionRequest)
                {
                    var getShippingOptionResponse = await srcm.GetShippingOptions(shippingOptionRequest);

                    if (getShippingOptionResponse.Success)
                    {
                        //success
                        if (srcmShippingOptions == null)
                        {
                            //first shipping option request
                            srcmShippingOptions = getShippingOptionResponse.ShippingOptions;
                        }
                        else
                        {
                            //get shipping options which already exist for prior requested packages for this scrm (i.e. common options)
                            srcmShippingOptions = srcmShippingOptions
                                .Where(existingso => getShippingOptionResponse.ShippingOptions.Any(newso => newso.Name == existingso.Name))
                                .ToList();

                            //and sum the rates
                            foreach (var existingso in srcmShippingOptions)
                            {
                                existingso.Rate += getShippingOptionResponse
                                    .ShippingOptions
                                    .First(newso => newso.Name == existingso.Name)
                                    .Rate;
                            }
                        }
                    }
                    else
                    {
                        //errors
                        foreach (string error in getShippingOptionResponse.Errors)
                        {
                            result.AddError(error);
                            _logger.Warning(string.Format("Shipping ({0}). {1}", srcm.PluginDescriptor.FriendlyName, error));
                        }
                        //clear the shipping options in this case
                        srcmShippingOptions = new List<ShippingOption>();
                        break;
                    }
                }

                // add this scrm's options to the result
                if (srcmShippingOptions != null)
                {
                    foreach (var so in srcmShippingOptions)
                    {
                        //set system name if not set yet
                        if (String.IsNullOrEmpty(so.ShippingRateComputationMethodSystemName))
                            so.ShippingRateComputationMethodSystemName = srcm.PluginDescriptor.SystemName;
                        if (_shoppingCartSettings.RoundPricesDuringCalculation)
                        {
                            var currency = await _currencyService.GetPrimaryExchangeRateCurrency();
                            so.Rate = RoundingHelper.RoundPrice(so.Rate, currency);
                        }
                        result.ShippingOptions.Add(so);
                    }
                }
            }

            if (_shippingSettings.ReturnValidOptionsIfThereAreAny)
            {
                //return valid options if there are any (no matter of the errors returned by other shipping rate compuation methods).
                if (!result.ShippingOptions.Any() && !result.Errors.Any())
                    result.Errors.Clear();
            }

            //no shipping options loaded
            if (result.ShippingOptions.Count == 0 && result.Errors.Count == 0)
                result.Errors.Add(_localizationService.GetResource("Checkout.ShippingOptionCouldNotBeLoaded"));

            return result;
        }

        #endregion

        #endregion
    }
}
