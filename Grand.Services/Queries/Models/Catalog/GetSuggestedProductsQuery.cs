using Grand.Domain.Catalog;
using MediatR;
using System.Collections.Generic;

namespace Grand.Services.Queries.Models.Catalog
{
    public class GetSuggestedProductsQuery : IRequest<IList<Product>>
    {
        public string[] CustomerTagIds { get; set; }
        public int ProductsNumber { get; set; }
    }
}
