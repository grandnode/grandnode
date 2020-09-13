using Grand.Api.DTOs.Shipping;
using Grand.Api.Queries.Models.Common;
using Grand.Domain.Data;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Api.Queries.Handlers.Shipping
{
    public class GetShippingMethodQueryHandler : IRequestHandler<GetQuery<ShippingMethodDto>, IMongoQueryable<ShippingMethodDto>>
    {
        private readonly IMongoDBContext _mongoDBContext;

        public GetShippingMethodQueryHandler(IMongoDBContext mongoDBContext)
        {
            _mongoDBContext = mongoDBContext;
        }
        public Task<IMongoQueryable<ShippingMethodDto>> Handle(GetQuery<ShippingMethodDto> request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Id))
                return Task.FromResult
                    (_mongoDBContext.Database()
                    .GetCollection<ShippingMethodDto>
                    (typeof(Domain.Shipping.ShippingMethod).Name)
                    .AsQueryable());
            else
                return Task.FromResult
                    (_mongoDBContext.Database()
                    .GetCollection<ShippingMethodDto>
                    (typeof(Domain.Shipping.ShippingMethod).Name)
                    .AsQueryable()
                    .Where(x => x.Id == request.Id));
        }
    }
}