using Grand.Domain.Common;
using Grand.Domain.Orders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Orders
{
    /// <summary>
    /// Checkout attribute parser interface
    /// </summary>
    public partial interface ICheckoutAttributeParser
    {
        /// <summary>
        /// Gets selected checkout attributes
        /// </summary>
        /// <param name="customAttributes">Attributes</param>
        /// <returns>Selected checkout attributes</returns>
        Task<IList<CheckoutAttribute>> ParseCheckoutAttributes(IList<CustomAttribute> customAttributes);

        /// <summary>
        /// Get checkout attribute values
        /// </summary>
        /// <param name="customAttributes">Attributes</param>
        /// <returns>Checkout attribute values</returns>
        Task<IList<CheckoutAttributeValue>> ParseCheckoutAttributeValues(IList<CustomAttribute> customAttributes);

        /// <summary>
        /// Get checkout attribute values with checkout attribute 
        /// </summary>
        /// <param name="attributes">Attributes</param>
        /// <returns>Checkout attribute values with checkout attribute </returns>
        Task<IList<(CheckoutAttribute ca, CheckoutAttributeValue cav)>> ParseCheckoutAttributeValue(IList<CustomAttribute> customAttributes);

        /// <summary>
        /// Adds an attribute
        /// </summary>
        /// <param name="customAttributes">Attributes</param>
        /// <param name="ca">Checkout attribute</param>
        /// <param name="value">Value</param>
        /// <returns>Attributes</returns>
        IList<CustomAttribute> AddCheckoutAttribute(IList<CustomAttribute> customAttributes, CheckoutAttribute ca, string value);

        /// <summary>
        /// Removes checkout attributes which cannot be applied to the current cart and returns an update attributes 
        /// </summary>
        /// <param name="customAttributes">Attributes</param>
        /// <param name="cart">Shopping cart items</param>
        /// <returns>Updated attributes</returns>
        Task<IList<CustomAttribute>> EnsureOnlyActiveAttributes(IList<CustomAttribute> customAttributes, IList<ShoppingCartItem> cart);

        /// <summary>
        /// Check whether condition of some attribute is met (if specified). Return "null" if not condition is specified
        /// </summary>
        /// <param name="attribute">Checkout attribute</param>
        /// <param name="selectedAttributes">Selected attributes</param>
        /// <returns>Result</returns>
        Task<bool?> IsConditionMet(CheckoutAttribute attribute, IList<CustomAttribute> customAttributes);

        /// <summary>
        /// Remove an attribute
        /// </summary>
        /// <param name="attributes">Attributes</param>
        /// <param name="attribute">Checkout attribute</param>
        /// <returns>Updated result</returns>
        IList<CustomAttribute> RemoveCheckoutAttribute(IList<CustomAttribute> customAttributes, CheckoutAttribute attribute);
    }
}
