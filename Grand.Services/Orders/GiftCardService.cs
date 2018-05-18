using System;
using System.Collections.Generic;
using System.Linq;
using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Orders;
using Grand.Services.Customers;
using Grand.Services.Events;
using MongoDB.Driver.Linq;

namespace Grand.Services.Orders
{
    /// <summary>
    /// Gift card service
    /// </summary>
    public partial class GiftCardService : IGiftCardService
    {
        #region Fields
        
        private readonly IRepository<GiftCard> _giftCardRepository;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="giftCardRepository">Gift card context</param>
        /// <param name="eventPublisher">Event published</param>
        public GiftCardService(IRepository<GiftCard> giftCardRepository, IEventPublisher eventPublisher)
        {
            _giftCardRepository = giftCardRepository;
            _eventPublisher = eventPublisher;
        }

        #endregion
        
        #region Methods

        /// <summary>
        /// Deletes a gift card
        /// </summary>
        /// <param name="giftCard">Gift card</param>
        public virtual void DeleteGiftCard(GiftCard giftCard)
        {
            if (giftCard == null)
                throw new ArgumentNullException("giftCard");

            _giftCardRepository.Delete(giftCard);

            //event notification
            _eventPublisher.EntityDeleted(giftCard);
        }

        /// <summary>
        /// Gets a gift card
        /// </summary>
        /// <param name="giftCardId">Gift card identifier</param>
        /// <returns>Gift card entry</returns>
        public virtual GiftCard GetGiftCardById(string giftCardId)
        {
            return _giftCardRepository.GetById(giftCardId);
        }

        /// <summary>
        /// Gets all gift cards
        /// </summary>
        /// <param name="purchasedWithOrderId">Associated order ID; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="isGiftCardActivated">Value indicating whether gift card is activated; null to load all records</param>
        /// <param name="giftCardCouponCode">Gift card coupon code; nullto load all records</param>
        /// <param name="recipientName">Recipient name; null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Gift cards</returns>
        public virtual IPagedList<GiftCard> GetAllGiftCards(string purchasedWithOrderId = "",
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null, 
            bool? isGiftCardActivated = null, string giftCardCouponCode = null,
            string recipientName = null,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _giftCardRepository.Table;

            //TO DO
            //if (!String.IsNullOrEmpty(purchasedWithOrderId))
            //    query = query.Where(gc => gc.PurchasedWithOrderItem != null && gc.PurchasedWithOrderItem.OrderId == purchasedWithOrderId);
            if (createdFromUtc.HasValue)
                query = query.Where(gc => createdFromUtc.Value <= gc.CreatedOnUtc);
            if (createdToUtc.HasValue)
                query = query.Where(gc => createdToUtc.Value >= gc.CreatedOnUtc);
            if (isGiftCardActivated.HasValue)
                query = query.Where(gc => gc.IsGiftCardActivated == isGiftCardActivated.Value);
            if (!String.IsNullOrEmpty(giftCardCouponCode))
                query = query.Where(gc => gc.GiftCardCouponCode == giftCardCouponCode);
            if (!String.IsNullOrWhiteSpace(recipientName))
                query = query.Where(c => c.RecipientName.Contains(recipientName));
            query = query.OrderByDescending(gc => gc.CreatedOnUtc);

            var giftCards = new PagedList<GiftCard>(query, pageIndex, pageSize);
            return giftCards;
        }

        public IList<GiftCardUsageHistory> GetAllGiftCardUsageHistory(string orderId = "")
        {
            var query = from g in _giftCardRepository.Table
                        from h in g.GiftCardUsageHistory
                        select h;

            query = query.Where(x => x.UsedWithOrderId == orderId);
            return query.ToList();
        }

        /// <summary>
        /// Inserts a gift card
        /// </summary>
        /// <param name="giftCard">Gift card</param>
        public virtual void InsertGiftCard(GiftCard giftCard)
        {
            if (giftCard == null)
                throw new ArgumentNullException("giftCard");
            giftCard.GiftCardCouponCode = giftCard.GiftCardCouponCode.ToLowerInvariant();
            _giftCardRepository.Insert(giftCard);

            //event notification
            _eventPublisher.EntityInserted(giftCard);
        }

        /// <summary>
        /// Updates the gift card
        /// </summary>
        /// <param name="giftCard">Gift card</param>
        public virtual void UpdateGiftCard(GiftCard giftCard)
        {
            if (giftCard == null)
                throw new ArgumentNullException("giftCard");

            giftCard.GiftCardCouponCode = giftCard.GiftCardCouponCode.ToLowerInvariant();
            _giftCardRepository.Update(giftCard);

            //event notification
            _eventPublisher.EntityUpdated(giftCard);
        }

        /// <summary>
        /// Gets gift cards by 'PurchasedWithOrderItemId'
        /// </summary>
        /// <param name="purchasedWithOrderItemId">Purchased with order item identifier</param>
        /// <returns>Gift card entries</returns>
        public virtual IList<GiftCard> GetGiftCardsByPurchasedWithOrderItemId(string purchasedWithOrderItemId)
        {
            if (String.IsNullOrEmpty(purchasedWithOrderItemId))
                return new List<GiftCard>();

            var query = _giftCardRepository.Table;

            query = query.Where(gc => gc.PurchasedWithOrderItem.Id == purchasedWithOrderItemId);
            query = query.OrderBy(gc => gc.Id);

            var giftCards = query.ToList();
            return giftCards;
        }

        /// <summary>
        /// Get active gift cards that are applied by a customer
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>Active gift cards</returns>
        public virtual IList<GiftCard> GetActiveGiftCardsAppliedByCustomer(Customer customer)
        {
            var result = new List<GiftCard>();
            if (customer == null)
                return result;

            string[] couponCodes = customer.ParseAppliedGiftCardCouponCodes();
            foreach (var couponCode in couponCodes)
            {
                var giftCards = GetAllGiftCards(isGiftCardActivated: true, giftCardCouponCode: couponCode);
                foreach (var gc in giftCards)
                {
                    if (gc.IsGiftCardValid())
                        result.Add(gc);
                }
            }

            return result;
        }

        /// <summary>
        /// Generate new gift card code
        /// </summary>
        /// <returns>Result</returns>
        public virtual string GenerateGiftCardCode()
        {
            int length = 13;
            string result = Guid.NewGuid().ToString();
            if (result.Length > length)
                result = result.Substring(0, length);
            return result;
        }

        #endregion
    }
}
