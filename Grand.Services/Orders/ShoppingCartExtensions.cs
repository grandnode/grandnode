using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Orders;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        /// <returns>Error (if exists); otherwise, empty string</returns>
        public static async Task<(string info, int cycleLength, RecurringProductCyclePeriod cyclePeriod, int totalCycles)> 
            GetRecurringCycleInfo(this IList<ShoppingCartItem> shoppingCart,
            ILocalizationService localizationService, IProductService productService)
        {
            var cycleLength = 0;
            RecurringProductCyclePeriod cyclePeriod = 0;
            var totalCycles = 0;

            int? _cycleLength = null;
            RecurringProductCyclePeriod? _cyclePeriod = null;
            int? _totalCycles = null;

            foreach (var sci in shoppingCart)
            {

                var product = await productService.GetProductById(sci.ProductId);
                if (product == null)
                {
                    throw new GrandException(string.Format("Product (Id={0}) cannot be loaded", sci.ProductId));
                }

                if (product.IsRecurring)
                {
                    string conflictError = localizationService.GetResource("ShoppingCart.ConflictingShipmentSchedules");

                    //cycle length
                    if (_cycleLength.HasValue && _cycleLength.Value != product.RecurringCycleLength)
                        return (conflictError, cycleLength, cyclePeriod, totalCycles);
                    _cycleLength = product.RecurringCycleLength;

                    //cycle period
                    if (_cyclePeriod.HasValue && _cyclePeriod.Value != product.RecurringCyclePeriod)
                        return (conflictError, cycleLength, cyclePeriod, totalCycles);
                    _cyclePeriod = product.RecurringCyclePeriod;

                    //total cycles
                    if (_totalCycles.HasValue && _totalCycles.Value != product.RecurringTotalCycles)
                        return (conflictError, cycleLength, cyclePeriod, totalCycles);
                    _totalCycles = product.RecurringTotalCycles;
                }
            }

            if (_cycleLength.HasValue && _cyclePeriod.HasValue && _totalCycles.HasValue)
            {
                cycleLength = _cycleLength.Value;
                cyclePeriod = _cyclePeriod.Value;
                totalCycles = _totalCycles.Value;
            }

            return ("", cycleLength, cyclePeriod, totalCycles);
        }

        public static IEnumerable<ShoppingCartItem> LimitPerStore(this IEnumerable<ShoppingCartItem> cart, bool cartsSharedBetweenStores, string storeId)
        {
            if (cartsSharedBetweenStores)
                return cart;

            return cart.Where(x => x.StoreId == storeId);
        }
    }
}
