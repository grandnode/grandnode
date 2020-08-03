using Grand.Domain;
using Grand.Domain.Messages;
using System;
using System.Threading.Tasks;

namespace Grand.Services.Messages
{
    /// <summary>
    /// Newsletter subscription service interface
    /// </summary>
    public partial interface INewsLetterSubscriptionService
    {
        /// <summary>
        /// Inserts a newsletter subscription
        /// </summary>
        /// <param name="newsLetterSubscription">NewsLetter subscription</param>
        /// <param name="publishSubscriptionEvents">if set to <c>true</c> [publish subscription events].</param>
        Task InsertNewsLetterSubscription(NewsLetterSubscription newsLetterSubscription, bool publishSubscriptionEvents = true);

        /// <summary>
        /// Updates a newsletter subscription
        /// </summary>
        /// <param name="newsLetterSubscription">NewsLetter subscription</param>
        /// <param name="publishSubscriptionEvents">if set to <c>true</c> [publish subscription events].</param>
        Task UpdateNewsLetterSubscription(NewsLetterSubscription newsLetterSubscription, bool publishSubscriptionEvents = true);

        /// <summary>
        /// Deletes a newsletter subscription
        /// </summary>
        /// <param name="newsLetterSubscription">NewsLetter subscription</param>
        /// <param name="publishSubscriptionEvents">if set to <c>true</c> [publish subscription events].</param>
        Task DeleteNewsLetterSubscription(NewsLetterSubscription newsLetterSubscription, bool publishSubscriptionEvents = true);

        /// <summary>
        /// Gets a newsletter subscription by newsletter subscription identifier
        /// </summary>
        /// <param name="newsLetterSubscriptionId">The newsletter subscription identifier</param>
        /// <returns>NewsLetter subscription</returns>
        Task<NewsLetterSubscription> GetNewsLetterSubscriptionById(string newsLetterSubscriptionId);

        /// <summary>
        /// Gets a newsletter subscription by newsletter subscription GUID
        /// </summary>
        /// <param name="newsLetterSubscriptionGuid">The newsletter subscription GUID</param>
        /// <returns>NewsLetter subscription</returns>
        Task<NewsLetterSubscription> GetNewsLetterSubscriptionByGuid(Guid newsLetterSubscriptionGuid);

        /// <summary>
        /// Gets a newsletter subscription by email and store ID
        /// </summary>
        /// <param name="email">The newsletter subscription email</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>NewsLetter subscription</returns>
        Task<NewsLetterSubscription> GetNewsLetterSubscriptionByEmailAndStoreId(string email, string storeId);


        /// <summary>
        /// Gets a newsletter subscription by customerId
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>NewsLetter subscription</returns>
        Task<NewsLetterSubscription> GetNewsLetterSubscriptionByCustomerId(string customerId);

        /// <summary>
        /// Gets the newsletter subscription list
        /// </summary>
        /// <param name="email">Email to search or string. Empty to load all records.</param>
        /// <param name="storeId">Store identifier. "" to load all records.</param>
        /// <param name="isActive">Value indicating whether subscriber record should be active or not; null to load all records</param>
        /// <param name="customerRoleId">Customer role identifier. Used to filter subscribers by customer role. "" to load all records.</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>NewsLetterSubscription entities</returns>
        Task<IPagedList<NewsLetterSubscription>> GetAllNewsLetterSubscriptions(string email = null,
            string storeId = "", bool? isActive = null, string[] categoryIds = null, int pageIndex = 0, int pageSize = int.MaxValue);
    }
}
