using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Services.Queries.Models.Catalog;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Grand.Services.Queries.Handlers.Catalog
{
    public class GetBidsByProductIdQueryHandler : IRequestHandler<GetBidsByProductIdQuery, IList<Bid>>
    {
        private readonly IRepository<Bid> _bidRepository;

        public GetBidsByProductIdQueryHandler(IRepository<Bid> bidRepository)
        {
            _bidRepository = bidRepository;
        }

        public async Task<IList<Bid>> Handle(GetBidsByProductIdQuery request, CancellationToken cancellationToken)
        {
            return await _bidRepository
                .Table.Where(x => x.ProductId == request.ProductId)
                .OrderByDescending(x => x.Date)
                .ToListAsync();
        }
    }
}
