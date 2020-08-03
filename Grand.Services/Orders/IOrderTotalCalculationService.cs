using Grand.Domain.Customers;
using Grand.Domain.Orders;
using Grand.Services.Discounts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Orders
{
    /// <summary>
    /// Order service interface
    /// </summary>
    public partial interface IOrderTotalCalculationService
    {
        /// <summary>
        /// Gets shopping cart subtotal
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="discountAmount">Applied discount amount</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <param name="subTotalWithoutDiscount">Sub total (without discount)</param>
        /// <param name="subTotalWithDiscount">Sub total (with discount)</param>
        /// <param name="taxRates">Tax rates (of order sub total)</param>
        Task<(decimal discountAmount, List<AppliedDiscount> appliedDiscounts, decimal subTotalWithoutDiscount, decimal subTotalWithDiscount, SortedDictionary<decimal, decimal> taxRates)> 
            GetShoppingCartSubTotal(IList<ShoppingCartItem> cart, 
            bool includingTax);

        /// <summary>
        /// Adjust shipping rate (free shipping, additional charges, discounts)
        /// </summary>
        /// <param name="shippingRate">Shipping rate to adjust</param>
        /// <param name="cart">Cart</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Adjusted shipping rate</returns>
        Task<(decimal shippingRate, List<AppliedDiscount> appliedDiscounts)> AdjustShippingRate(decimal shippingRate, IList<ShoppingCartItem> cart);

        /// <summary>
        /// Gets shopping cart additional shipping charge
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <returns>Additional shipping charge</returns>
        Task<decimal> GetShoppingCartAdditionalShippingCharge(IList<ShoppingCartItem> cart);

        /// <summary>
        /// Gets a value indicating whether shipping is free
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <returns>A value indicating whether shipping is free</returns>
        Task<bool> IsFreeShipping(IList<ShoppingCartItem> cart);

        /// <summary>
        /// Gets shopping cart shipping total
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <returns>Shipping total</returns>
        Task<(decimal? shoppingCartShippingTotal, decimal taxRate, List<AppliedDiscount> appliedDiscounts)> GetShoppingCartShippingTotal(IList<ShoppingCartItem> cart);

        /// <summary>
        /// Gets shopping cart shipping total
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="taxRate">Applied tax rate</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Shipping total</returns>
        Task<(decimal? shoppingCartShippingTotal, decimal taxRate, List<AppliedDiscount> appliedDiscounts)> GetShoppingCartShippingTotal(IList<ShoppingCartItem> cart, bool includingTax);

        
        /// <summary>
        /// Gets tax
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="taxRates">Tax rates</param>
        /// <param name="usePaymentMethodAdditionalFee">A value indicating whether we should use payment method additional fee when calculating tax</param>
        /// <returns>Tax total</returns>
        Task<(decimal taxtotal, SortedDictionary<decimal, decimal> taxRates)> GetTaxTotal(IList<ShoppingCartItem> cart, bool usePaymentMethodAdditionalFee = true);

        /// <summary>
        /// Gets shopping cart total
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="appliedGiftCards">Applied gift cards</param>
        /// <param name="discountAmount">Applied discount amount</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <param name="redeemedRewardPoints">Reward points to redeem</param>
        /// <param name="redeemedRewardPointsAmount">Reward points amount in primary store currency to redeem</param>
        /// <param name="useRewardPoints">A value indicating reward points should be used; null to detect current choice of the customer</param>
        /// <param name="usePaymentMethodAdditionalFee">A value indicating whether we should use payment method additional fee when calculating order total</param>
        /// <returns>Shopping cart total;Null if shopping cart total couldn't be calculated now</returns>
        Task<(decimal? shoppingCartTotal, decimal discountAmount, List<AppliedDiscount> appliedDiscounts, List<AppliedGiftCard> appliedGiftCards,
            int redeemedRewardPoints, decimal redeemedRewardPointsAmount)> 
            GetShoppingCartTotal(IList<ShoppingCartItem> cart, bool? useRewardPoints = null, bool usePaymentMethodAdditionalFee = true);

        /// <summary>
        /// Converts existing reward points to amount
        /// </summary>
        /// <param name="rewardPoints">Reward points</param>
        /// <returns>Converted value</returns>
        Task<decimal> ConvertRewardPointsToAmount(int rewardPoints);

        /// <summary>
        /// Converts an amount to reward points
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <returns>Converted value</returns>
        int ConvertAmountToRewardPoints(decimal amount);

        /// <summary>
        /// Gets a value indicating whether a customer has minimum amount of reward points to use (if enabled)
        /// </summary>
        /// <param name="rewardPoints">Reward points to check</param>
        /// <returns>true - reward points could use; false - cannot be used.</returns>
        bool CheckMinimumRewardPointsToUseRequirement(int rewardPoints);

    }
}
