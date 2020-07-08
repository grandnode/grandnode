using Grand.Core;
using Grand.Domain;
using Grand.Domain.Data;
using Grand.Domain.Messages;
using Grand.Services.Common;
using Grand.Services.Events;
using Grand.Services.Events.Extensions;
using MediatR;
using MongoDB.Driver.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Messages
{
    /// <summary>
    /// Newsletter subscription service
    /// </summary>
    public class NewsLetterSubscriptionService : INewsLetterSubscriptionService
    {
        #region Fields

        private readonly IMediator _mediator;
        private readonly IRepository<NewsLetterSubscription> _subscriptionRepository;
        private readonly IHistoryService _historyService;

        #endregion

        #region Ctor

        public NewsLetterSubscriptionService(
            IRepository<NewsLetterSubscription> subscriptionRepository,
            IMediator mediator,
            IHistoryService historyService)
        {
            _subscriptionRepository = subscriptionRepository;
            _mediator = mediator;
            _historyService = historyService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Publishes the subscription event.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="isSubscribe">if set to <c>true</c> [is subscribe].</param>
        /// <param name="publishSubscriptionEvents">if set to <c>true</c> [publish subscription events].</param>
        private async Task PublishSubscriptionEvent(string email, bool isSubscribe, bool publishSubscriptionEvents)
        {
            if (publishSubscriptionEvents)
            {
                if (isSubscribe)
                {
                    await _mediator.PublishNewsletterSubscribe(email);
                }
                else
                {
                    await _mediator.PublishNewsletterUnsubscribe(email);
                }
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Inserts a newsletter subscription
        /// </summary>
        /// <param name="newsLetterSubscription">NewsLetter subscription</param>
        /// <param name="publishSubscriptionEvents">if set to <c>true</c> [publish subscription events].</param>
        public virtual async Task InsertNewsLetterSubscription(NewsLetterSubscription newsLetterSubscription, bool publishSubscriptionEvents = true)
        {
            if (newsLetterSubscription == null)
            {
                throw new ArgumentNullException("newsLetterSubscription");
            }

            //Handle e-mail
            newsLetterSubscription.Email = CommonHelper.EnsureSubscriberEmailOrThrow(newsLetterSubscription.Email);

            //Persist
            await _subscriptionRepository.InsertAsync(newsLetterSubscription);

            //Publish the subscription event 
            if (newsLetterSubscription.Active)
            {
                await PublishSubscriptionEvent(newsLetterSubscription.Email, true, publishSubscriptionEvents);
            }

            //save history
            await newsLetterSubscription.SaveHistory<NewsLetterSubscription>(_historyService);

            //Publish event
            await _mediator.EntityInserted(newsLetterSubscription);
        }

        /// <summary>
        /// Updates a newsletter subscription
        /// </summary>
        /// <param name="newsLetterSubscription">NewsLetter subscription</param>
        /// <param name="publishSubscriptionEvents">if set to <c>true</c> [publish subscription events].</param>
        public virtual async Task UpdateNewsLetterSubscription(NewsLetterSubscription newsLetterSubscription, bool publishSubscriptionEvents = true)
        {
            if (newsLetterSubscription == null)
            {
                throw new ArgumentNullException("newsLetterSubscription");
            }

            //Handle e-mail
            newsLetterSubscription.Email = CommonHelper.EnsureSubscriberEmailOrThrow(newsLetterSubscription.Email);

            //Persist
            await _subscriptionRepository.UpdateAsync(newsLetterSubscription);

            //save history
            await newsLetterSubscription.SaveHistory<NewsLetterSubscription>(_historyService);

            //Publish event
            await _mediator.EntityUpdated(newsLetterSubscription);
        }

        /// <summary>
        /// Deletes a newsletter subscription
        /// </summary>
        /// <param name="newsLetterSubscription">NewsLetter subscription</param>
        /// <param name="publishSubscriptionEvents">if set to <c>true</c> [publish subscription events].</param>
        public virtual async Task DeleteNewsLetterSubscription(NewsLetterSubscription newsLetterSubscription, bool publishSubscriptionEvents = true)
        {
            if (newsLetterSubscription == null) throw new ArgumentNullException("newsLetterSubscription");

            await _subscriptionRepository.DeleteAsync(newsLetterSubscription);

            //Publish the unsubscribe event 
            await PublishSubscriptionEvent(newsLetterSubscription.Email, false, publishSubscriptionEvents);

            //event notification
            await _mediator.EntityDeleted(newsLetterSubscription);
        }

        /// <summary>
        /// Gets a newsletter subscription by newsletter subscription identifier
        /// </summary>
        /// <param name="newsLetterSubscriptionId">The newsletter subscription identifier</param>
        /// <returns>NewsLetter subscription</returns>
        public virtual Task<NewsLetterSubscription> GetNewsLetterSubscriptionById(string newsLetterSubscriptionId)
        {
            return _subscriptionRepository.GetByIdAsync(newsLetterSubscriptionId);
        }

        /// <summary>
        /// Gets a newsletter subscription by newsletter subscription GUID
        /// </summary>
        /// <param name="newsLetterSubscriptionGuid">The newsletter subscription GUID</param>
        /// <returns>NewsLetter subscription</returns>
        public virtual async Task<NewsLetterSubscription> GetNewsLetterSubscriptionByGuid(Guid newsLetterSubscriptionGuid)
        {
            if (newsLetterSubscriptionGuid == Guid.Empty) return null;

            var newsLetterSubscriptions = from nls in _subscriptionRepository.Table
                                          where nls.NewsLetterSubscriptionGuid == newsLetterSubscriptionGuid
                                          orderby nls.Id
                                          select nls;

            return await newsLetterSubscriptions.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets a newsletter subscription by email and store ID
        /// </summary>
        /// <param name="email">The newsletter subscription email</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>NewsLetter subscription</returns>
        public virtual async Task<NewsLetterSubscription> GetNewsLetterSubscriptionByEmailAndStoreId(string email, string storeId)
        {
            if (!CommonHelper.IsValidEmail(email)) 
                return null;

            email = email.Trim();

            var newsLetterSubscriptions = from nls in _subscriptionRepository.Table
                                          where nls.Email.ToLower() == email.ToLower() && nls.StoreId == storeId
                                          orderby nls.Id
                                          select nls;

            return await newsLetterSubscriptions.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets a newsletter subscription by customer ID
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>NewsLetter subscription</returns>
        public virtual async Task<NewsLetterSubscription> GetNewsLetterSubscriptionByCustomerId(string customerId)
        {
            if (String.IsNullOrEmpty(customerId))
                return null;

            var newsLetterSubscriptions = from nls in _subscriptionRepository.Table
                                          where nls.CustomerId == customerId
                                          orderby nls.Id
                                          select nls;

            return await newsLetterSubscriptions.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets the newsletter subscription list
        /// </summary>
        /// <param name="email">Email to search or string. Empty to load all records.</param>
        /// <param name="storeId">Store identifier. "" to load all records.</param>
        /// <param name="customerRoleId">Customer role identifier. Used to filter subscribers by customer role. "" to load all records.</param>
        /// <param name="isActive">Value indicating whether subscriber record should be active or not; null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>NewsLetterSubscription entities</returns>
        public virtual async Task<IPagedList<NewsLetterSubscription>> GetAllNewsLetterSubscriptions(string email = null,
            string storeId = "", bool? isActive = null, string[] categoryIds = null,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            //do not filter by customer role
            var query = _subscriptionRepository.Table;

            if (!String.IsNullOrEmpty(email))
                query = query.Where(nls => nls.Email.ToLower().Contains(email.ToLower()));
            if (!String.IsNullOrEmpty(storeId))
                query = query.Where(nls => nls.StoreId == storeId);
            if (isActive.HasValue)
                query = query.Where(nls => nls.Active == isActive.Value);
            if (categoryIds != null && categoryIds.Length > 0)
                query = query.Where(c => c.Categories.Any(x => categoryIds.Contains(x)));

            query = query.OrderBy(nls => nls.Email);
            return await PagedList<NewsLetterSubscription>.Create(query, pageIndex, pageSize);
        }

        #endregion
    }
}