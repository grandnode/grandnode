using Grand.Domain;
using Grand.Domain.Data;
using Grand.Domain.Orders;
using Grand.Services.Events;
using Grand.Services.Queries.Models.Orders;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Orders
{
    /// <summary>
    /// Gift card service
    /// </summary>
    public partial class GiftCardService : IGiftCardService
    {
        #region Fields

        private readonly IRepository<GiftCard> _giftCardRepository;
        private readonly IMediator _mediator;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="giftCardRepository">Gift card context</param>
        /// <param name="mediator">Mediator</param>
        public GiftCardService(IRepository<GiftCard> giftCardRepository, IMediator mediator)
        {
            _giftCardRepository = giftCardRepository;
            _mediator = mediator;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a gift card
        /// </summary>
        /// <param name="giftCard">Gift card</param>
        public virtual async Task DeleteGiftCard(GiftCard giftCard)
        {
            if (giftCard == null)
                throw new ArgumentNullException("giftCard");

            await _giftCardRepository.DeleteAsync(giftCard);

            //event notification
            await _mediator.EntityDeleted(giftCard);
        }

        /// <summary>
        /// Gets a gift card
        /// </summary>
        /// <param name="giftCardId">Gift card identifier</param>
        /// <returns>Gift card entry</returns>
        public virtual Task<GiftCard> GetGiftCardById(string giftCardId)
        {
            return _giftCardRepository.GetByIdAsync(giftCardId);
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
        public virtual async Task<IPagedList<GiftCard>> GetAllGiftCards(string purchasedWithOrderItemId = "",
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            bool? isGiftCardActivated = null, string giftCardCouponCode = null,
            string recipientName = null,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var model = new GetGiftCardQuery() {
                CreatedFromUtc = createdFromUtc,
                CreatedToUtc = createdToUtc,
                GiftCardCouponCode = giftCardCouponCode,
                IsGiftCardActivated = isGiftCardActivated,
                PageIndex = pageIndex,
                PageSize = pageSize,
                PurchasedWithOrderItemId = purchasedWithOrderItemId,
                RecipientName = recipientName
            };

            var query = await _mediator.Send(model);
            return await PagedList<GiftCard>.Create(query, pageIndex, pageSize);
        }

        public virtual async Task<IList<GiftCardUsageHistory>> GetAllGiftCardUsageHistory(string orderId = "")
        {
            var query = from g in _giftCardRepository.Table
                        from h in g.GiftCardUsageHistory
                        select h;

            query = query.Where(x => x.UsedWithOrderId == orderId);
            return await query.ToListAsync();
        }

        /// <summary>
        /// Inserts a gift card
        /// </summary>
        /// <param name="giftCard">Gift card</param>
        public virtual async Task InsertGiftCard(GiftCard giftCard)
        {
            if (giftCard == null)
                throw new ArgumentNullException("giftCard");
            giftCard.GiftCardCouponCode = giftCard.GiftCardCouponCode.ToLowerInvariant();
            await _giftCardRepository.InsertAsync(giftCard);

            //event notification
            await _mediator.EntityInserted(giftCard);
        }

        /// <summary>
        /// Updates the gift card
        /// </summary>
        /// <param name="giftCard">Gift card</param>
        public virtual async Task UpdateGiftCard(GiftCard giftCard)
        {
            if (giftCard == null)
                throw new ArgumentNullException("giftCard");

            giftCard.GiftCardCouponCode = giftCard.GiftCardCouponCode.ToLowerInvariant();
            await _giftCardRepository.UpdateAsync(giftCard);

            //event notification
            await _mediator.EntityUpdated(giftCard);
        }

        /// <summary>
        /// Gets gift cards by 'PurchasedWithOrderItemId'
        /// </summary>
        /// <param name="purchasedWithOrderItemId">Purchased with order item identifier</param>
        /// <returns>Gift card entries</returns>
        public virtual async Task<IList<GiftCard>> GetGiftCardsByPurchasedWithOrderItemId(string purchasedWithOrderItemId)
        {
            if (String.IsNullOrEmpty(purchasedWithOrderItemId))
                return new List<GiftCard>();

            var query = _giftCardRepository.Table;

            query = query.Where(gc => gc.PurchasedWithOrderItem.Id == purchasedWithOrderItemId);
            query = query.OrderBy(gc => gc.Id);

            return await query.ToListAsync();
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
