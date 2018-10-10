using Grand.Core.Domain.Messages;
using System.Collections.Generic;

namespace Grand.Services.Messages
{
    /// <summary>
    /// Contact attribute service
    /// </summary>
    public partial interface IContactAttributeService
    {
        #region Contact attributes

        /// <summary>
        /// Deletes a contact attribute
        /// </summary>
        /// <param name="contactAttribute">Contact attribute</param>
        void DeleteContactAttribute(ContactAttribute contactAttribute);

        /// <summary>
        /// Gets all Contact attributes
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Contact attributes</returns>
        IList<ContactAttribute> GetAllContactAttributes(string storeId = "", bool ignorAcl = false);

        /// <summary>
        /// Gets a Contact attribute 
        /// </summary>
        /// <param name="contactAttributeId">Contact attribute identifier</param>
        /// <returns>Contact attribute</returns>
        ContactAttribute GetContactAttributeById(string contactAttributeId);

        /// <summary>
        /// Inserts a Contact attribute
        /// </summary>
        /// <param name="contactAttribute">Contact attribute</param>
        void InsertContactAttribute(ContactAttribute contactAttribute);

        /// <summary>
        /// Updates the Contact attribute
        /// </summary>
        /// <param name="contactAttribute">Contact attribute</param>
        void UpdateContactAttribute(ContactAttribute contactAttribute);

        #endregion

        
    }
}
