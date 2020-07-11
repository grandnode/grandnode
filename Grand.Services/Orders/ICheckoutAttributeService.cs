using Grand.Domain.Orders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Orders
{
    /// <summary>
    /// Checkout attribute service
    /// </summary>
    public partial interface ICheckoutAttributeService
    {
        #region Checkout attributes

        /// <summary>
        /// Deletes a checkout attribute
        /// </summary>
        /// <param name="checkoutAttribute">Checkout attribute</param>
        Task DeleteCheckoutAttribute(CheckoutAttribute checkoutAttribute);

        /// <summary>
        /// Gets all checkout attributes
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <param name="excludeShippableAttributes">A value indicating whether we should exlude shippable attributes</param>
        /// <returns>Checkout attributes</returns>
        Task<IList<CheckoutAttribute>> GetAllCheckoutAttributes(string storeId = "", bool excludeShippableAttributes = false, bool ignorAcl = false);

        /// <summary>
        /// Gets a checkout attribute 
        /// </summary>
        /// <param name="checkoutAttributeId">Checkout attribute identifier</param>
        /// <returns>Checkout attribute</returns>
        Task<CheckoutAttribute> GetCheckoutAttributeById(string checkoutAttributeId);

        /// <summary>
        /// Inserts a checkout attribute
        /// </summary>
        /// <param name="checkoutAttribute">Checkout attribute</param>
        Task InsertCheckoutAttribute(CheckoutAttribute checkoutAttribute);

        /// <summary>
        /// Updates the checkout attribute
        /// </summary>
        /// <param name="checkoutAttribute">Checkout attribute</param>
        Task UpdateCheckoutAttribute(CheckoutAttribute checkoutAttribute);

        #endregion

    }
}
