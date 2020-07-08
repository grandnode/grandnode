using Grand.Domain;
using Grand.Domain.Messages;
using System;
using System.Threading.Tasks;

namespace Grand.Services.Messages
{
    /// <summary>
    /// ContactUs interface
    /// </summary>
    public partial interface IContactUsService
    {
        /// <summary>
        /// Deletes a contactus item
        /// </summary>
        /// <param name="contactus">ContactUs item</param>
        Task DeleteContactUs(ContactUs contactus);

        /// <summary>
        /// Clears table
        /// </summary>
        Task ClearTable();

        /// <summary>
        /// Gets all contactUs items
        /// </summary>
        /// <param name="fromUtc">ContactUs item creation from; null to load all records</param>
        /// <param name="toUtc">ContactUs item creation to; null to load all records</param>
        /// <param name="email">email</param>
        /// <param name="vendorId">vendorId; null to load all records</param>
        /// <param name="customerId">customerId; null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>ContactUs items</returns>
        Task<IPagedList<ContactUs>> GetAllContactUs(DateTime? fromUtc = null, DateTime? toUtc = null,
            string email = "", string vendorId = "", string customerId = "", string storeId = "",
            int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets a contactus item
        /// </summary>
        /// <param name="contactUsId">ContactUs item identifier</param>
        /// <returns>ContactUs item</returns>
        Task<ContactUs> GetContactUsById(string contactUsId);

        /// <summary>
        /// Inserts a contactus item
        /// </summary>
        /// <param name="contactus">ContactUs</param>
        /// <returns>A contactus item</returns>
        Task InsertContactUs(ContactUs contactus);

    }
}
