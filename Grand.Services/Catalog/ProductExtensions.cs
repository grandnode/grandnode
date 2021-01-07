using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Services.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Services.Catalog
{
    /// <summary>
    /// Extensions
    /// </summary>
    public static class ProductExtensions
    {
        /// <summary>
        /// Gets an appropriate tier price
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">Customer</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="quantity">Quantity</param>
        /// <returns>Price</returns>
        public static TierPrice GetPreferredTierPrice(this Product product, Customer customer, string storeId, string currencyCode, int quantity)
        {
            if (!product.TierPrices.Any())
                return null;

            //get actual tier prices
            var actualTierPrices = product.TierPrices.OrderBy(price => price.Quantity).ToList()
                .FilterByStore(storeId)
                .FilterByCurrency(currencyCode)
                .FilterForCustomer(customer)
                .FilterByDate()
                .RemoveDuplicatedQuantities();

            //get the most suitable tier price based on the passed quantity
            var tierPrice = actualTierPrices.LastOrDefault(price => quantity >= price.Quantity);

            return tierPrice;
        }

        /// <summary>
        /// Formats the stock availability/quantity message
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="attributes">Selected product attributes (if specified)</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="productAttributeParser">Product attribute parser</param>
        /// <returns>The stock message</returns>
        public static string FormatStockMessage(this Product product, string warehouseId, IList<CustomAttribute> attributes,
            ILocalizationService localizationService, IProductAttributeParser productAttributeParser)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (localizationService == null)
                throw new ArgumentNullException("localizationService");

            if (productAttributeParser == null)
                throw new ArgumentNullException("productAttributeParser");

            string stockMessage = string.Empty;

            switch (product.ManageInventoryMethod)
            {
                case ManageInventoryMethod.ManageStock:
                    {
                        #region Manage stock

                        if (!product.DisplayStockAvailability)
                            return stockMessage;

                        var stockQuantity = product.GetTotalStockQuantity(warehouseId: warehouseId);
                        if (stockQuantity > 0)
                        {
                            stockMessage = product.DisplayStockQuantity ?
                                //display "in stock" with stock quantity
                                string.Format(localizationService.GetResource("Products.Availability.InStockWithQuantity"), stockQuantity) :
                                //display "in stock" without stock quantity
                                localizationService.GetResource("Products.Availability.InStock");
                        }
                        else
                        {
                            //out of stock
                            switch (product.BackorderMode)
                            {
                                case BackorderMode.NoBackorders:
                                    stockMessage = localizationService.GetResource("Products.Availability.OutOfStock");
                                    break;
                                case BackorderMode.AllowQtyBelow0:
                                    stockMessage = localizationService.GetResource("Products.Availability.InStock");
                                    break;
                                case BackorderMode.AllowQtyBelow0AndNotifyCustomer:
                                    stockMessage = localizationService.GetResource("Products.Availability.Backordering");
                                    break;
                                default:
                                    break;
                            }
                        }

                        #endregion
                    }
                    break;
                case ManageInventoryMethod.ManageStockByAttributes:
                    {
                        #region Manage stock by attributes

                        if (!product.DisplayStockAvailability)
                            return stockMessage;

                        var combination = productAttributeParser.FindProductAttributeCombination(product, attributes);
                        if (combination != null)
                        {
                            //combination exists
                            var stockQuantity = product.GetTotalStockQuantityForCombination(combination, warehouseId: warehouseId);
                            if (stockQuantity > 0)
                            {
                                stockMessage = product.DisplayStockQuantity ?
                                    //display "in stock" with stock quantity
                                    string.Format(localizationService.GetResource("Products.Availability.InStockWithQuantity"), stockQuantity) :
                                    //display "in stock" without stock quantity
                                    localizationService.GetResource("Products.Availability.InStock");
                            }
                            else
                            {
                                //out of stock
                                switch (product.BackorderMode)
                                {
                                    case BackorderMode.NoBackorders:
                                        stockMessage = localizationService.GetResource("Products.Availability.Attributes.OutOfStock");
                                        break;
                                    case BackorderMode.AllowQtyBelow0:
                                        stockMessage = localizationService.GetResource("Products.Availability.Attributes.InStock");
                                        break;
                                    case BackorderMode.AllowQtyBelow0AndNotifyCustomer:
                                        stockMessage = localizationService.GetResource("Products.Availability.Attributes.Backordering");
                                        break;
                                    default:
                                        break;
                                }
                                if (!combination.AllowOutOfStockOrders)
                                    stockMessage = localizationService.GetResource("Products.Availability.Attributes.OutOfStock");
                            }
                        }
                        else
                        {
                            //no combination configured
                            stockMessage = localizationService.GetResource("Products.Availability.InStock");
                            if (product.AllowAddingOnlyExistingAttributeCombinations)
                            {
                                stockMessage = localizationService.GetResource("Products.Availability.AllowAddingOnlyExistingAttributeCombinations.Yes");
                            }
                            else
                            {
                                stockMessage = localizationService.GetResource("Products.Availability.AllowAddingOnlyExistingAttributeCombinations.No");
                            }
                        }

                        #endregion
                    }
                    break;
                case ManageInventoryMethod.DontManageStock:
                case ManageInventoryMethod.ManageStockByBundleProducts:
                default:
                    return stockMessage;
            }
            return stockMessage;
        }

        /// <summary>
        /// Indicates whether a product tag exists
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="productTagId">Product tag identifier</param>
        /// <returns>Result</returns>
        public static bool ProductTagExists(this Product product,
            string productTagName)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            bool result = product.ProductTags.FirstOrDefault(pt => pt == productTagName) != null;
            return result;
        }

        /// <summary>
        /// Get a list of allowed quantities (parse 'AllowedQuantities' property)
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>Result</returns>
        public static int[] ParseAllowedQuantities(this Product product)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            var result = new List<int>();
            if (!String.IsNullOrWhiteSpace(product.AllowedQuantities))
            {
                product.AllowedQuantities
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList()
                    .ForEach(qtyStr =>
                    {
                        int qty;
                        if (int.TryParse(qtyStr.Trim(), out qty))
                        {
                            result.Add(qty);
                        }
                    });
            }

            return result.ToArray();
        }

        /// <summary>
        /// Get total quantity
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="useReservedQuantity">
        /// A value indicating whether we should consider "Reserved Quantity" property 
        /// when "multiple warehouses" are used
        /// </param>
        /// <param name="warehouseId">
        /// Warehouse identifier. Used to limit result to certain warehouse.
        /// Used only with "multiple warehouses" enabled.
        /// </param>
        /// <returns>Result</returns>
        public static int GetTotalStockQuantity(this Product product,
            bool useReservedQuantity = true, string warehouseId = "")
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (product.ManageInventoryMethod != ManageInventoryMethod.ManageStock)
            {
                //We can calculate total stock quantity when 'Manage inventory' property is set to 'Track inventory'
                return 0;
            }

            if (product.UseMultipleWarehouses)
            {
                var pwi = product.ProductWarehouseInventory;
                if (!String.IsNullOrEmpty(warehouseId))
                {
                    pwi = pwi.Where(x => x.WarehouseId == warehouseId).ToList();
                }
                var result = pwi.Sum(x => x.StockQuantity);
                if (useReservedQuantity)
                {
                    result = result - pwi.Sum(x => x.ReservedQuantity);
                }
                return result;
            }
            if (string.IsNullOrEmpty(warehouseId) || string.IsNullOrEmpty(product.WarehouseId))
                return product.StockQuantity;
            else
            {
                if (product.WarehouseId == warehouseId)
                    return product.StockQuantity;
                else
                    return 0;
            }
        }


        /// <summary>
        /// Get total quantity
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="combination">Combination</param>
        /// <param name="useReservedQuantity">
        /// A value indicating whether we should consider "Reserved Quantity" property 
        /// when "multiple warehouses" are used
        /// </param>
        /// <param name="warehouseId">
        /// Warehouse identifier. Used to limit result to certain warehouse.
        /// Used only with "multiple warehouses" enabled.
        /// </param>
        /// <returns>Result</returns>
        public static int GetTotalStockQuantityForCombination(this Product product, ProductAttributeCombination combination,
            bool useReservedQuantity = true, string warehouseId = "")
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (combination == null)
                throw new ArgumentNullException("combination");

            if (product.ManageInventoryMethod != ManageInventoryMethod.ManageStockByAttributes)
            {
                return 0;
            }

            if (product.UseMultipleWarehouses)
            {
                var pwi = combination.WarehouseInventory;
                if (!String.IsNullOrEmpty(warehouseId))
                {
                    pwi = pwi.Where(x => x.WarehouseId == warehouseId).ToList();
                }
                var result = pwi.Sum(x => x.StockQuantity);
                if (useReservedQuantity)
                {
                    result = result - pwi.Sum(x => x.ReservedQuantity);
                }
                return result;
            }

            if (string.IsNullOrEmpty(warehouseId) || string.IsNullOrEmpty(product.WarehouseId))
                return combination.StockQuantity;
            else
            {
                if (product.WarehouseId == warehouseId)
                    return combination.StockQuantity;
                else
                    return 0;
            }

        }



        /// <summary>
        /// Gets SKU, Manufacturer part number and GTIN
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="attributes">Attributes</param>
        /// <param name="productAttributeParser">Product attribute service (used when attributes are specified)</param>
        /// <param name="sku">SKU</param>
        /// <param name="manufacturerPartNumber">Manufacturer part number</param>
        /// <param name="gtin">GTIN</param>
        private static void GetSkuMpnGtin(this Product product, IList<CustomAttribute> attributes, IProductAttributeParser productAttributeParser,
            out string sku, out string manufacturerPartNumber, out string gtin)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            sku = null;
            manufacturerPartNumber = null;
            gtin = null;

            if (attributes != null &&
                product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes)
            {
                //manage stock by attribute combinations
                if (productAttributeParser == null)
                    throw new ArgumentNullException("productAttributeParser");

                //let's find appropriate record
                var combination = productAttributeParser.FindProductAttributeCombination(product, attributes);
                if (combination != null)
                {
                    sku = combination.Sku;
                    manufacturerPartNumber = combination.ManufacturerPartNumber;
                    gtin = combination.Gtin;
                }
            }

            if (string.IsNullOrEmpty(sku))
                sku = product.Sku;
            if (string.IsNullOrEmpty(manufacturerPartNumber))
                manufacturerPartNumber = product.ManufacturerPartNumber;
            if (string.IsNullOrEmpty(gtin))
                gtin = product.Gtin;
        }

        /// <summary>
        /// Formats SKU
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="attributes">Attributes</param>
        /// <param name="productAttributeParser">Product attribute service (used when attributes are specified)</param>
        /// <returns>SKU</returns>
        public static string FormatSku(this Product product, IList<CustomAttribute> attributes = null, IProductAttributeParser productAttributeParser = null)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            string sku;

            product.GetSkuMpnGtin(attributes, productAttributeParser,
                out sku, out _, out _);

            return sku;
        }

        /// <summary>
        /// Formats manufacturer part number
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="attributes">Attributes</param>
        /// <param name="productAttributeParser">Product attribute service (used when attributes are specified)</param>
        /// <returns>Manufacturer part number</returns>
        public static string FormatMpn(this Product product, IList<CustomAttribute> attributes = null, IProductAttributeParser productAttributeParser = null)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            string manufacturerPartNumber;

            product.GetSkuMpnGtin(attributes, productAttributeParser,
                out _, out manufacturerPartNumber, out _);

            return manufacturerPartNumber;
        }

        /// <summary>
        /// Formats GTIN
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="attributes">Attributes</param>
        /// <param name="productAttributeParser">Product attribute service (used when attributes are specified)</param>
        /// <returns>GTIN</returns>
        public static string FormatGtin(this Product product, IList<CustomAttribute> attributes = null, IProductAttributeParser productAttributeParser = null)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            string gtin;

            product.GetSkuMpnGtin(attributes, productAttributeParser,
                out _, out _, out gtin);

            return gtin;
        }
    }
}
