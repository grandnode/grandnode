using System.Collections.Generic;
using System.Linq;
using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Orders;
using Grand.Services.Localization;
using Grand.Services.Catalog;
using Grand.Services.Customers;
using Grand.Core.Infrastructure;

namespace Grand.Services.Orders
{
    /// <summary>
    /// Represents a shopping cart
    /// </summary>
    public static class ShoppingCartExtensions
    {
        /// <summary>
        /// Indicates whether the shopping cart requires shipping
        /// </summary>
        /// <param name="shoppingCart">Shopping cart</param>
        /// <returns>True if the shopping cart requires shipping; otherwise, false.</returns>
        public static bool RequiresShipping(this IList<ShoppingCartItem> shoppingCart)
        {
            foreach (var shoppingCartItem in shoppingCart)
            {
                if (shoppingCartItem.IsShipEnabled)
                    return true;
            }
            return false;
        }
    
        /// <summary>
        /// Gets a value indicating whether shopping cart is recurring
        /// </summary>
        /// <param name="shoppingCart">Shopping cart</param>
        /// <returns>Result</returns>
        public static bool IsRecurring(this IList<ShoppingCartItem> shoppingCart)
        {
            foreach (ShoppingCartItem sci in shoppingCart)
            {
                if (sci.IsRecurring)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get a recurring cycle information
        /// </summary>
        /// <param name="shoppingCart">Shopping cart</param>
        /// <param name="localizationService">Localization service</param>
        /// <param name="cycleLength">Cycle length</param>
        /// <param name="cyclePeriod">Cycle period</param>
        /// <param name="totalCycles">Total cycles</param>
        /// <returns>Error (if exists); otherwise, empty string</returns>
        public static string GetRecurringCycleInfo(this IList<ShoppingCartItem> shoppingCart,
            ILocalizationService localizationService, IProductService productService,
            out int cycleLength, out RecurringProductCyclePeriod cyclePeriod, out int totalCycles)
        {
            cycleLength = 0;
            cyclePeriod = 0;
            totalCycles = 0;

            int? _cycleLength = null;
            RecurringProductCyclePeriod? _cyclePeriod = null;
            int? _totalCycles = null;

            foreach (var sci in shoppingCart)
            {

                var product = productService.GetProductById(sci.ProductId);
                if (product == null)
                {
                    throw new GrandException(string.Format("Product (Id={0}) cannot be loaded", sci.ProductId));
                }

                if (product.IsRecurring)
                {
                    string conflictError = localizationService.GetResource("ShoppingCart.ConflictingShipmentSchedules");

                    //cycle length
                    if (_cycleLength.HasValue && _cycleLength.Value != product.RecurringCycleLength)
                        return conflictError;
                    _cycleLength = product.RecurringCycleLength;

                    //cycle period
                    if (_cyclePeriod.HasValue && _cyclePeriod.Value != product.RecurringCyclePeriod)
                        return conflictError;
                    _cyclePeriod = product.RecurringCyclePeriod;

                    //total cycles
                    if (_totalCycles.HasValue && _totalCycles.Value != product.RecurringTotalCycles)
                        return conflictError;
                    _totalCycles = product.RecurringTotalCycles;
                }
            }

            if (_cycleLength.HasValue && _cyclePeriod.HasValue && _totalCycles.HasValue)
            {
                cycleLength = _cycleLength.Value;
                cyclePeriod = _cyclePeriod.Value;
                totalCycles = _totalCycles.Value;
            }

            return "";
        }

        public static IEnumerable<ShoppingCartItem> LimitPerStore(this IEnumerable<ShoppingCartItem> cart, string storeId)
        {
            var shoppingCartSettings = EngineContext.Current.Resolve<ShoppingCartSettings>();
            if (shoppingCartSettings.CartsSharedBetweenStores)
                return cart;

            return cart.Where(x => x.StoreId == storeId);
        }
    }
}
