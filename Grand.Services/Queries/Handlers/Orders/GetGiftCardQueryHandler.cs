using Grand.Domain.Data;
using Grand.Domain.Orders;
using Grand.Services.Queries.Models.Orders;
using MediatR;
using MongoDB.Driver.Linq;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Queries.Handlers.Orders
{
    public class GetGiftCardQueryHandler : IRequestHandler<GetGiftCardQuery, IMongoQueryable<GiftCard>>
    {
        private readonly IRepository<GiftCard> _giftCardRepository;

        public GetGiftCardQueryHandler(IRepository<GiftCard> giftCardRepository)
        {
            _giftCardRepository = giftCardRepository;
        }

        public Task<IMongoQueryable<GiftCard>> Handle(GetGiftCardQuery request, CancellationToken cancellationToken)
        {
            var query = _giftCardRepository.Table;

            if (!string.IsNullOrEmpty(request.GiftCardId))
                query = query.Where(gc => gc.Id == request.GiftCardId);

            if (!string.IsNullOrEmpty(request.PurchasedWithOrderItemId))
                query = query.Where(gc => gc.PurchasedWithOrderItem.Id == request.PurchasedWithOrderItemId);

            if (request.CreatedFromUtc.HasValue)
                query = query.Where(gc => request.CreatedFromUtc.Value <= gc.CreatedOnUtc);
            if (request.CreatedToUtc.HasValue)
                query = query.Where(gc => request.CreatedToUtc.Value >= gc.CreatedOnUtc);
            if (request.IsGiftCardActivated.HasValue)
                query = query.Where(gc => gc.IsGiftCardActivated == request.IsGiftCardActivated.Value);
            if (!string.IsNullOrEmpty(request.GiftCardCouponCode))
            {
                query = query.Where(gc => gc.GiftCardCouponCode == request.GiftCardCouponCode.ToLowerInvariant());
            }
            if (!string.IsNullOrWhiteSpace(request.RecipientName))
                query = query.Where(c => c.RecipientName.Contains(request.RecipientName));
            query = query.OrderByDescending(gc => gc.CreatedOnUtc);

            return Task.FromResult(query);
        }
    }
}
