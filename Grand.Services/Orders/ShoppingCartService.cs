using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Domain.Orders;
using Grand.Services.Catalog;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Events;
using Grand.Services.Events.Extensions;
using Grand.Services.Localization;
using Grand.Services.Notifications.ShoppingCart;
using Grand.Services.Security;
using Grand.Services.Shipping;
using Grand.Services.Stores;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Orders
{
    /// <summary>
    /// Shopping cart service
    /// </summary>
    public partial class ShoppingCartService : IShoppingCartService
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ICurrencyService _currencyService;
        private readonly IProductService _productService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICustomerService _customerService;
        private readonly IMediator _mediator;
        private readonly IPermissionService _permissionService;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductReservationService _productReservationService;
        private readonly IShippingService _shippingService;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        #endregion

        #region Ctor

        public ShoppingCartService(
            IWorkContext workContext,
            IStoreContext storeContext,
            ICurrencyService currencyService,
            IProductService productService,
            ILocalizationService localizationService,
            IProductAttributeParser productAttributeParser,
            ICheckoutAttributeService checkoutAttributeService,
            ICheckoutAttributeParser checkoutAttributeParser,
            IPriceFormatter priceFormatter,
            ICustomerService customerService,
            IMediator mediator,
            IPermissionService permissionService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IGenericAttributeService genericAttributeService,
            IProductAttributeService productAttributeService,
            ICustomerActionEventService customerActionEventService,
            IProductReservationService productReservationService,
            IShippingService shippingService,
            ShoppingCartSettings shoppingCartSettings)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _currencyService = currencyService;
            _productService = productService;
            _localizationService = localizationService;
            _productAttributeParser = productAttributeParser;
            _checkoutAttributeService = checkoutAttributeService;
            _checkoutAttributeParser = checkoutAttributeParser;
            _priceFormatter = priceFormatter;
            _customerService = customerService;
            _mediator = mediator;
            _permissionService = permissionService;
            _aclService = aclService;
            _storeMappingService = storeMappingService;
            _genericAttributeService = genericAttributeService;
            _productAttributeService = productAttributeService;
            _productReservationService = productReservationService;
            _shippingService = shippingService;
            _shoppingCartSettings = shoppingCartSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete shopping cart item
        /// </summary>
        /// <param name="shoppingCartItem">Shopping cart item</param>
        /// <param name="resetCheckoutData">A value indicating whether to reset checkout data</param>
        /// <param name="ensureOnlyActiveCheckoutAttributes">A value indicating whether to ensure that only active checkout attributes are attached to the current customer</param>
        public virtual async Task DeleteShoppingCartItem(Customer customer, ShoppingCartItem shoppingCartItem, bool resetCheckoutData = true,
            bool ensureOnlyActiveCheckoutAttributes = false)
        {
            if (shoppingCartItem == null)
                throw new ArgumentNullException("shoppingCartItem");

            if (shoppingCartItem.RentalStartDateUtc.HasValue && shoppingCartItem.RentalEndDateUtc.HasValue)
            {
                var reserved = await _productReservationService.GetCustomerReservationsHelperBySciId(shoppingCartItem.Id);
                foreach (var res in reserved)
                {
                    if (res.CustomerId == _workContext.CurrentCustomer.Id)
                    {
                        await _productReservationService.DeleteCustomerReservationsHelper(res);
                    }
                }
            }
            var storeId = shoppingCartItem.StoreId;

            //reset checkout data
            if (resetCheckoutData)
            {
                await _customerService.ResetCheckoutData(customer, shoppingCartItem.StoreId);
            }

            //delete item
            customer.ShoppingCartItems.Remove(customer.ShoppingCartItems.Where(x => x.Id == shoppingCartItem.Id).FirstOrDefault());
            await _customerService.DeleteShoppingCartItem(customer.Id, shoppingCartItem);

            //validate checkout attributes
            if (ensureOnlyActiveCheckoutAttributes &&
                //only for shopping cart items (ignore wishlist)
                shoppingCartItem.ShoppingCartType == ShoppingCartType.ShoppingCart)
            {
                var cart = customer.ShoppingCartItems
                    .Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart)
                    .LimitPerStore(_shoppingCartSettings.CartsSharedBetweenStores, storeId)
                    .ToList();

                var checkoutAttributesXml = await customer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.CheckoutAttributes, storeId);
                checkoutAttributesXml = await _checkoutAttributeParser.EnsureOnlyActiveAttributes(checkoutAttributesXml, cart);
                await _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CheckoutAttributes, checkoutAttributesXml, storeId);
            }

            //event notification
            await _mediator.EntityDeleted(shoppingCartItem);
        }


        /// <summary>
        /// Validates required products (products which require some other products to be added to the cart)
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="shoppingCartType">Shopping cart type</param>
        /// <param name="product">Product</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="automaticallyAddRequiredProductsIfEnabled">Automatically add required products if enabled</param>
        /// <returns>Warnings</returns>
        public virtual async Task<IList<string>> GetRequiredProductWarnings(Customer customer,
            ShoppingCartType shoppingCartType, Product product,
            string storeId, bool automaticallyAddRequiredProductsIfEnabled)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (product == null)
                throw new ArgumentNullException("product");

            var cart = customer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == shoppingCartType)
                .LimitPerStore(_shoppingCartSettings.CartsSharedBetweenStores, storeId)
                .ToList();

            var warnings = new List<string>();

            if (product.RequireOtherProducts)
            {
                var requiredProducts = new List<Product>();
                foreach (var id in product.ParseRequiredProductIds())
                {
                    var rp = await _productService.GetProductById(id);
                    if (rp != null)
                        requiredProducts.Add(rp);
                }

                foreach (var rp in requiredProducts)
                {
                    //ensure that product is in the cart
                    bool alreadyInTheCart = false;
                    foreach (var sci in cart)
                    {
                        if (sci.ProductId == rp.Id)
                        {
                            alreadyInTheCart = true;
                            break;
                        }
                    }
                    //not in the cart
                    if (!alreadyInTheCart)
                    {

                        if (product.AutomaticallyAddRequiredProducts)
                        {
                            //add to cart (if possible)
                            if (automaticallyAddRequiredProductsIfEnabled)
                            {
                                //pass 'false' for 'automaticallyAddRequiredProductsIfEnabled' to prevent circular references
                                var addToCartWarnings = await AddToCart(customer: customer,
                                    productId: rp.Id,
                                    shoppingCartType: shoppingCartType,
                                    storeId: storeId,
                                    automaticallyAddRequiredProductsIfEnabled: false);
                                if (addToCartWarnings.Any())
                                {
                                    //a product wasn't atomatically added for some reasons

                                    //don't display specific errors from 'addToCartWarnings' variable
                                    //display only generic error
                                    warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.RequiredProductWarning"), rp.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id)));
                                }
                            }
                            else
                            {
                                warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.RequiredProductWarning"), rp.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id)));
                            }
                        }
                        else
                        {
                            warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.RequiredProductWarning"), rp.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id)));
                        }
                    }
                }
            }

            return warnings;
        }

        /// <summary>
        /// Validates a product for standard properties
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="product">Product</param>
        /// <param name="shoppingCartItem">Shopping cart item</param>
        /// <returns>Warnings</returns>
        public virtual async Task<IList<string>> GetStandardWarnings(Customer customer, Product product, ShoppingCartItem shoppingCartItem)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (product == null)
                throw new ArgumentNullException("product");

            var warnings = new List<string>();

            //published
            if (!product.Published)
            {
                warnings.Add(_localizationService.GetResource("ShoppingCart.ProductUnpublished"));
            }

            //we can't add grouped product
            if (product.ProductType == ProductType.GroupedProduct)
            {
                warnings.Add("You can't add grouped product");
            }

            //ACL
            if (!_aclService.Authorize(product, customer))
            {
                warnings.Add(_localizationService.GetResource("ShoppingCart.ProductUnpublished"));
            }

            //Store mapping
            if (!_storeMappingService.Authorize(product, shoppingCartItem.StoreId))
            {
                warnings.Add(_localizationService.GetResource("ShoppingCart.ProductUnpublished"));
            }

            //disabled "add to cart" button
            if (shoppingCartItem.ShoppingCartType == ShoppingCartType.ShoppingCart && product.DisableBuyButton)
            {
                warnings.Add(_localizationService.GetResource("ShoppingCart.BuyingDisabled"));
            }

            //disabled "add to wishlist" button
            if (shoppingCartItem.ShoppingCartType == ShoppingCartType.Wishlist && product.DisableWishlistButton)
            {
                warnings.Add(_localizationService.GetResource("ShoppingCart.WishlistDisabled"));
            }

            //call for price
            if (shoppingCartItem.ShoppingCartType == ShoppingCartType.ShoppingCart && product.CallForPrice)
            {
                warnings.Add(_localizationService.GetResource("Products.CallForPrice"));
            }

            //customer entered price
            if (product.CustomerEntersPrice)
            {
                if (shoppingCartItem.CustomerEnteredPrice < product.MinimumCustomerEnteredPrice ||
                    shoppingCartItem.CustomerEnteredPrice > product.MaximumCustomerEnteredPrice)
                {
                    decimal minimumCustomerEnteredPrice = await _currencyService.ConvertFromPrimaryStoreCurrency(product.MinimumCustomerEnteredPrice, _workContext.WorkingCurrency);
                    decimal maximumCustomerEnteredPrice = await _currencyService.ConvertFromPrimaryStoreCurrency(product.MaximumCustomerEnteredPrice, _workContext.WorkingCurrency);
                    warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.CustomerEnteredPrice.RangeError"),
                        _priceFormatter.FormatPrice(minimumCustomerEnteredPrice, false, false),
                        _priceFormatter.FormatPrice(maximumCustomerEnteredPrice, false, false)));
                }
            }

            //quantity validation
            var hasQtyWarnings = false;
            if (shoppingCartItem.Quantity < product.OrderMinimumQuantity)
            {
                warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.MinimumQuantity"), product.OrderMinimumQuantity));
                hasQtyWarnings = true;
            }
            if (shoppingCartItem.Quantity > product.OrderMaximumQuantity)
            {
                warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.MaximumQuantity"), product.OrderMaximumQuantity));
                hasQtyWarnings = true;
            }
            var allowedQuantities = product.ParseAllowedQuantities();
            if (allowedQuantities.Length > 0 && !allowedQuantities.Contains(shoppingCartItem.Quantity))
            {
                warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.AllowedQuantities"), string.Join(", ", allowedQuantities)));
            }

            if (_shoppingCartSettings.AllowToSelectWarehouse && string.IsNullOrEmpty(shoppingCartItem.WarehouseId))
            {
                warnings.Add(_localizationService.GetResource("ShoppingCart.RequiredWarehouse"));
            }

            var warehouseId = !string.IsNullOrEmpty(shoppingCartItem.WarehouseId) ? shoppingCartItem.WarehouseId : _storeContext.CurrentStore?.DefaultWarehouseId;

            if (!string.IsNullOrEmpty(warehouseId))
            {
                var warehouse = await _shippingService.GetWarehouseById(warehouseId);
                if (warehouse == null)
                    warnings.Add(_localizationService.GetResource("ShoppingCart.WarehouseNotExists"));
            }

            var validateOutOfStock = shoppingCartItem.ShoppingCartType == ShoppingCartType.ShoppingCart || !_shoppingCartSettings.AllowOutOfStockItemsToBeAddedToWishlist;
            if (validateOutOfStock && !hasQtyWarnings)
            {
                switch (product.ManageInventoryMethod)
                {
                    case ManageInventoryMethod.DontManageStock:
                        {
                            //do nothing
                        }
                        break;
                    case ManageInventoryMethod.ManageStock:
                        {
                            if (product.BackorderMode == BackorderMode.NoBackorders)
                            {
                                int maximumQuantityCanBeAdded = product.GetTotalStockQuantity(warehouseId: warehouseId);
                                if (maximumQuantityCanBeAdded < shoppingCartItem.Quantity)
                                {
                                    if (maximumQuantityCanBeAdded <= 0)
                                        warnings.Add(_localizationService.GetResource("ShoppingCart.OutOfStock"));
                                    else
                                        warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.QuantityExceedsStock"), maximumQuantityCanBeAdded));
                                }
                            }
                        }
                        break;
                    case ManageInventoryMethod.ManageStockByBundleProducts:
                        {
                            foreach (var item in product.BundleProducts)
                            {
                                var _qty = shoppingCartItem.Quantity * item.Quantity;
                                var p1 = await _productService.GetProductById(item.ProductId);
                                if (p1 != null)
                                {
                                    if (p1.BackorderMode == BackorderMode.NoBackorders)
                                    {
                                        if (p1.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                                        {
                                            int maximumQuantityCanBeAdded = p1.GetTotalStockQuantity(warehouseId: warehouseId);
                                            if (maximumQuantityCanBeAdded < _qty)
                                            {
                                                warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.OutOfStock.BundleProduct"), p1.Name));
                                            }
                                        }
                                        if (p1.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
                                        {
                                            var combination = _productAttributeParser.FindProductAttributeCombination(p1, shoppingCartItem.AttributesXml);
                                            if (combination != null)
                                            {
                                                //combination exists
                                                //let's check stock level
                                                var stockquantity = p1.GetTotalStockQuantityForCombination(combination, warehouseId: warehouseId);
                                                if (!combination.AllowOutOfStockOrders && stockquantity < _qty)
                                                {
                                                    int maximumQuantityCanBeAdded = stockquantity;
                                                    if (maximumQuantityCanBeAdded <= 0)
                                                    {
                                                        warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.OutOfStock.BundleProduct"), p1.Name));
                                                    }
                                                    else
                                                    {
                                                        warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.QuantityExceedsStock.BundleProduct"), p1.Name, maximumQuantityCanBeAdded));
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                //combination doesn't exist
                                                if (p1.AllowAddingOnlyExistingAttributeCombinations)
                                                {
                                                    warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.Combination.BundleProduct.NotExist"), p1.Name));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case ManageInventoryMethod.ManageStockByAttributes:
                        {
                            var combination = _productAttributeParser.FindProductAttributeCombination(product, shoppingCartItem.AttributesXml);
                            if (combination != null)
                            {
                                //combination exists
                                //let's check stock level
                                var stockquantity = product.GetTotalStockQuantityForCombination(combination, warehouseId: warehouseId);
                                if (!combination.AllowOutOfStockOrders && stockquantity < shoppingCartItem.Quantity)
                                {
                                    int maximumQuantityCanBeAdded = stockquantity;
                                    if (maximumQuantityCanBeAdded <= 0)
                                    {
                                        warnings.Add(_localizationService.GetResource("ShoppingCart.OutOfStock"));
                                    }
                                    else
                                    {
                                        warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.QuantityExceedsStock"), maximumQuantityCanBeAdded));
                                    }
                                }
                            }
                            else
                            {
                                //combination doesn't exist
                                if (product.AllowAddingOnlyExistingAttributeCombinations)
                                {
                                    warnings.Add(_localizationService.GetResource("ShoppingCart.Combination.NotExist"));
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            //availability dates
            bool availableStartDateError = false;
            if (product.AvailableStartDateTimeUtc.HasValue)
            {
                DateTime now = DateTime.UtcNow;
                DateTime availableStartDateTime = DateTime.SpecifyKind(product.AvailableStartDateTimeUtc.Value, DateTimeKind.Utc);
                if (availableStartDateTime.CompareTo(now) > 0)
                {
                    warnings.Add(_localizationService.GetResource("ShoppingCart.NotAvailable"));
                    availableStartDateError = true;
                }
            }
            if (product.AvailableEndDateTimeUtc.HasValue && !availableStartDateError && shoppingCartItem.ShoppingCartType == ShoppingCartType.ShoppingCart)
            {
                DateTime now = DateTime.UtcNow;
                DateTime availableEndDateTime = DateTime.SpecifyKind(product.AvailableEndDateTimeUtc.Value, DateTimeKind.Utc);
                if (availableEndDateTime.CompareTo(now) < 0)
                {
                    warnings.Add(_localizationService.GetResource("ShoppingCart.NotAvailable"));
                }
            }
            return warnings;
        }

        /// <summary>
        /// Gets shopping cart
        /// </summary>
        /// <param name="storeId">Store identifier; pass null to load all records</param>
        /// <param name="shoppingCartType">Shopping cart type; pass null to load all records</param>
        /// <returns>Shopping Cart</returns>
        public IList<ShoppingCartItem> GetShoppingCart(string storeId = null, params ShoppingCartType[] shoppingCartType)
        {
            IEnumerable<ShoppingCartItem> cart = _workContext.CurrentCustomer.ShoppingCartItems;

            if (!string.IsNullOrEmpty(storeId))
                cart = cart.LimitPerStore(_shoppingCartSettings.CartsSharedBetweenStores, storeId);

            if (shoppingCartType.Length > 0)
                cart = cart.Where(sci => shoppingCartType.Contains(sci.ShoppingCartType));

            return cart.ToList();
        }

        /// <summary>
        /// Validates shopping cart item attributes
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="shoppingCartType">Shopping cart type</param>
        /// <param name="product">Product</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="ignoreNonCombinableAttributes">A value indicating whether we should ignore non-combinable attributes</param>
        /// <returns>Warnings</returns>
        public virtual async Task<IList<string>> GetShoppingCartItemAttributeWarnings(Customer customer,
            ShoppingCartType shoppingCartType,
            Product product,
            int quantity = 1,
            string attributesXml = "",
            bool ignoreNonCombinableAttributes = false)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            var warnings = new List<string>();

            //ensure it's our attributes
            var attributes1 = _productAttributeParser.ParseProductAttributeMappings(product, attributesXml).ToList();
            if (product.ProductType == ProductType.BundledProduct)
            {
                foreach (var bundle in product.BundleProducts)
                {
                    var p1 = await _productService.GetProductById(bundle.ProductId);
                    if (p1 != null)
                    {
                        var a1 = _productAttributeParser.ParseProductAttributeMappings(p1, attributesXml).ToList();
                        attributes1.AddRange(a1);
                    }
                }

            }
            if (ignoreNonCombinableAttributes)
            {
                attributes1 = attributes1.Where(x => !x.IsNonCombinable()).ToList();

            }
            foreach (var attribute in attributes1)
            {
                if (string.IsNullOrEmpty(attribute.ProductId))
                {
                    warnings.Add("Attribute error");
                    return warnings;
                }
            }

            //validate required product attributes (whether they're chosen/selected/entered)
            var attributes2 = product.ProductAttributeMappings.ToList();
            if (product.ProductType == ProductType.BundledProduct)
            {
                foreach (var bundle in product.BundleProducts)
                {
                    var p1 = await _productService.GetProductById(bundle.ProductId);
                    if (p1 != null && p1.ProductAttributeMappings.Any())
                    {
                        attributes2.AddRange(p1.ProductAttributeMappings);
                    }
                }
            }
            if (ignoreNonCombinableAttributes)
            {
                attributes2 = attributes2.Where(x => !x.IsNonCombinable()).ToList();
            }
            //validate conditional attributes only (if specified)
            attributes2 = attributes2.Where(x =>
            {
                var conditionMet = _productAttributeParser.IsConditionMet(product, x, attributesXml);
                return !conditionMet.HasValue || conditionMet.Value;
            }).ToList();
            foreach (var a2 in attributes2)
            {
                if (a2.IsRequired)
                {
                    bool found = false;
                    //selected product attributes
                    foreach (var a1 in attributes1)
                    {
                        if (a1.Id == a2.Id)
                        {
                            var attributeValuesStr = _productAttributeParser.ParseValues(attributesXml, a1.Id);
                            foreach (string str1 in attributeValuesStr)
                            {
                                if (!String.IsNullOrEmpty(str1.Trim()))
                                {
                                    found = true;
                                    break;
                                }
                            }
                        }
                    }

                    //if not found
                    if (!found)
                    {
                        var paa = await _productAttributeService.GetProductAttributeById(a2.ProductAttributeId);
                        var notFoundWarning = !string.IsNullOrEmpty(a2.TextPrompt) ?
                            a2.TextPrompt :
                            string.Format(_localizationService.GetResource("ShoppingCart.SelectAttribute"), paa.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id));

                        warnings.Add(notFoundWarning);
                    }
                }

                if (a2.AttributeControlType == AttributeControlType.ReadonlyCheckboxes)
                {
                    //customers cannot edit read-only attributes
                    var allowedReadOnlyValueIds = a2.ProductAttributeValues.Where(x => x.Id == a2.Id)
                        .Where(x => x.IsPreSelected)
                        .Select(x => x.Id)
                        .ToArray();

                    var selectedReadOnlyValueIds = _productAttributeParser.ParseProductAttributeValues(product, attributesXml)
                        .Where(x => x.ProductAttributeMappingId == a2.Id)
                        .Select(x => x.Id)
                        .ToArray();

                    if (!CommonHelper.ArraysEqual(allowedReadOnlyValueIds, selectedReadOnlyValueIds))
                    {
                        warnings.Add("You cannot change read-only values");
                    }
                }
            }

            //validation rules
            foreach (var pam in attributes2)
            {
                if (!pam.ValidationRulesAllowed())
                    continue;

                //minimum length
                if (pam.ValidationMinLength.HasValue)
                {
                    if (pam.AttributeControlType == AttributeControlType.TextBox ||
                        pam.AttributeControlType == AttributeControlType.MultilineTextbox)
                    {
                        var valuesStr = _productAttributeParser.ParseValues(attributesXml, pam.Id);
                        var enteredText = valuesStr.FirstOrDefault();
                        int enteredTextLength = String.IsNullOrEmpty(enteredText) ? 0 : enteredText.Length;

                        if (pam.ValidationMinLength.Value > enteredTextLength)
                        {
                            var _pam = await _productAttributeService.GetProductAttributeById(pam.ProductAttributeId);
                            warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.TextboxMinimumLength"), _pam.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id), pam.ValidationMinLength.Value));
                        }
                    }
                }

                //maximum length
                if (pam.ValidationMaxLength.HasValue)
                {
                    if (pam.AttributeControlType == AttributeControlType.TextBox ||
                        pam.AttributeControlType == AttributeControlType.MultilineTextbox)
                    {
                        var valuesStr = _productAttributeParser.ParseValues(attributesXml, pam.Id);
                        var enteredText = valuesStr.FirstOrDefault();
                        int enteredTextLength = String.IsNullOrEmpty(enteredText) ? 0 : enteredText.Length;

                        if (pam.ValidationMaxLength.Value < enteredTextLength)
                        {
                            var _pam = await _productAttributeService.GetProductAttributeById(pam.ProductAttributeId);
                            warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.TextboxMaximumLength"), _pam.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id), pam.ValidationMaxLength.Value));
                        }
                    }
                }
            }

            if (warnings.Any())
                return warnings;

            //validate bundled products
            var attributeValues = _productAttributeParser.ParseProductAttributeValues(product, attributesXml);
            foreach (var attributeValue in attributeValues)
            {
                var _productAttributeMapping = product.ProductAttributeMappings.Where(x => x.Id == attributeValue.ProductAttributeMappingId).FirstOrDefault();
                if (attributeValue.AttributeValueType == AttributeValueType.AssociatedToProduct)
                {
                    if (ignoreNonCombinableAttributes && _productAttributeMapping.IsNonCombinable())
                        continue;

                    //associated product (bundle)
                    var associatedProduct = await _productService.GetProductById(attributeValue.AssociatedProductId);
                    if (associatedProduct != null)
                    {
                        var totalQty = quantity * attributeValue.Quantity;
                        var associatedProductWarnings = await GetShoppingCartItemWarnings(customer, new ShoppingCartItem() { ShoppingCartType = shoppingCartType, StoreId = _storeContext.CurrentStore.Id, Quantity = totalQty }, associatedProduct, false);
                        foreach (var associatedProductWarning in associatedProductWarnings)
                        {
                            var productAttribute = await _productAttributeService.GetProductAttributeById(_productAttributeMapping.ProductAttributeId);
                            var attributeName = productAttribute.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id);
                            var attributeValueName = attributeValue.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id);
                            warnings.Add(string.Format(
                                _localizationService.GetResource("ShoppingCart.AssociatedAttributeWarning"),
                                attributeName, attributeValueName, associatedProductWarning));
                        }
                    }
                    else
                    {
                        warnings.Add(string.Format("Associated product cannot be loaded - {0}", attributeValue.AssociatedProductId));
                    }
                }
            }

            return warnings;
        }

        /// <summary>
        /// Validates shopping cart item (gift card)
        /// </summary>
        /// <param name="shoppingCartType">Shopping cart type</param>
        /// <param name="product">Product</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <returns>Warnings</returns>
        public virtual IList<string> GetShoppingCartItemGiftCardWarnings(ShoppingCartType shoppingCartType,
            Product product, string attributesXml)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            var warnings = new List<string>();

            //gift cards
            if (product.IsGiftCard)
            {
                _productAttributeParser.GetGiftCardAttribute(attributesXml,
                    out string giftCardRecipientName, out string giftCardRecipientEmail,
                    out string giftCardSenderName, out string giftCardSenderEmail, out string giftCardMessage);

                if (String.IsNullOrEmpty(giftCardRecipientName))
                    warnings.Add(_localizationService.GetResource("ShoppingCart.RecipientNameError"));

                if (product.GiftCardType == GiftCardType.Virtual)
                {
                    //validate for virtual gift cards only
                    if (String.IsNullOrEmpty(giftCardRecipientEmail) || !CommonHelper.IsValidEmail(giftCardRecipientEmail))
                        warnings.Add(_localizationService.GetResource("ShoppingCart.RecipientEmailError"));
                }

                if (String.IsNullOrEmpty(giftCardSenderName))
                    warnings.Add(_localizationService.GetResource("ShoppingCart.SenderNameError"));

                if (product.GiftCardType == GiftCardType.Virtual)
                {
                    //validate for virtual gift cards only
                    if (String.IsNullOrEmpty(giftCardSenderEmail) || !CommonHelper.IsValidEmail(giftCardSenderEmail))
                        warnings.Add(_localizationService.GetResource("ShoppingCart.SenderEmailError"));
                }
            }

            return warnings;
        }


        /// <summary>
        /// Validate bid 
        /// </summary>
        /// <param name="bid"></param>
        /// <param name="product"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        public virtual IList<string> GetAuctionProductWarning(decimal bid, Product product, Customer customer)
        {
            var warnings = new List<string>();
            if (bid <= product.HighestBid || bid <= product.StartPrice)
            {
                warnings.Add(_localizationService.GetResource("ShoppingCart.BidMustBeHigher"));
            }

            if (!product.AvailableEndDateTimeUtc.HasValue)
            {
                warnings.Add(_localizationService.GetResource("ShoppingCart.NotAvailable"));
            }

            if (product.AvailableEndDateTimeUtc < DateTime.UtcNow)
            {
                warnings.Add(_localizationService.GetResource("ShoppingCart.NotAvailable"));
            }

            return warnings;
        }

        /// <summary>
        /// Validates shopping cart item for reservation products
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="product">Product</param>
        /// <param name="shoppingCartItem">ShoppingCartItem</param>
        /// <returns>Warnings</returns>
        public virtual async Task<IList<string>> GetReservationProductWarnings(Customer customer, Product product, ShoppingCartItem shoppingCartItem)
        {
            var warnings = new List<string>();

            if (product.ProductType != ProductType.Reservation)
                return warnings;

            if (string.IsNullOrEmpty(shoppingCartItem.ReservationId) && product.IntervalUnitType != IntervalUnit.Day)
            {
                warnings.Add(_localizationService.GetResource("ShoppingCart.Reservation.NoReservationFound"));
                return warnings;
            }

            if (product.IntervalUnitType != IntervalUnit.Day)
            {
                var reservation = await _productReservationService.GetProductReservation(shoppingCartItem.ReservationId);
                if (reservation == null)
                {
                    warnings.Add(_localizationService.GetResource("ShoppingCart.Reservation.ReservationDeleted"));
                }
                else
                {
                    if (!string.IsNullOrEmpty(reservation.OrderId))
                    {
                        warnings.Add(_localizationService.GetResource("ShoppingCart.Reservation.AlreadyReserved"));
                    }
                }
            }
            else
            {
                if (!(shoppingCartItem.RentalStartDateUtc.HasValue && shoppingCartItem.RentalEndDateUtc.HasValue))
                {
                    warnings.Add(_localizationService.GetResource("ShoppingCart.Reservation.ChoosebothDates"));
                }
                else
                {
                    if (!product.IncBothDate)
                    {
                        if (shoppingCartItem.RentalStartDateUtc.Value >= shoppingCartItem.RentalEndDateUtc.Value)
                        {
                            warnings.Add(_localizationService.GetResource("ShoppingCart.Reservation.EndDateMustBeLaterThanStartDate"));
                        }
                    }
                    else
                    {
                        if (shoppingCartItem.RentalStartDateUtc.Value > shoppingCartItem.RentalEndDateUtc.Value)
                        {
                            warnings.Add(_localizationService.GetResource("ShoppingCart.Reservation.EndDateMustBeLaterThanStartDate"));
                        }
                    }

                    if (shoppingCartItem.RentalStartDateUtc.Value < DateTime.Now || shoppingCartItem.RentalEndDateUtc.Value < DateTime.Now)
                    {
                        warnings.Add(_localizationService.GetResource("ShoppingCart.Reservation.ReservationDatesMustBeLaterThanToday"));
                    }

                    if (customer.ShoppingCartItems.Any(x => x.Id == shoppingCartItem.Id))
                    {
                        var reserved = await _productReservationService.GetCustomerReservationsHelperBySciId(shoppingCartItem.Id);
                        if (!reserved.Any())
                            warnings.Add(_localizationService.GetResource("ShoppingCart.Reservation.ReservationDeleted"));
                        else
                            foreach (var item in reserved)
                            {
                                var reservation = await _productReservationService.GetProductReservation(item.ReservationId);
                                if (reservation == null)
                                {
                                    warnings.Add(_localizationService.GetResource("ShoppingCart.Reservation.ReservationDeleted"));
                                    break;
                                }
                                else if (!string.IsNullOrEmpty(reservation.OrderId))
                                {
                                    warnings.Add(_localizationService.GetResource("ShoppingCart.Reservation.AlreadyReserved"));
                                    break;
                                }
                            }
                    }
                }
            }

            return warnings;
        }

        /// <summary>
        /// Validates shopping cart item
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="shoppingCartType">Shopping cart type</param>
        /// <param name="product">Product</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="customerEnteredPrice">Customer entered price</param>
        /// <param name="rentalStartDate">Rental start date</param>
        /// <param name="rentalEndDate">Rental end date</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="automaticallyAddRequiredProductsIfEnabled">Automatically add required products if enabled</param>
        /// <param name="getStandardWarnings">A value indicating whether we should validate a product for standard properties</param>
        /// <param name="getAttributesWarnings">A value indicating whether we should validate product attributes</param>
        /// <param name="getGiftCardWarnings">A value indicating whether we should validate gift card properties</param>
        /// <param name="getRequiredProductWarnings">A value indicating whether we should validate required products (products which require other products to be added to the cart)</param>
        /// <param name="getRentalWarnings">A value indicating whether we should validate rental properties</param>
        /// <returns>Warnings</returns>
        public virtual async Task<IList<string>> GetShoppingCartItemWarnings(Customer customer, ShoppingCartItem shoppingCartItem,
            Product product, bool automaticallyAddRequiredProductsIfEnabled = true,
            bool getStandardWarnings = true, bool getAttributesWarnings = true,
            bool getGiftCardWarnings = true, bool getRequiredProductWarnings = true,
            bool getRentalWarnings = true, bool getReservationWarnings = true)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            var warnings = new List<string>();

            //standard properties
            if (getStandardWarnings)
                warnings.AddRange(await GetStandardWarnings(customer, product, shoppingCartItem));

            //selected attributes
            if (getAttributesWarnings)
                warnings.AddRange(await GetShoppingCartItemAttributeWarnings(customer, shoppingCartItem.ShoppingCartType, product, shoppingCartItem.Quantity, shoppingCartItem.AttributesXml));

            //gift cards
            if (getGiftCardWarnings)
                warnings.AddRange(GetShoppingCartItemGiftCardWarnings(shoppingCartItem.ShoppingCartType, product, shoppingCartItem.AttributesXml));

            //required products
            if (getRequiredProductWarnings)
                warnings.AddRange(await GetRequiredProductWarnings(customer, shoppingCartItem.ShoppingCartType, product, shoppingCartItem.StoreId, automaticallyAddRequiredProductsIfEnabled));

            //reservation products
            if (getReservationWarnings)
                warnings.AddRange(await GetReservationProductWarnings(customer, product, shoppingCartItem));

            //event notification
            await _mediator.ShoppingCartItemWarningsAdded(warnings, customer, shoppingCartItem, product);

            return warnings;
        }

        /// <summary>
        /// Validates whether this shopping cart is valid
        /// </summary>
        /// <param name="shoppingCart">Shopping cart</param>
        /// <param name="checkoutAttributesXml">Checkout attributes in XML format</param>
        /// <param name="validateCheckoutAttributes">A value indicating whether to validate checkout attributes</param>
        /// <returns>Warnings</returns>
        public virtual async Task<IList<string>> GetShoppingCartWarnings(IList<ShoppingCartItem> shoppingCart,
            string checkoutAttributesXml, bool validateCheckoutAttributes)
        {
            var warnings = new List<string>();

            bool hasStandartProducts = false;
            bool hasRecurringProducts = false;

            foreach (var sci in shoppingCart)
            {
                var product = await _productService.GetProductById(sci.ProductId);
                if (product == null)
                {
                    warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.CannotLoadProduct"), sci.ProductId));
                    return warnings;
                }

                if (product.IsRecurring)
                    hasRecurringProducts = true;
                else
                    hasStandartProducts = true;
            }

            //don't mix standard and recurring products
            if (hasStandartProducts && hasRecurringProducts)
                warnings.Add(_localizationService.GetResource("ShoppingCart.CannotMixStandardAndAutoshipProducts"));

            //recurring cart validation
            if (hasRecurringProducts)
            {
                var cyclesInfo = await shoppingCart.GetRecurringCycleInfo(_localizationService, _productService);
                string cyclesError = cyclesInfo.info;
                int cycleLength = cyclesInfo.cycleLength;
                RecurringProductCyclePeriod cyclePeriod = cyclesInfo.cyclePeriod;
                int totalCycles = cyclesInfo.totalCycles;

                if (!string.IsNullOrEmpty(cyclesError))
                {
                    warnings.Add(cyclesError);
                    return warnings;
                }
            }

            //validate checkout attributes
            if (validateCheckoutAttributes)
            {
                //selected attributes
                var attributes1 = await _checkoutAttributeParser.ParseCheckoutAttributes(checkoutAttributesXml);

                //existing checkout attributes
                var attributes2 = await _checkoutAttributeService.GetAllCheckoutAttributes(_storeContext.CurrentStore.Id, !shoppingCart.RequiresShipping());
                foreach (var a2 in attributes2)
                {
                    var conditionMet = await _checkoutAttributeParser.IsConditionMet(a2, checkoutAttributesXml);
                    if (a2.IsRequired && ((conditionMet.HasValue && conditionMet.Value) || !conditionMet.HasValue))
                    {
                        bool found = false;
                        //selected checkout attributes
                        foreach (var a1 in attributes1)
                        {
                            if (a1.Id == a2.Id)
                            {
                                var attributeValuesStr = _checkoutAttributeParser.ParseValues(checkoutAttributesXml, a1.Id);
                                foreach (string str1 in attributeValuesStr)
                                    if (!String.IsNullOrEmpty(str1.Trim()))
                                    {
                                        found = true;
                                        break;
                                    }
                            }
                        }

                        //if not found
                        if (!found)
                        {
                            if (!string.IsNullOrEmpty(a2.GetLocalized(a => a.TextPrompt, _workContext.WorkingLanguage.Id)))
                                warnings.Add(a2.GetLocalized(a => a.TextPrompt, _workContext.WorkingLanguage.Id));
                            else
                                warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.SelectAttribute"), a2.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id)));
                        }
                    }
                }

                //now validation rules

                //minimum length
                foreach (var ca in attributes2)
                {
                    if (ca.ValidationMinLength.HasValue)
                    {
                        if (ca.AttributeControlType == AttributeControlType.TextBox ||
                            ca.AttributeControlType == AttributeControlType.MultilineTextbox)
                        {
                            var valuesStr = _checkoutAttributeParser.ParseValues(checkoutAttributesXml, ca.Id);
                            var enteredText = valuesStr.FirstOrDefault();
                            int enteredTextLength = String.IsNullOrEmpty(enteredText) ? 0 : enteredText.Length;

                            if (ca.ValidationMinLength.Value > enteredTextLength)
                            {
                                warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.TextboxMinimumLength"), ca.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id), ca.ValidationMinLength.Value));
                            }
                        }
                    }

                    //maximum length
                    if (ca.ValidationMaxLength.HasValue)
                    {
                        if (ca.AttributeControlType == AttributeControlType.TextBox ||
                            ca.AttributeControlType == AttributeControlType.MultilineTextbox)
                        {
                            var valuesStr = _checkoutAttributeParser.ParseValues(checkoutAttributesXml, ca.Id);
                            var enteredText = valuesStr.FirstOrDefault();
                            int enteredTextLength = String.IsNullOrEmpty(enteredText) ? 0 : enteredText.Length;

                            if (ca.ValidationMaxLength.Value < enteredTextLength)
                            {
                                warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.TextboxMaximumLength"), ca.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id), ca.ValidationMaxLength.Value));
                            }
                        }
                    }
                }
            }

            //event notification
            await _mediator.ShoppingCartWarningsAdd(warnings, shoppingCart, checkoutAttributesXml, validateCheckoutAttributes);

            return warnings;
        }

        /// <summary>
        /// Finds a shopping cart item in the cart
        /// </summary>
        /// <param name="shoppingCart">Shopping cart</param>
        /// <param name="shoppingCartType">Shopping cart type</param>
        /// <param name="product">Product</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="customerEnteredPrice">Price entered by a customer</param>
        /// <param name="rentalStartDate">Rental start date</param>
        /// <param name="rentalEndDate">Rental end date</param>
        /// <returns>Found shopping cart item</returns>
        public virtual async Task<ShoppingCartItem> FindShoppingCartItemInTheCart(IList<ShoppingCartItem> shoppingCart,
            ShoppingCartType shoppingCartType,
            string productId,
            string warehouseId = null,
            string attributesXml = "",
            decimal customerEnteredPrice = decimal.Zero,
            DateTime? rentalStartDate = null,
            DateTime? rentalEndDate = null)
        {
            if (shoppingCart == null)
                throw new ArgumentNullException("shoppingCart");

            foreach (var sci in shoppingCart.Where(a => a.ShoppingCartType == shoppingCartType))
            {
                if (sci.ProductId == productId && sci.WarehouseId == warehouseId)
                {
                    //attributes
                    var _product = await _productService.GetProductById(sci.ProductId);
                    bool attributesEqual = _productAttributeParser.AreProductAttributesEqual(_product, sci.AttributesXml, attributesXml, false);
                    if (_product.ProductType == ProductType.BundledProduct)
                    {
                        foreach (var bundle in _product.BundleProducts)
                        {
                            var p1 = await _productService.GetProductById(bundle.ProductId);
                            if (p1 != null)
                            {
                                if (!_productAttributeParser.AreProductAttributesEqual(p1, sci.AttributesXml, attributesXml, false))
                                    attributesEqual = false;
                            }
                        }
                    }
                    //gift cards
                    bool giftCardInfoSame = true;
                    if (_product.IsGiftCard)
                    {
                        _productAttributeParser.GetGiftCardAttribute(attributesXml,
                            out var giftCardRecipientName1, out var giftCardRecipientEmail1,
                            out var giftCardSenderName1, out var giftCardSenderEmail1, out var giftCardMessage1);

                        _productAttributeParser.GetGiftCardAttribute(sci.AttributesXml,
                            out var giftCardRecipientName2, out var giftCardRecipientEmail2,
                            out var giftCardSenderName2, out var giftCardSenderEmail2, out var giftCardMessage2);

                        if (giftCardRecipientName1.ToLowerInvariant() != giftCardRecipientName2.ToLowerInvariant() ||
                            giftCardSenderName1.ToLowerInvariant() != giftCardSenderName2.ToLowerInvariant())
                            giftCardInfoSame = false;
                    }

                    //price is the same (for products which require customers to enter a price)
                    bool customerEnteredPricesEqual = true;
                    if (_product.CustomerEntersPrice)
                        customerEnteredPricesEqual = Math.Round(sci.CustomerEnteredPrice, 2) == Math.Round(customerEnteredPrice, 2);

                    //found?
                    if (attributesEqual && giftCardInfoSame && customerEnteredPricesEqual)
                        return sci;
                }
            }

            return null;
        }

        /// <summary>
        /// Add a product to shopping cart
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="product">Product</param>
        /// <param name="shoppingCartType">Shopping cart type</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="customerEnteredPrice">The price enter by a customer</param>
        /// <param name="rentalStartDate">Rental start date</param>
        /// <param name="rentalEndDate">Rental end date</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="automaticallyAddRequiredProductsIfEnabled">Automatically add required products if enabled</param>
        /// <returns>Warnings</returns>
        public virtual async Task<IList<string>> AddToCart(Customer customer, string productId,
            ShoppingCartType shoppingCartType, string storeId, string warehouseId = null, string attributesXml = null,
            decimal customerEnteredPrice = decimal.Zero,
            DateTime? rentalStartDate = null, DateTime? rentalEndDate = null,
            int quantity = 1, bool automaticallyAddRequiredProductsIfEnabled = true,
            string reservationId = "", string parameter = "", string duration = "")
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var product = await _productService.GetProductById(productId);
            if (product == null)
                throw new ArgumentNullException("product");

            if (string.IsNullOrEmpty(productId))
                throw new ArgumentNullException("product");

            var warnings = new List<string>();
            if (shoppingCartType == ShoppingCartType.ShoppingCart && !await _permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart, customer))
            {
                warnings.Add("Shopping cart is disabled");
                return warnings;
            }
            if (shoppingCartType == ShoppingCartType.Wishlist && !await _permissionService.Authorize(StandardPermissionProvider.EnableWishlist, customer))
            {
                warnings.Add("Wishlist is disabled");
                return warnings;
            }
            if (customer.IsSearchEngineAccount())
            {
                warnings.Add("Search engine can't add to cart");
                return warnings;
            }

            if (quantity <= 0)
            {
                warnings.Add(_localizationService.GetResource("ShoppingCart.QuantityShouldPositive"));
                return warnings;
            }

            IGrouping<string, ProductReservation> groupToBook = null;
            if (rentalStartDate.HasValue && rentalEndDate.HasValue)
            {
                var reservations = await _productReservationService.GetProductReservationsByProductId(product.Id, true, null);
                var reserved = await _productReservationService.GetCustomerReservationsHelpers(_workContext.CurrentCustomer.Id);
                foreach (var item in reserved)
                {
                    var match = reservations.Where(x => x.Id == item.ReservationId).FirstOrDefault();
                    if (match != null)
                    {
                        reservations.Remove(match);
                    }
                }

                var grouped = reservations.GroupBy(x => x.Resource);
                foreach (var group in grouped)
                {
                    bool groupCanBeBooked = true;
                    if (product.IncBothDate && product.IntervalUnitType == IntervalUnit.Day)
                    {
                        for (DateTime iterator = rentalStartDate.Value; iterator <= rentalEndDate.Value; iterator += new TimeSpan(24, 0, 0))
                        {
                            if (!group.Select(x => x.Date).Contains(iterator))
                            {
                                groupCanBeBooked = false;
                                break;
                            }
                        }
                    }
                    else
                    {
                        for (DateTime iterator = rentalStartDate.Value; iterator < rentalEndDate.Value; iterator += new TimeSpan(24, 0, 0))
                        {
                            if (!group.Select(x => x.Date).Contains(iterator))
                            {
                                groupCanBeBooked = false;
                                break;
                            }
                        }
                    }
                    if (groupCanBeBooked)
                    {
                        groupToBook = group;
                        break;
                    }
                }

                if (groupToBook == null)
                {
                    warnings.Add(_localizationService.GetResource("ShoppingCart.Reservation.NoFreeReservationsInThisPeriod"));
                    return warnings;
                }
            }

            //reset checkout info
            await _customerService.ResetCheckoutData(customer, storeId);

            var cart = customer.ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == shoppingCartType)
                .LimitPerStore(_shoppingCartSettings.CartsSharedBetweenStores, storeId)
                .ToList();

            var shoppingCartItem = await FindShoppingCartItemInTheCart(cart,
                shoppingCartType, productId, warehouseId, attributesXml, customerEnteredPrice,
                rentalStartDate, rentalEndDate);

            if (shoppingCartItem != null && product.ProductType != ProductType.Reservation)
            {
                //update existing shopping cart item
                shoppingCartItem.Quantity = shoppingCartItem.Quantity + quantity;
                warnings.AddRange(await GetShoppingCartItemWarnings(customer, shoppingCartItem, product,
                    automaticallyAddRequiredProductsIfEnabled));

                if (!warnings.Any())
                {
                    shoppingCartItem.AttributesXml = attributesXml;
                    shoppingCartItem.UpdatedOnUtc = DateTime.UtcNow;
                    await _customerService.UpdateShoppingCartItem(customer.Id, shoppingCartItem);

                    //event notification
                    await _mediator.EntityUpdated(shoppingCartItem);
                }
            }
            else
            {
                //new shopping cart item
                DateTime now = DateTime.UtcNow;
                shoppingCartItem = new ShoppingCartItem {
                    ShoppingCartType = shoppingCartType,
                    StoreId = storeId,
                    WarehouseId = warehouseId,
                    ProductId = productId,
                    AttributesXml = attributesXml,
                    CustomerEnteredPrice = customerEnteredPrice,
                    Quantity = quantity,
                    RentalStartDateUtc = rentalStartDate,
                    RentalEndDateUtc = rentalEndDate,
                    AdditionalShippingChargeProduct = product.AdditionalShippingCharge,
                    IsFreeShipping = product.IsFreeShipping,
                    IsRecurring = product.IsRecurring,
                    IsShipEnabled = product.IsShipEnabled,
                    IsTaxExempt = product.IsTaxExempt,
                    IsGiftCard = product.IsGiftCard,
                    ShoppingCartTypeId = (int)shoppingCartType,
                    CreatedOnUtc = now,
                    UpdatedOnUtc = now,
                    ReservationId = reservationId,
                    Parameter = parameter,
                    Duration = duration
                };
                warnings.AddRange(await GetShoppingCartItemWarnings(customer, shoppingCartItem, product, automaticallyAddRequiredProductsIfEnabled));
                if (!warnings.Any())
                {
                    //maximum items validation
                    switch (shoppingCartType)
                    {
                        case ShoppingCartType.ShoppingCart:
                            {
                                if (cart.Count >= _shoppingCartSettings.MaximumShoppingCartItems)
                                {
                                    warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.MaximumShoppingCartItems"), _shoppingCartSettings.MaximumShoppingCartItems));
                                    return warnings;
                                }
                            }
                            break;
                        case ShoppingCartType.Wishlist:
                            {
                                if (cart.Count >= _shoppingCartSettings.MaximumWishlistItems)
                                {
                                    warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.MaximumWishlistItems"), _shoppingCartSettings.MaximumWishlistItems));
                                    return warnings;
                                }
                            }
                            break;
                        default:
                            break;
                    }

                    customer.ShoppingCartItems.Add(shoppingCartItem);
                    await _customerService.InsertShoppingCartItem(customer.Id, shoppingCartItem);

                    //event notification
                    await _mediator.Publish(new AddToCartEvent(customer, shoppingCartItem, product));
                }
            }

            if (!warnings.Any() && groupToBook != null)
            {
                if (product.IncBothDate && product.IntervalUnitType == IntervalUnit.Day)
                {
                    foreach (var item in groupToBook.Where(x => x.Date >= rentalStartDate && x.Date <= rentalEndDate))
                    {
                        await _productReservationService.InsertCustomerReservationsHelper(new CustomerReservationsHelper {
                            CustomerId = customer.Id,
                            ReservationId = item.Id,
                            ShoppingCartItemId = shoppingCartItem.Id
                        });
                    }
                }
                else
                {
                    foreach (var item in groupToBook.Where(x => x.Date >= rentalStartDate && x.Date < rentalEndDate))
                    {
                        await _productReservationService.InsertCustomerReservationsHelper(new CustomerReservationsHelper {
                            CustomerId = customer.Id,
                            ReservationId = item.Id,
                            ShoppingCartItemId = shoppingCartItem.Id
                        });
                    }
                }
            }
            return warnings;
        }

        /// <summary>
        /// Updates the shopping cart item
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="shoppingCartItemId">Shopping cart item identifier</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="customerEnteredPrice">New customer entered price</param>
        /// <param name="rentalStartDate">Rental start date</param>
        /// <param name="rentalEndDate">Rental end date</param>
        /// <param name="quantity">New shopping cart item quantity</param>
        /// <param name="resetCheckoutData">A value indicating whether to reset checkout data</param>
        /// <returns>Warnings</returns>
        public virtual async Task<IList<string>> UpdateShoppingCartItem(Customer customer,
            string shoppingCartItemId, string warehouseId, string attributesXml,
            decimal customerEnteredPrice,
            DateTime? rentalStartDate = null, DateTime? rentalEndDate = null,
            int quantity = 1, bool resetCheckoutData = true, string reservationId = "", string sciId = "")
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var warnings = new List<string>();

            var shoppingCartItem = customer.ShoppingCartItems.FirstOrDefault(sci => sci.Id == shoppingCartItemId);
            if (shoppingCartItem != null)
            {
                if (resetCheckoutData)
                {
                    //reset checkout data
                    await _customerService.ResetCheckoutData(customer, shoppingCartItem.StoreId);
                }
                if (quantity > 0)
                {
                    var product = await _productService.GetProductById(shoppingCartItem.ProductId);
                    shoppingCartItem.Quantity = quantity;
                    shoppingCartItem.WarehouseId = warehouseId;
                    shoppingCartItem.AttributesXml = attributesXml;
                    shoppingCartItem.CustomerEnteredPrice = customerEnteredPrice;
                    shoppingCartItem.RentalStartDateUtc = rentalStartDate;
                    shoppingCartItem.RentalEndDateUtc = rentalEndDate;
                    shoppingCartItem.UpdatedOnUtc = DateTime.UtcNow;
                    shoppingCartItem.AdditionalShippingChargeProduct = product.AdditionalShippingCharge;
                    shoppingCartItem.IsFreeShipping = product.IsFreeShipping;
                    shoppingCartItem.IsRecurring = product.IsRecurring;
                    shoppingCartItem.IsShipEnabled = product.IsShipEnabled;
                    shoppingCartItem.IsTaxExempt = product.IsTaxExempt;
                    shoppingCartItem.IsGiftCard = product.IsGiftCard;
                    //check warnings
                    warnings.AddRange(await GetShoppingCartItemWarnings(customer, shoppingCartItem, product, false));
                    if (!warnings.Any())
                    {
                        //if everything is OK, then update a shopping cart item
                        await _customerService.UpdateShoppingCartItem(customer.Id, shoppingCartItem);

                        //event notification
                        await _mediator.EntityUpdated(shoppingCartItem);
                    }
                }
                else
                {
                    //delete a shopping cart item
                    await DeleteShoppingCartItem(customer, shoppingCartItem, resetCheckoutData, true);
                }
            }

            return warnings;
        }

        /// <summary>
        /// Migrate shopping cart
        /// </summary>
        /// <param name="fromCustomer">From customer</param>
        /// <param name="toCustomer">To customer</param>
        /// <param name="includeCouponCodes">A value indicating whether to coupon codes (discount and gift card) should be also re-applied</param>
        public virtual async Task MigrateShoppingCart(Customer fromCustomer, Customer toCustomer, bool includeCouponCodes)
        {
            if (fromCustomer == null)
                throw new ArgumentNullException("fromCustomer");
            if (toCustomer == null)
                throw new ArgumentNullException("toCustomer");

            if (fromCustomer.Id == toCustomer.Id)
                return; //the same customer

            //shopping cart items
            var fromCart = fromCustomer.ShoppingCartItems.ToList();
            for (int i = 0; i < fromCart.Count; i++)
            {
                var sci = fromCart[i];
                await AddToCart(toCustomer, sci.ProductId, sci.ShoppingCartType, sci.StoreId, sci.WarehouseId,
                    sci.AttributesXml, sci.CustomerEnteredPrice,
                    sci.RentalStartDateUtc, sci.RentalEndDateUtc, sci.Quantity, false, sci.ReservationId, sci.Parameter, sci.Duration);
            }
            for (int i = 0; i < fromCart.Count; i++)
            {
                var sci = fromCart[i];
                await DeleteShoppingCartItem(fromCustomer, sci);
            }

            //copy discount and gift card coupon codes
            if (includeCouponCodes)
            {
                //discount
                var coupons = fromCustomer.ParseAppliedCouponCodes(SystemCustomerAttributeNames.DiscountCoupons);
                var resultcoupons = toCustomer.ApplyCouponCode(SystemCustomerAttributeNames.DiscountCoupons, coupons);
                await _genericAttributeService.SaveAttribute(toCustomer, SystemCustomerAttributeNames.DiscountCoupons, resultcoupons);

                //gift card
                var giftcard = fromCustomer.ParseAppliedCouponCodes(SystemCustomerAttributeNames.GiftCardCoupons);
                var resultgift = toCustomer.ApplyCouponCode(SystemCustomerAttributeNames.GiftCardCoupons, giftcard);
                await _genericAttributeService.SaveAttribute(toCustomer, SystemCustomerAttributeNames.GiftCardCoupons, resultgift);
            }

            //copy url referer
            var lastUrlReferrer = await fromCustomer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.LastUrlReferrer);
            await _genericAttributeService.SaveAttribute(toCustomer, SystemCustomerAttributeNames.LastUrlReferrer, lastUrlReferrer);

            //move selected checkout attributes
            var checkoutAttributesXml = await fromCustomer.GetAttribute<string>(_genericAttributeService, SystemCustomerAttributeNames.CheckoutAttributes, _storeContext.CurrentStore.Id);
            await _genericAttributeService.SaveAttribute(toCustomer, SystemCustomerAttributeNames.CheckoutAttributes, checkoutAttributesXml, _storeContext.CurrentStore.Id);
        }

        #endregion
    }
}
