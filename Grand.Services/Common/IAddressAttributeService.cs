using Grand.Core.Domain.Common;
using System.Collections.Generic;

namespace Grand.Services.Common
{
    /// <summary>
    /// Address attribute service
    /// </summary>
    public partial interface IAddressAttributeService
    {
        /// <summary>
        /// Deletes an address attribute
        /// </summary>
        /// <param name="addressAttribute">Address attribute</param>
        void DeleteAddressAttribute(AddressAttribute addressAttribute);

        /// <summary>
        /// Gets all address attributes
        /// </summary>
        /// <returns>Address attributes</returns>
        IList<AddressAttribute> GetAllAddressAttributes();

        /// <summary>
        /// Gets an address attribute 
        /// </summary>
        /// <param name="addressAttributeId">Address attribute identifier</param>
        /// <returns>Address attribute</returns>
        AddressAttribute GetAddressAttributeById(string addressAttributeId);

        /// <summary>
        /// Inserts an address attribute
        /// </summary>
        /// <param name="addressAttribute">Address attribute</param>
        void InsertAddressAttribute(AddressAttribute addressAttribute);

        /// <summary>
        /// Updates the address attribute
        /// </summary>
        /// <param name="addressAttribute">Address attribute</param>
        void UpdateAddressAttribute(AddressAttribute addressAttribute);

        /// <summary>
        /// Deletes an address attribute value
        /// </summary>
        /// <param name="addressAttributeValue">Address attribute value</param>
        void DeleteAddressAttributeValue(AddressAttributeValue addressAttributeValue);

        /// <summary>
        /// Inserts a ddress attribute value
        /// </summary>
        /// <param name="addressAttributeValue">Address attribute value</param>
        void InsertAddressAttributeValue(AddressAttributeValue addressAttributeValue);

        /// <summary>
        /// Updates the ddress attribute value
        /// </summary>
        /// <param name="addressAttributeValue">Address attribute value</param>
        void UpdateAddressAttributeValue(AddressAttributeValue addressAttributeValue);
    }
}
