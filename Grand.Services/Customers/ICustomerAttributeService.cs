using Grand.Core.Domain.Customers;
using System.Collections.Generic;

namespace Grand.Services.Customers
{
    /// <summary>
    /// Customer attribute service
    /// </summary>
    public partial interface ICustomerAttributeService
    {
        /// <summary>
        /// Deletes a customer attribute
        /// </summary>
        /// <param name="customerAttribute">Customer attribute</param>
        void DeleteCustomerAttribute(CustomerAttribute customerAttribute);

        /// <summary>
        /// Gets all customer attributes
        /// </summary>
        /// <returns>Customer attributes</returns>
        IList<CustomerAttribute> GetAllCustomerAttributes();

        /// <summary>
        /// Gets a customer attribute 
        /// </summary>
        /// <param name="customerAttributeId">Customer attribute identifier</param>
        /// <returns>Customer attribute</returns>
        CustomerAttribute GetCustomerAttributeById(string customerAttributeId);

        /// <summary>
        /// Inserts a customer attribute
        /// </summary>
        /// <param name="customerAttribute">Customer attribute</param>
        void InsertCustomerAttribute(CustomerAttribute customerAttribute);

        /// <summary>
        /// Updates the customer attribute
        /// </summary>
        /// <param name="customerAttribute">Customer attribute</param>
        void UpdateCustomerAttribute(CustomerAttribute customerAttribute);

        /// <summary>
        /// Deletes a customer attribute value
        /// </summary>
        /// <param name="customerAttributeValue">Customer attribute value</param>
        void DeleteCustomerAttributeValue(CustomerAttributeValue customerAttributeValue);

        /// <summary>
        /// Inserts a customer attribute value
        /// </summary>
        /// <param name="customerAttributeValue">Customer attribute value</param>
        void InsertCustomerAttributeValue(CustomerAttributeValue customerAttributeValue);

        /// <summary>
        /// Updates the customer attribute value
        /// </summary>
        /// <param name="customerAttributeValue">Customer attribute value</param>
        void UpdateCustomerAttributeValue(CustomerAttributeValue customerAttributeValue);
    }
}
