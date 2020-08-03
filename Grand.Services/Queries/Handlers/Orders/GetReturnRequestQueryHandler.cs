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
    public class GetReturnRequestQueryHandler : IRequestHandler<GetReturnRequestQuery, IMongoQueryable<ReturnRequest>>
    {
        private readonly IRepository<ReturnRequest> _returnRequestRepository;

        public GetReturnRequestQueryHandler(IRepository<ReturnRequest> returnRequestRepository)
        {
            _returnRequestRepository = returnRequestRepository;
        }

        public Task<IMongoQueryable<ReturnRequest>> Handle(GetReturnRequestQuery request, CancellationToken cancellationToken)
        {
            var query = _returnRequestRepository.Table;
            if (!string.IsNullOrEmpty(request.StoreId))
                query = query.Where(rr => request.StoreId == rr.StoreId);

            if (!string.IsNullOrEmpty(request.CustomerId))
                query = query.Where(rr => request.CustomerId == rr.CustomerId);

            if (!string.IsNullOrEmpty(request.VendorId))
                query = query.Where(rr => request.VendorId == rr.VendorId);

            if (!string.IsNullOrEmpty(request.OwnerId))
                query = query.Where(rr => request.OwnerId == rr.OwnerId);

            if (request.Rs.HasValue)
            {
                var returnStatusId = (int)request.Rs.Value;
                query = query.Where(rr => rr.ReturnRequestStatusId == returnStatusId);
            }
            if (!string.IsNullOrEmpty(request.OrderItemId))
                query = query.Where(rr => rr.ReturnRequestItems.Any(x => x.OrderItemId == request.OrderItemId));

            if (request.CreatedFromUtc.HasValue)
                query = query.Where(rr => request.CreatedFromUtc.Value <= rr.CreatedOnUtc);

            if (request.CreatedToUtc.HasValue)
                query = query.Where(rr => request.CreatedToUtc.Value >= rr.CreatedOnUtc);

            query = query.OrderByDescending(rr => rr.CreatedOnUtc).ThenByDescending(rr => rr.Id);

            return Task.FromResult(query);
        }
    }
}
