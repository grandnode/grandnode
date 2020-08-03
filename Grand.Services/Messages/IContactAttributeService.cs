using Grand.Domain.Messages;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        Task DeleteContactAttribute(ContactAttribute contactAttribute);

        /// <summary>
        /// Gets all Contact attributes
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Contact attributes</returns>
        Task<IList<ContactAttribute>> GetAllContactAttributes(string storeId = "", bool ignorAcl = false);

        /// <summary>
        /// Gets a Contact attribute 
        /// </summary>
        /// <param name="contactAttributeId">Contact attribute identifier</param>
        /// <returns>Contact attribute</returns>
        Task<ContactAttribute> GetContactAttributeById(string contactAttributeId);

        /// <summary>
        /// Inserts a Contact attribute
        /// </summary>
        /// <param name="contactAttribute">Contact attribute</param>
        Task InsertContactAttribute(ContactAttribute contactAttribute);

        /// <summary>
        /// Updates the Contact attribute
        /// </summary>
        /// <param name="contactAttribute">Contact attribute</param>
        Task UpdateContactAttribute(ContactAttribute contactAttribute);

        #endregion
    }
}
