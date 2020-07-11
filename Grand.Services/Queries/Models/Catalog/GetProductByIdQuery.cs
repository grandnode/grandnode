using Grand.Domain.Catalog;
using MediatR;

namespace Grand.Services.Queries.Models.Catalog
{
    public class GetProductByIdQuery : IRequest<Product>
    {
        public string Id { get; set; }
    }
}
