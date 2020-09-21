using Grand.Api.DTOs.Customers;
using Grand.Api.Queries.Models.Common;
using Grand.Domain.Data;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Api.Queries.Handlers.Customers
{
    public class GetCustomerRoleQueryHandler : IRequestHandler<GetQuery<CustomerRoleDto>, IMongoQueryable<CustomerRoleDto>>
    {
        private readonly IMongoDBContext _mongoDBContext;

        public GetCustomerRoleQueryHandler(IMongoDBContext mongoDBContext)
        {
            _mongoDBContext = mongoDBContext;
        }
        public Task<IMongoQueryable<CustomerRoleDto>> Handle(GetQuery<CustomerRoleDto> request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Id))
                return Task.FromResult(
                    _mongoDBContext.Database()
                    .GetCollection<CustomerRoleDto>
                    (typeof(Domain.Customers.CustomerRole).Name)
                    .AsQueryable());
            else
                return Task.FromResult(_mongoDBContext.Database()
                    .GetCollection<CustomerRoleDto>(typeof(Domain.Customers.CustomerRole).Name)
                    .AsQueryable()
                    .Where(x => x.Id == request.Id));
        }
    }
}
