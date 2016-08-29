using System;
using System.Collections.Generic;
using System.Linq;
using Grand.Core;
using Grand.Core.Caching;
using Grand.Core.Data;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Shipping;
using Grand.Core.Plugins;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Events;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Orders;
using Grand.Core.Infrastructure;
using Grand.Services.Directory;
using MongoDB.Bson;
using MongoDB.Driver;

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
        private const string WAREHOUSES_BY_ID_KEY = "Nop.warehouse.id-{0}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string WAREHOUSES_PATTERN_KEY = "Nop.warehouse.";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string SHIPPINGMETHOD_PATTERN_KEY = "Nop.shippingmethod.";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PICKUPPOINTS_PATTERN_KEY = "Nop.pickuppoint.";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string PRODUCTS_PATTERN_KEY = "Nop.product.";

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
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IAddressService _addressService;
        private readonly ShippingSettings _shippingSettings;
        private readonly IPluginFinder _pluginFinder;
        private readonly IStoreContext _storeContext;
        private readonly IEventPublisher _eventPublisher;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly ICacheManager _cacheManager;
        private readonly IRepository<Product> _productRepository;


        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="shippingMethodRepository">Shipping method repository</param>
        /// <param name="deliveryDateRepository">Delivery date repository</param>
        /// <param name="warehouseRepository">Warehouse repository</param>
        /// <param name="pickupPointsRepository">Pickup points repository</param>
        /// <param name="logger">Logger</param>
        /// <param name="productService">Product service</param>
        /// <param name="productAttributeParser">Product attribute parser</param>
        /// <param name="checkoutAttributeParser">Checkout attribute parser</param>
        /// <param name="genericAttributeService">Generic attribute service</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="addressService">Address service</param>
        /// <param name="shippingSettings">Shipping settings</param>
        /// <param name="pluginFinder">Plugin finder</param>
        /// <param name="storeContext">Store context</param>
        /// <param name="eventPublisher">Event published</param>
        /// <param name="shoppingCartSettings">Shopping cart settings</param>
        /// <param name="cacheManager">Cache manager</param>
        public ShippingService(IRepository<ShippingMethod> shippingMethodRepository,
            IRepository<DeliveryDate> deliveryDateRepository,
            IRepository<Warehouse> warehouseRepository,
            IRepository<PickupPoint> pickupPointsRepository,
            ILogger logger,
            IProductService productService,
            IProductAttributeParser productAttributeParser,
            ICheckoutAttributeParser checkoutAttributeParser,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IAddressService addressService,
            ShippingSettings shippingSettings,
            IPluginFinder pluginFinder,
            IStoreContext storeContext,
            IEventPublisher eventPublisher,
            ShoppingCartSettings shoppingCartSettings,
            ICacheManager cacheManager,
            IRepository<Product> productRepository)
        {
            this._shippingMethodRepository = shippingMethodRepository;
            this._deliveryDateRepository = deliveryDateRepository;
            this._warehouseRepository = warehouseRepository;
            this._pickupPointsRepository = pickupPointsRepository;
            this._logger = logger;
            this._productService = productService;
            this._productAttributeParser = productAttributeParser;
            this._checkoutAttributeParser = checkoutAttributeParser;
            this._genericAttributeService = genericAttributeService;
            this._localizationService = localizationService;
            this._addressService = addressService;
            this._shippingSettings = shippingSettings;
            this._pluginFinder = pluginFinder;
            this._storeContext = storeContext;
            this._eventPublisher = eventPublisher;
            this._shoppingCartSettings = shoppingCartSettings;
            this._cacheManager = cacheManager;
            this._productRepository = productRepository;
        }

        #endregion
        
        #region Methods

        #region Shipping rate computation methods

        /// <summary>
        /// Load active shipping rate computation methods
        /// </summary>
        /// <param name="storeId">Load records allowed only in a specified store; pass 0 to load all records</param>
        /// <returns>Shipping rate computation methods</returns>
        public virtual IList<IShippingRateComputationMethod> LoadActiveShippingRateComputationMethods(string storeId = "")
        {
            return LoadAllShippingRateComputationMethods(storeId)
                   .Where(provider => _shippingSettings.ActiveShippingRateComputationMethodSystemNames.Contains(provider.PluginDescriptor.SystemName, StringComparer.InvariantCultureIgnoreCase))
                   .ToList();
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
                return descriptor.Instance<IShippingRateComputationMethod>();

            return null;
        }

        /// <summary>
        /// Load all shipping rate computation methods
        /// </summary>
        /// <param name="storeId">Load records allowed only in a specified store; pass 0 to load all records</param>
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
        public virtual void DeleteShippingMethod(ShippingMethod shippingMethod)
        {
            if (shippingMethod == null)
                throw new ArgumentNullException("shippingMethod");

            _shippingMethodRepository.Delete(shippingMethod);

            //clear cache
            _cacheManager.RemoveByPattern(SHIPPINGMETHOD_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(shippingMethod);
        }

        /// <summary>
        /// Gets a shipping method
        /// </summary>
        /// <param name="shippingMethodId">The shipping method identifier</param>
        /// <returns>Shipping method</returns>
        public virtual ShippingMethod GetShippingMethodById(string shippingMethodId)
        {
            return _shippingMethodRepository.GetById(shippingMethodId);
        }
        
        /// <summary>
        /// Gets all shipping methods
        /// </summary>
        /// <param name="filterByCountryId">The country indentifier to filter by</param>
        /// <returns>Shipping methods</returns>
        public virtual IList<ShippingMethod> GetAllShippingMethods(string filterByCountryId = "", Customer customer = null)
        {
            List<ShippingMethod> shippingMethods = new List<ShippingMethod>();

            shippingMethods = _cacheManager.Get(SHIPPINGMETHOD_PATTERN_KEY, () =>
            {
                var query = from sm in _shippingMethodRepository.Table
                            orderby sm.DisplayOrder
                            select sm;
                return query.ToList();
            });

            if (!String.IsNullOrEmpty(filterByCountryId))
            {
                shippingMethods = shippingMethods.Where(x => !x.CountryRestrictionExists(filterByCountryId)).ToList();
            }
            if(customer !=null)
            {
                shippingMethods = shippingMethods.Where(x => !x.CustomerRoleRestrictionExists(customer.CustomerRoles.Select(y=>y.Id).ToList())).ToList();
            }

            return shippingMethods;
        }

        /// <summary>
        /// Inserts a shipping method
        /// </summary>
        /// <param name="shippingMethod">Shipping method</param>
        public virtual void InsertShippingMethod(ShippingMethod shippingMethod)
        {
            if (shippingMethod == null)
                throw new ArgumentNullException("shippingMethod");

            _shippingMethodRepository.Insert(shippingMethod);

            //clear cache
            _cacheManager.RemoveByPattern(SHIPPINGMETHOD_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(shippingMethod);
        }

        /// <summary>
        /// Updates the shipping method
        /// </summary>
        /// <param name="shippingMethod">Shipping method</param>
        public virtual void UpdateShippingMethod(ShippingMethod shippingMethod)
        {
            if (shippingMethod == null)
                throw new ArgumentNullException("shippingMethod");

            _shippingMethodRepository.Update(shippingMethod);

            //clear cache
            _cacheManager.RemoveByPattern(SHIPPINGMETHOD_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(shippingMethod);
        }

        #endregion

        #region Delivery dates

        /// <summary>
        /// Deletes a delivery date
        /// </summary>
        /// <param name="deliveryDate">The delivery date</param>
        public virtual void DeleteDeliveryDate(DeliveryDate deliveryDate)
        {
            if (deliveryDate == null)
                throw new ArgumentNullException("deliveryDate");

            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.DeliveryDateId, deliveryDate.Id);
            var update = Builders<Product>.Update
                .Set(x => x.DeliveryDateId, "");
            var result = _productRepository.Collection.UpdateManyAsync(filter, update).Result;

            _deliveryDateRepository.Delete(deliveryDate);
            _cacheManager.RemoveByPattern(PRODUCTS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(deliveryDate);
        }

        /// <summary>
        /// Gets a delivery date
        /// </summary>
        /// <param name="deliveryDateId">The delivery date identifier</param>
        /// <returns>Delivery date</returns>
        public virtual DeliveryDate GetDeliveryDateById(string deliveryDateId)
        {
            return _deliveryDateRepository.GetById(deliveryDateId);
        }

        /// <summary>
        /// Gets all delivery dates
        /// </summary>
        /// <returns>Delivery dates</returns>
        public virtual IList<DeliveryDate> GetAllDeliveryDates()
        {
            var query = from dd in _deliveryDateRepository.Table
                        orderby dd.DisplayOrder
                        select dd;
            var deliveryDates = query.ToList();
            return deliveryDates;
        }

        /// <summary>
        /// Inserts a delivery date
        /// </summary>
        /// <param name="deliveryDate">Delivery date</param>
        public virtual void InsertDeliveryDate(DeliveryDate deliveryDate)
        {
            if (deliveryDate == null)
                throw new ArgumentNullException("deliveryDate");

            _deliveryDateRepository.Insert(deliveryDate);

            //event notification
            _eventPublisher.EntityInserted(deliveryDate);
        }

        /// <summary>
        /// Updates the delivery date
        /// </summary>
        /// <param name="deliveryDate">Delivery date</param>
        public virtual void UpdateDeliveryDate(DeliveryDate deliveryDate)
        {
            if (deliveryDate == null)
                throw new ArgumentNullException("deliveryDate");

            _deliveryDateRepository.Update(deliveryDate);

            //event notification
            _eventPublisher.EntityUpdated(deliveryDate);
        }

        #endregion

        #region Warehouses

        /// <summary>
        /// Deletes a warehouse
        /// </summary>
        /// <param name="warehouse">The warehouse</param>
        public virtual void DeleteWarehouse(Warehouse warehouse)
        {
            if (warehouse == null)
                throw new ArgumentNullException("warehouse");

            var builder = Builders<Product>.Update;
            var updatefilter = builder.PullFilter(x => x.ProductWarehouseInventory, y => y.WarehouseId == warehouse.Id);
            var result = _productRepository.Collection.UpdateManyAsync(new BsonDocument(), updatefilter).Result;

            var builder2 = Builders<Product>.Filter;
            var filter2 = builder2.Eq(x => x.WarehouseId, warehouse.Id);
            var update2 = Builders<Product>.Update
                .Set(x => x.WarehouseId, "");
            var result2 = _productRepository.Collection.UpdateManyAsync(filter2, update2).Result;

            _warehouseRepository.Delete(warehouse);

            //clear cache
            _cacheManager.RemoveByPattern(WAREHOUSES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(warehouse);
        }

        /// <summary>
        /// Gets a warehouse
        /// </summary>
        /// <param name="warehouseId">The warehouse identifier</param>
        /// <returns>Warehouse</returns>
        public virtual Warehouse GetWarehouseById(string warehouseId)
        {
            string key = string.Format(WAREHOUSES_BY_ID_KEY, warehouseId);
            return _cacheManager.Get(key, () => _warehouseRepository.GetById(warehouseId));
        }

        /// <summary>
        /// Gets all warehouses
        /// </summary>
        /// <returns>Warehouses</returns>
        public virtual IList<Warehouse> GetAllWarehouses()
        {
            var query = from wh in _warehouseRepository.Table
                        orderby wh.Name
                        select wh;
            var warehouses = query.ToList();
            return warehouses;
        }

        /// <summary>
        /// Inserts a warehouse
        /// </summary>
        /// <param name="warehouse">Warehouse</param>
        public virtual void InsertWarehouse(Warehouse warehouse)
        {
            if (warehouse == null)
                throw new ArgumentNullException("warehouse");

            _warehouseRepository.Insert(warehouse);

            //clear cache
            _cacheManager.RemoveByPattern(WAREHOUSES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(warehouse);
        }

        /// <summary>
        /// Updates the warehouse
        /// </summary>
        /// <param name="warehouse">Warehouse</param>
        public virtual void UpdateWarehouse(Warehouse warehouse)
        {
            if (warehouse == null)
                throw new ArgumentNullException("warehouse");

            _warehouseRepository.Update(warehouse);

            //clear cache
            _cacheManager.RemoveByPattern(WAREHOUSES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(warehouse);
        }

        #endregion


        #region Pickup points


        /// <summary>
        /// Gets a pickup point
        /// </summary>
        /// <param name="pickupPointId">The pickup point identifier</param>
        /// <returns>Delivery date</returns>
        public virtual PickupPoint GetPickupPointById(string pickupPointId)
        {
            return _pickupPointsRepository.GetById(pickupPointId);
        }

        /// <summary>
        /// Gets all pickup points
        /// </summary>
        /// <returns>Warehouses</returns>
        public virtual IList<PickupPoint> GetAllPickupPoints()
        {
            var query = from pp in _pickupPointsRepository.Table
                        orderby pp.DisplayOrder
                        select pp;
            var pickuppoints = query.ToList();
            return pickuppoints;
        }

        /// <summary>
        /// Gets all pickup points
        /// </summary>
        /// <returns>Warehouses</returns>
        public virtual IList<PickupPoint> LoadActivePickupPoints(string storeId = "")
        {
            var query = from pp in _pickupPointsRepository.Table
                        where pp.StoreId == storeId || String.IsNullOrEmpty(pp.StoreId)
                        orderby pp.DisplayOrder
                        select pp;
            var pickuppoints = query.ToList();
            return pickuppoints;
        }


        /// <summary>
        /// Inserts a warehouse
        /// </summary>
        /// <param name="warehouse">Warehouse</param>
        public virtual void InsertPickupPoint(PickupPoint pickupPoint)
        {
            if (pickupPoint == null)
                throw new ArgumentNullException("pickupPoint");

            _pickupPointsRepository.Insert(pickupPoint);

            //clear cache
            _cacheManager.RemoveByPattern(PICKUPPOINTS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(pickupPoint);
        }

        /// <summary>
        /// Updates the warehouse
        /// </summary>
        /// <param name="warehouse">Warehouse</param>
        public virtual void UpdatePickupPoint(PickupPoint pickupPoint)
        {
            if (pickupPoint == null)
                throw new ArgumentNullException("pickupPoint");

            _pickupPointsRepository.Update(pickupPoint);

            //clear cache
            _cacheManager.RemoveByPattern(WAREHOUSES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(pickupPoint);
        }

        /// <summary>
        /// Deletes a delivery date
        /// </summary>
        /// <param name="deliveryDate">The delivery date</param>
        public virtual void DeletePickupPoint(PickupPoint pickupPoint)
        {
            if (pickupPoint == null)
                throw new ArgumentNullException("pickupPoint");

            _pickupPointsRepository.Delete(pickupPoint);
            _cacheManager.RemoveByPattern(PICKUPPOINTS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(pickupPoint);
        }


        #endregion

        #region Workflow

        /// <summary>
        /// Gets shopping cart item weight (of one item)
        /// </summary>
        /// <param name="shoppingCartItem">Shopping cart item</param>
        /// <returns>Shopping cart item weight</returns>
        public virtual decimal GetShoppingCartItemWeight(ShoppingCartItem shoppingCartItem)
        {
            if (shoppingCartItem == null)
                throw new ArgumentNullException("shoppingCartItem");
            var product = _productService.GetProductById(shoppingCartItem.ProductId);
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
                            var associatedProduct = _productService.GetProductById(attributeValue.AssociatedProductId);
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
        public virtual decimal GetTotalWeight(GetShippingOptionRequest request, bool includeCheckoutAttributes = true)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            Customer customer = request.Customer;

            decimal totalWeight = decimal.Zero;
            //shopping cart items
            foreach (var packageItem in request.Items)
                totalWeight += GetShoppingCartItemWeight(packageItem.ShoppingCartItem) * packageItem.GetQuantity();

            //checkout attributes
            if (customer != null && includeCheckoutAttributes)
            {
                var checkoutAttributesXml = customer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, _storeContext.CurrentStore.Id);
                if (!String.IsNullOrEmpty(checkoutAttributesXml))
                {
                    var attributeValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(checkoutAttributesXml);
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
        public virtual void GetAssociatedProductDimensions(ShoppingCartItem shoppingCartItem,
            out decimal width, out decimal length, out decimal height)
        {
            if (shoppingCartItem == null)
                throw new ArgumentNullException("shoppingCartItem");

            width = length = height = decimal.Zero;

            //attributes
            if (String.IsNullOrEmpty(shoppingCartItem.AttributesXml))
                return;

            var product = _productService.GetProductById(shoppingCartItem.ProductId);
            //bundled products (associated attributes)
            var attributeValues = _productAttributeParser.ParseProductAttributeValues(product, shoppingCartItem.AttributesXml)
                .Where(x => x.AttributeValueType == AttributeValueType.AssociatedToProduct)
                .ToList();
            foreach (var attributeValue in attributeValues)
            {
                var associatedProduct = _productService.GetProductById(attributeValue.AssociatedProductId);
                if (associatedProduct != null && associatedProduct.IsShipEnabled)
                {
                    width += associatedProduct.Width*attributeValue.Quantity;
                    length += associatedProduct.Length * attributeValue.Quantity;
                    height += associatedProduct.Height*attributeValue.Quantity;
                }
            }
        }

        /// <summary>
        /// Get total dimensions
        /// </summary>
        /// <param name="packageItems">Package items</param>
        /// <param name="width">Width</param>
        /// <param name="length">Length</param>
        /// <param name="height">Height</param>
        public virtual void GetDimensions(IList<GetShippingOptionRequest.PackageItem> packageItems,
            out decimal width, out decimal length, out decimal height)
        {
            if (packageItems == null)
                throw new ArgumentNullException("packageItems");

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

                    var product = _productService.GetProductById(shoppingCartItem.ProductId);
                    var qty = packageItem.GetQuantity();

                    //associated products
                    decimal associatedProductsWidth;
                    decimal associatedProductsLength;
                    decimal associatedProductsHeight;
                    GetAssociatedProductDimensions(shoppingCartItem, out associatedProductsWidth,
                        out associatedProductsLength, out associatedProductsHeight);

                    var productWidth = product.Width + associatedProductsWidth;
                    var productLength = product.Length + associatedProductsLength;
                    var productHeight = product.Height + associatedProductsHeight;

                    //we do not use cube root method when we have only one item with "qty" set to 1
                    if (packageItems.Count == 1 && qty == 1)
                    {
                        width = productWidth;
                        length = productLength;
                        height = productHeight;
                        return;
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
                length = width = height = dimension;

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
                    var product = _productService.GetProductById(shoppingCartItem.ProductId);
                    var qty = packageItem.GetQuantity();
                    width += product.Width*qty;
                    length += product.Length*qty;
                    height += product.Height*qty;

                    //associated products
                    decimal associatedProductsWidth;
                    decimal associatedProductsLength;
                    decimal associatedProductsHeight;
                    GetAssociatedProductDimensions(shoppingCartItem, out associatedProductsWidth,
                        out associatedProductsLength, out associatedProductsHeight);

                    width += associatedProductsWidth;
                    length += associatedProductsLength;
                    height += associatedProductsHeight;
                }
            }
        }

        /// <summary>
        /// Get the nearest warehouse for the specified address
        /// </summary>
        /// <param name="address">Address</param>
        /// <param name="warehouses">List of warehouses, if null all warehouses are used.</param>
        /// <returns></returns>
        public virtual Warehouse GetNearestWarehouse(Address address, IList<Warehouse> warehouses = null)
        {
            warehouses = warehouses ?? GetAllWarehouses();

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
                var warehouseAddress = _addressService.GetAddressByIdSettings(warehouse.AddressId);
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
                var warehouseAddress = _addressService.GetAddressByIdSettings(warehouse.AddressId);
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
        /// <param name="storeId">Load records allowed only in a specified store; pass 0 to load all records</param>
        /// <param name="shippingFromMultipleLocations">Value indicating whether shipping is done from multiple locations (warehouses)</param>
        /// <returns>Shipment packages (requests)</returns>
        public virtual IList<GetShippingOptionRequest> CreateShippingOptionRequests(IList<ShoppingCartItem> cart,
            Address shippingAddress, string storeId, out bool shippingFromMultipleLocations)
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

                var product = _productService.GetProductById(sci.ProductId);

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
                            var tmpWarehouse = GetWarehouseById(pwi.WarehouseId);
                            if (tmpWarehouse != null)
                                allWarehouses.Add(tmpWarehouse);
                        }
                        warehouse = GetNearestWarehouse(shippingAddress, allWarehouses);
                    }
                    else
                    {
                        //multiple warehouses are not supported
                        warehouse = GetWarehouseById(product.WarehouseId);
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
                    request.StoreId = storeId;
                    //add item
                    request.Items.Add(new GetShippingOptionRequest.PackageItem(sci));
                    //customer
                    request.Customer = cart.GetCustomer();
                    //ship to
                    request.ShippingAddress = shippingAddress;
                    //ship from
                    Address originAddress = null;
                    if (warehouse != null)
                    {
                        //warehouse address
                        originAddress = _addressService.GetAddressByIdSettings(warehouse.AddressId);
                        request.WarehouseFrom = warehouse;
                    }
                    if (originAddress == null)
                    {
                        //no warehouse address. in this case use the default shipping origin
                        originAddress = _addressService.GetAddressByIdSettings(_shippingSettings.ShippingOriginAddressId);
                    }
                    if (originAddress != null)
                    {
                        var country = EngineContext.Current.Resolve<ICountryService>().GetCountryById(originAddress.CountryId);
                        var state = EngineContext.Current.Resolve<IStateProvinceService>().GetStateProvinceById(originAddress.StateProvinceId);
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
            shippingFromMultipleLocations = requests.Select(x => x.Key).Distinct().Count() > 1;


            var result = requests.Values.ToList();
            result.AddRange(separateRequests);

            return result;
        }

        /// <summary>
        ///  Gets available shipping options
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="shippingAddress">Shipping address</param>
        /// <param name="allowedShippingRateComputationMethodSystemName">Filter by shipping rate computation method identifier; null to load shipping options of all shipping rate computation methods</param>
        /// <param name="storeId">Load records allowed only in a specified store; pass 0 to load all records</param>
        /// <returns>Shipping options</returns>
        public virtual GetShippingOptionResponse GetShippingOptions(IList<ShoppingCartItem> cart,
            Address shippingAddress, string allowedShippingRateComputationMethodSystemName = "", 
            string storeId = "")
        {
            if (cart == null)
                throw new ArgumentNullException("cart");

            var result = new GetShippingOptionResponse();
            
            //create a package
            bool shippingFromMultipleLocations;
            var shippingOptionRequests = CreateShippingOptionRequests(cart, shippingAddress, storeId, out shippingFromMultipleLocations);
            result.ShippingFromMultipleLocations = shippingFromMultipleLocations;

            var shippingRateComputationMethods = LoadActiveShippingRateComputationMethods(storeId);
            //filter by system name
            if (!String.IsNullOrWhiteSpace(allowedShippingRateComputationMethodSystemName))
            {
                shippingRateComputationMethods = shippingRateComputationMethods
                    .Where(srcm => allowedShippingRateComputationMethodSystemName.Equals(srcm.PluginDescriptor.SystemName, StringComparison.InvariantCultureIgnoreCase))
                    .ToList();
            }
            if (!shippingRateComputationMethods.Any())
                throw new NopException("Shipping rate computation method could not be loaded");



            //request shipping options from each shipping rate computation methods
            foreach (var srcm in shippingRateComputationMethods)
            {
                //request shipping options (separately for each package-request)
                IList<ShippingOption> srcmShippingOptions = null;
                foreach (var shippingOptionRequest in shippingOptionRequests)
                {
                    var getShippingOptionResponse = srcm.GetShippingOptions(shippingOptionRequest);

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
                            so.Rate = RoundingHelper.RoundPrice(so.Rate);
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
