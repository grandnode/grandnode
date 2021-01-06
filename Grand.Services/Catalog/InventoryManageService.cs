using Grand.Core.Caching;
using Grand.Core.Caching.Constants;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Data;
using Grand.Domain.Shipping;
using Grand.Services.Commands.Models.Catalog;
using Grand.Services.Events;
using Grand.Services.Notifications.Catalog;
using MediatR;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Catalog
{
    public class InventoryManageService : IInventoryManageService
    {
        #region Fields

        private readonly IRepository<Product> _productRepository;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ICacheManager _cacheManager;
        private readonly IMediator _mediator;
        private readonly CatalogSettings _catalogSettings;

        #endregion

        #region Ctor

        public InventoryManageService(
            IRepository<Product> productRepository,
            IProductAttributeService productAttributeService,
            IProductAttributeParser productAttributeParser,
            ICacheManager cacheManager,
            IMediator mediator,
            CatalogSettings catalogSettings)
        {
            _productRepository = productRepository;
            _productAttributeService = productAttributeService;
            _productAttributeParser = productAttributeParser;
            _cacheManager = cacheManager;
            _mediator = mediator;
            _catalogSettings = catalogSettings;
        }

        #endregion

        #region Inventory management methods

        /// <summary>
        /// Adjust inventory
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="quantityToChange">Quantity to increase or descrease</param>
        /// <param name="attributes">Attributes</param>
        public virtual async Task AdjustInventory(Product product, int quantityToChange, IList<CustomAttribute> attributes = null, string warehouseId = "")
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (quantityToChange == 0)
                return;

            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
            {
                var prevStockQuantity = product.GetTotalStockQuantity(warehouseId: warehouseId);

                //update stock quantity
                if (product.UseMultipleWarehouses)
                {
                    //use multiple warehouses
                    if (quantityToChange < 0)
                        await ReserveInventory(product, quantityToChange, warehouseId);
                    else
                        await UnblockReservedInventory(product, quantityToChange, warehouseId);

                    product.StockQuantity = product.ProductWarehouseInventory.Sum(x => x.StockQuantity);
                    await UpdateStockProduct(product);
                }
                else
                {
                    //do not use multiple warehouses
                    //simple inventory management
                    product.StockQuantity += quantityToChange;
                    await UpdateStockProduct(product);
                }

                //check if minimum quantity is reached
                if (quantityToChange < 0 && product.MinStockQuantity >= product.GetTotalStockQuantity(warehouseId: ""))
                {
                    switch (product.LowStockActivity)
                    {
                        case LowStockActivity.DisableBuyButton:
                            product.DisableBuyButton = true;
                            product.DisableWishlistButton = true;

                            var filter = Builders<Product>.Filter.Eq("Id", product.Id);
                            var update = Builders<Product>.Update
                                    .Set(x => x.DisableBuyButton, product.DisableBuyButton)
                                    .Set(x => x.DisableWishlistButton, product.DisableWishlistButton)
                                    .Set(x => x.LowStock, true)
                                    .CurrentDate("UpdatedOnUtc");
                            await _productRepository.Collection.UpdateOneAsync(filter, update);
                            //cache
                            await _cacheManager.RemoveAsync(string.Format(CacheKey.PRODUCTS_BY_ID_KEY, product.Id));

                            //event notification
                            await _mediator.EntityUpdated(product);

                            break;
                        case LowStockActivity.Unpublish:
                            product.Published = false;
                            var filter2 = Builders<Product>.Filter.Eq("Id", product.Id);
                            var update2 = Builders<Product>.Update
                                    .Set(x => x.Published, product.Published)
                                    .CurrentDate("UpdatedOnUtc");
                            await _productRepository.Collection.UpdateOneAsync(filter2, update2);

                            //cache
                            await _cacheManager.RemoveAsync(string.Format(CacheKey.PRODUCTS_BY_ID_KEY, product.Id));
                            if (product.ShowOnHomePage)
                                await _cacheManager.RemoveByPrefix(CacheKey.PRODUCTS_SHOWONHOMEPAGE);

                            //event notification
                            await _mediator.EntityUpdated(product);

                            break;
                        default:
                            break;
                    }
                }
                //qty is increased. product is back in stock (minimum stock quantity is reached again)?
                if (_catalogSettings.PublishBackProductWhenCancellingOrders)
                {
                    if (quantityToChange > 0 && prevStockQuantity <= product.MinStockQuantity && product.MinStockQuantity < product.GetTotalStockQuantity(warehouseId: ""))
                    {
                        switch (product.LowStockActivity)
                        {
                            case LowStockActivity.DisableBuyButton:
                                var filter = Builders<Product>.Filter.Eq("Id", product.Id);
                                var update = Builders<Product>.Update
                                        .Set(x => x.DisableBuyButton, product.DisableBuyButton)
                                        .Set(x => x.DisableWishlistButton, product.DisableWishlistButton)
                                        .Set(x => x.LowStock, true)
                                        .CurrentDate("UpdatedOnUtc");
                                await _productRepository.Collection.UpdateOneAsync(filter, update);
                                //cache
                                await _cacheManager.RemoveAsync(string.Format(CacheKey.PRODUCTS_BY_ID_KEY, product.Id));
                                break;
                            case LowStockActivity.Unpublish:
                                product.Published = false;
                                var filter2 = Builders<Product>.Filter.Eq("Id", product.Id);
                                var update2 = Builders<Product>.Update
                                        .Set(x => x.Published, product.Published)
                                        .CurrentDate("UpdatedOnUtc");
                                await _productRepository.Collection.UpdateOneAsync(filter2, update2);

                                //cache
                                await _cacheManager.RemoveAsync(string.Format(CacheKey.PRODUCTS_BY_ID_KEY, product.Id));
                                if (product.ShowOnHomePage)
                                    await _cacheManager.RemoveByPrefix(CacheKey.PRODUCTS_SHOWONHOMEPAGE);

                                break;
                            default:
                                break;
                        }
                    }
                }

                //send email notification
                if (quantityToChange < 0 && product.GetTotalStockQuantity(warehouseId: warehouseId) < product.NotifyAdminForQuantityBelow)
                {
                    await _mediator.Send(new SendQuantityBelowStoreOwnerNotificationCommand() {
                        Product = product
                    });
                }
            }

            if (attributes != null && product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
            {
                var combination = _productAttributeParser.FindProductAttributeCombination(product, attributes);
                if (combination != null)
                {
                    combination.ProductId = product.Id;
                    if (!product.UseMultipleWarehouses)
                    {
                        combination.StockQuantity += quantityToChange;
                        await _productAttributeService.UpdateProductAttributeCombination(combination);
                    }
                    else
                    {
                        if (quantityToChange < 0)
                            await ReserveInventoryCombination(product, combination, quantityToChange, warehouseId);
                        else
                            await UnblockReservedInventoryCombination(product, combination, quantityToChange, warehouseId);
                    }

                    product.StockQuantity += quantityToChange;
                    await UpdateStockProduct(product);

                    //send email notification
                    if (quantityToChange < 0 && combination.StockQuantity < combination.NotifyAdminForQuantityBelow)
                    {
                        await _mediator.Send(new SendQuantityBelowStoreOwnerNotificationCommand() {
                            Product = product,
                            ProductAttributeCombination = combination
                        });
                    }
                }
            }

            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByBundleProducts)
            {
                foreach (var item in product.BundleProducts)
                {
                    var p1 = await _productRepository.GetByIdAsync(item.ProductId);
                    if (p1 != null && (p1.ManageInventoryMethod == ManageInventoryMethod.ManageStock || p1.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes))
                    {
                        await AdjustInventory(p1, quantityToChange * item.Quantity, attributes, warehouseId);
                    }
                }
            }

            //bundled products
            var attributeValues = _productAttributeParser.ParseProductAttributeValues(product, attributes);
            foreach (var attributeValue in attributeValues)
            {
                if (attributeValue.AttributeValueType == AttributeValueType.AssociatedToProduct)
                {
                    //associated product (bundle)
                    var associatedProduct = await _productRepository.GetByIdAsync(attributeValue.AssociatedProductId);
                    if (associatedProduct != null)
                    {
                        await AdjustInventory(associatedProduct, quantityToChange * attributeValue.Quantity, null, warehouseId);
                    }
                }
            }

            //event notification
            await _mediator.EntityUpdated(product);
        }

        /// <summary>
        /// Reserve the given quantity in the warehouses.
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="quantity">Quantity, must be negative</param>
        public virtual async Task ReserveInventory(Product product, int quantity, string warehouseId)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (quantity >= 0)
                throw new ArgumentException("Value must be negative.", "quantity");

            var qty = -quantity;

            var productInventory = product.ProductWarehouseInventory
                .OrderByDescending(pwi => pwi.StockQuantity - pwi.ReservedQuantity)
                .ToList();

            if (productInventory.Count <= 0)
                return;

            Action pass = () =>
            {
                foreach (var item in productInventory.Where(x => x.WarehouseId == warehouseId || string.IsNullOrEmpty(warehouseId)))
                {
                    var selectQty = Math.Min(item.StockQuantity - item.ReservedQuantity, qty);
                    if (qty - selectQty < 0)
                        break;

                    item.ReservedQuantity += selectQty;
                    qty -= selectQty;
                    if (qty <= 0)
                        break;
                }
            };

            // 1st pass: Applying reserved
            pass();

            if (qty > 0)
            {
                // 2rd pass: Booking negative stock!
                var pwi = productInventory[0];
                pwi.ReservedQuantity += qty;
            }

            var filter = Builders<Product>.Filter.Eq("Id", product.Id);
            var update = Builders<Product>.Update
                    .Set(x => x.ProductWarehouseInventory, productInventory)
                    .CurrentDate("UpdatedOnUtc");
            await _productRepository.Collection.UpdateOneAsync(filter, update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(CacheKey.PRODUCTS_BY_ID_KEY, product.Id));

            //event notification
            await _mediator.EntityUpdated(product);
        }


        /// <summary>
        /// Reserve the given quantity in the warehouses.
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="combination">Combination</param>
        /// <param name="quantity">Quantity, must be negative</param>
        public virtual async Task ReserveInventoryCombination(Product product, ProductAttributeCombination combination, int quantity, string warehouseId)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (combination == null)
                throw new ArgumentNullException("combination");

            if (quantity >= 0)
                throw new ArgumentException("Value must be negative.", "quantity");

            var qty = -quantity;

            var productInventory = combination.WarehouseInventory
                .OrderByDescending(pwi => pwi.StockQuantity - pwi.ReservedQuantity)
                .ToList();

            if (productInventory.Count <= 0)
                return;

            Action pass = () =>
            {
                foreach (var item in productInventory.Where(x => x.WarehouseId == warehouseId || string.IsNullOrEmpty(warehouseId)))
                {
                    var selectQty = Math.Min(item.StockQuantity - item.ReservedQuantity, qty);
                    if (qty - selectQty < 0)
                        break;

                    item.ReservedQuantity += selectQty;
                    qty -= selectQty;
                    if (qty <= 0)
                        break;
                }
            };

            // 1st pass: Applying reserved
            pass();

            if (qty > 0)
            {
                // 2rd pass: Booking negative stock!
                var pwi = productInventory[0];
                pwi.ReservedQuantity += qty;
            }
            combination.StockQuantity = combination.WarehouseInventory.Sum(x => x.StockQuantity - x.ReservedQuantity);
            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.Id, combination.ProductId);
            filter = filter & builder.ElemMatch(x => x.ProductAttributeCombinations, y => y.Id == combination.Id);
            var update = Builders<Product>.Update
                .Set("ProductAttributeCombinations.$.StockQuantity", combination.StockQuantity)
                .Set("ProductAttributeCombinations.$.WarehouseInventory", combination.WarehouseInventory)
                .CurrentDate("UpdatedOnUtc");

            await _productRepository.Collection.UpdateManyAsync(filter, update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(CacheKey.PRODUCTS_BY_ID_KEY, product.Id));

            //event notification
            await _mediator.EntityUpdated(product);
        }

        /// <summary>
        /// Unblocks the given quantity reserved items in the warehouses
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="quantity">Quantity, must be positive</param>
        public virtual async Task UnblockReservedInventory(Product product, int quantity, string warehouseId)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (quantity < 0)
                throw new ArgumentException("Value must be positive.", "quantity");

            var productInventory = product.ProductWarehouseInventory
                .OrderByDescending(pwi => pwi.ReservedQuantity)
                .ThenByDescending(pwi => pwi.StockQuantity)
                .ToList();

            if (productInventory.Count <= 0)
                return;

            var qty = quantity;

            foreach (var item in productInventory.Where(x => x.WarehouseId == warehouseId || string.IsNullOrEmpty(warehouseId)))
            {
                var selectQty = Math.Min(item.ReservedQuantity, qty);
                if (qty - selectQty < 0)
                    break;

                item.ReservedQuantity -= selectQty;
                qty -= selectQty;
                if (qty <= 0)
                    break;
            }

            if (qty > 0)
            {
                var pwi = productInventory[0];
                pwi.StockQuantity += qty;

            }

            var filter = Builders<Product>.Filter.Eq("Id", product.Id);
            var update = Builders<Product>.Update
                    .Set(x => x.ProductWarehouseInventory, productInventory)
                    .CurrentDate("UpdatedOnUtc");
            await _productRepository.Collection.UpdateOneAsync(filter, update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(CacheKey.PRODUCTS_BY_ID_KEY, product.Id));

            //event notification
            await _mediator.EntityUpdated(product);
        }


        /// <summary>
        /// Unblocks the given quantity reserved items in the warehouses
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="quantity">Quantity, must be positive</param>
        public virtual async Task UnblockReservedInventoryCombination(Product product, ProductAttributeCombination combination, int quantity, string warehouseId)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (quantity < 0)
                throw new ArgumentException("Value must be positive.", "quantity");

            var productInventory = combination.WarehouseInventory
                .OrderByDescending(pwi => pwi.StockQuantity - pwi.ReservedQuantity)
                .ToList();

            if (productInventory.Count <= 0)
                return;

            var qty = quantity;

            foreach (var item in productInventory.Where(x => x.WarehouseId == warehouseId || string.IsNullOrEmpty(warehouseId)))
            {
                var selectQty = Math.Min(item.ReservedQuantity, qty);
                if (qty - selectQty < 0)
                    break;

                item.ReservedQuantity -= selectQty;
                qty -= selectQty;
                if (qty <= 0)
                    break;
            }

            if (qty > 0)
            {
                var pwi = productInventory[0];
                pwi.StockQuantity += qty;
            }

            combination.StockQuantity = combination.WarehouseInventory.Sum(x => x.StockQuantity);
            var builder = Builders<Product>.Filter;
            var filter = builder.Eq(x => x.Id, combination.ProductId);
            filter = filter & builder.ElemMatch(x => x.ProductAttributeCombinations, y => y.Id == combination.Id);
            var update = Builders<Product>.Update
                .Set("ProductAttributeCombinations.$.StockQuantity", combination.StockQuantity)
                .Set("ProductAttributeCombinations.$.WarehouseInventory", combination.WarehouseInventory)
                .CurrentDate("UpdatedOnUtc");

            await _productRepository.Collection.UpdateManyAsync(filter, update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(CacheKey.PRODUCTS_BY_ID_KEY, product.Id));

            //event notification
            await _mediator.EntityUpdated(product);
        }

        /// <summary>
        /// Book the reserved quantity
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customAttribute">Attribute</param>
        /// <param name="warehouseId">Warehouse identifier</param>
        /// <param name="quantity">Quantity, must be negative</param>
        public virtual async Task BookReservedInventory(Product product, IList<CustomAttribute> customAttribute, string warehouseId, int quantity)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (quantity >= 0)
                throw new ArgumentException("Value must be negative.", "quantity");

            //only products with "use multiple warehouses" are handled this way
            if (product.ManageInventoryMethod == ManageInventoryMethod.DontManageStock)
                return;
            if (!product.UseMultipleWarehouses && product.ManageInventoryMethod != ManageInventoryMethod.ManageStockByBundleProducts)
                return;

            //standard manage stock 
            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
            {
                var pwi = product.ProductWarehouseInventory.FirstOrDefault(pi => pi.WarehouseId == warehouseId);
                if (pwi == null)
                    return;

                pwi.ReservedQuantity = Math.Max(pwi.ReservedQuantity + quantity, 0);
                pwi.StockQuantity += quantity;

                var builder = Builders<Product>.Filter;
                var filter = builder.Eq(x => x.Id, product.Id);
                filter &= builder.Where(x => x.ProductWarehouseInventory.Any(y => y.WarehouseId == pwi.WarehouseId));

                var update = Builders<Product>.Update
                        .Set(x => x.ProductWarehouseInventory.ElementAt(-1), pwi)
                        .CurrentDate("UpdatedOnUtc");
                await _productRepository.Collection.UpdateOneAsync(filter, update);

                product.StockQuantity = product.ProductWarehouseInventory.Sum(x => x.StockQuantity);
                await UpdateStockProduct(product);

            }
            //manage stock by attributes
            if (customAttribute != null && product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
            {
                var combination = _productAttributeParser.FindProductAttributeCombination(product, customAttribute);
                if (combination == null)
                    return;
                combination.ProductId = product.Id;

                var pwi = combination.WarehouseInventory.FirstOrDefault(pi => pi.WarehouseId == warehouseId);
                if (pwi == null)
                    return;

                pwi.ReservedQuantity = Math.Max(pwi.ReservedQuantity + quantity, 0);
                pwi.StockQuantity += quantity;

                combination.StockQuantity = combination.WarehouseInventory.Sum(x => x.StockQuantity);

                var builder = Builders<Product>.Filter;
                var filter = builder.Eq(x => x.Id, combination.ProductId);
                filter = filter & builder.ElemMatch(x => x.ProductAttributeCombinations, y => y.Id == combination.Id);
                var update = Builders<Product>.Update
                    .Set("ProductAttributeCombinations.$.StockQuantity", combination.StockQuantity)
                    .Set("ProductAttributeCombinations.$.WarehouseInventory", combination.WarehouseInventory)
                    .CurrentDate("UpdatedOnUtc");

                await _productRepository.Collection.UpdateManyAsync(filter, update);

                product.StockQuantity = product.ProductAttributeCombinations.Sum(x => x.StockQuantity);
                await UpdateStockProduct(product);

            }
            //manage stock by bundle products
            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByBundleProducts)
            {
                foreach (var item in product.BundleProducts)
                {
                    var p1 = await _productRepository.GetByIdAsync(item.ProductId);
                    if (p1 != null && p1.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                    {
                        await BookReservedInventory(p1, null, warehouseId, quantity * item.Quantity);
                    }
                }
            }

            //cache
            await _cacheManager.RemoveAsync(string.Format(CacheKey.PRODUCTS_BY_ID_KEY, product.Id));

            //event notification
            await _mediator.EntityUpdated(product);

        }

        /// <summary>
        /// Reverse booked inventory (if acceptable)
        /// </summary>
        /// <param name="product">product</param>
        /// <param name="shipmentItem">Shipment item</param>
        /// <returns>Quantity reversed</returns>
        public virtual async Task<int> ReverseBookedInventory(Product product, Shipment shipment, ShipmentItem shipmentItem)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (shipmentItem == null)
                throw new ArgumentNullException("shipmentItem");

            //only products with "use multiple warehouses" are handled this way
            if (product.ManageInventoryMethod == ManageInventoryMethod.DontManageStock)
                return 0;
            if (!product.UseMultipleWarehouses && product.ManageInventoryMethod != ManageInventoryMethod.ManageStockByBundleProducts)
                return 0;

            var qty = shipmentItem.Quantity;

            //standard manage stock
            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
            {
                var pwi = product.ProductWarehouseInventory.FirstOrDefault(x => x.WarehouseId == shipmentItem.WarehouseId);
                if (pwi == null)
                    return 0;

                //not shipped yet? hence "BookReservedInventory" method was not invoked
                if (!shipment.ShippedDateUtc.HasValue)
                    return 0;

                pwi.StockQuantity += qty;
                pwi.ReservedQuantity += qty;

                var builder = Builders<Product>.Filter;
                var filter = builder.Eq(x => x.Id, product.Id);
                filter = filter & builder.Where(x => x.ProductWarehouseInventory.Any(y => y.WarehouseId == pwi.WarehouseId));

                var update = Builders<Product>.Update
                        .Set(x => x.ProductWarehouseInventory.ElementAt(-1), pwi)
                        .CurrentDate("UpdatedOnUtc");
                await _productRepository.Collection.UpdateOneAsync(filter, update);
            }

            //manage stock by attributes
            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
            {
                var combination = _productAttributeParser.FindProductAttributeCombination(product, shipmentItem.Attributes);
                if (combination == null)
                    return 0;

                combination.ProductId = product.Id;

                var pwi = combination.WarehouseInventory.FirstOrDefault(x => x.WarehouseId == shipmentItem.WarehouseId);
                if (pwi == null)
                    return 0;

                //not shipped yet? hence "BookReservedInventory" method was not invoked
                if (!shipment.ShippedDateUtc.HasValue)
                    return 0;

                pwi.StockQuantity += qty;
                pwi.ReservedQuantity += qty;

                combination.StockQuantity = combination.WarehouseInventory.Sum(x => x.StockQuantity);

                var builder = Builders<Product>.Filter;
                var filter = builder.Eq(x => x.Id, combination.ProductId);
                filter = filter & builder.ElemMatch(x => x.ProductAttributeCombinations, y => y.Id == combination.Id);
                var update = Builders<Product>.Update
                    .Set("ProductAttributeCombinations.$.StockQuantity", combination.StockQuantity)
                    .Set("ProductAttributeCombinations.$.WarehouseInventory", combination.WarehouseInventory)
                    .CurrentDate("UpdatedOnUtc");

                await _productRepository.Collection.UpdateManyAsync(filter, update);
            }
            //manage stock by bundle products
            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByBundleProducts)
            {
                foreach (var item in product.BundleProducts)
                {
                    var p1 = await _productRepository.GetByIdAsync(item.ProductId);
                    if (p1 != null && p1.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                    {
                        shipmentItem.Quantity *= item.Quantity;
                        await ReverseBookedInventory(p1, shipment, shipmentItem);
                    }
                }
            }
            //cache
            await _cacheManager.RemoveAsync(string.Format(CacheKey.PRODUCTS_BY_ID_KEY, product.Id));

            //event notification
            await _mediator.EntityUpdated(product);

            return qty;
        }

        public virtual async Task UpdateStockProduct(Product product, bool mediator = true)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            //update
            var filter = Builders<Product>.Filter.Eq("Id", product.Id);
            var update = Builders<Product>.Update
                    .Set(x => x.StockQuantity, product.StockQuantity)
                    .Set(x => x.LowStock, ((product.MinStockQuantity > 0 && product.MinStockQuantity >= product.StockQuantity) || product.StockQuantity < 0))
                    .CurrentDate("UpdatedOnUtc");
            await _productRepository.Collection.UpdateOneAsync(filter, update);

            //cache
            await _cacheManager.RemoveAsync(string.Format(CacheKey.PRODUCTS_BY_ID_KEY, product.Id));

            //event notification
            if (mediator)
                await _mediator.Publish(new UpdateStockEvent(product));

            //event notification
            await _mediator.EntityUpdated(product);
        }
        #endregion
    }
}
