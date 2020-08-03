using Grand.Domain.Orders;
using MediatR;
using MongoDB.Driver.Linq;
using System;

namespace Grand.Services.Queries.Models.Orders
{
    public class GetGiftCardQuery : IRequest<IMongoQueryable<GiftCard>>
    {
        public string GiftCardId { get; set; } = "";
        public string PurchasedWithOrderItemId { get; set; } = "";
        public bool? IsGiftCardActivated { get; set; }
        public string GiftCardCouponCode { get; set; } = "";
        public DateTime? CreatedFromUtc { get; set; } = null;
        public DateTime? CreatedToUtc { get; set; } = null;
        public string RecipientName { get; set; } = null;
        public int PageIndex { get; set; } = 0;
        public int PageSize { get; set; } = int.MaxValue;
    }
}
