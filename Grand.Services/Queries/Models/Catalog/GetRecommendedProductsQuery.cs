using Grand.Domain.Catalog;
using MediatR;
using System.Collections.Generic;

namespace Grand.Services.Queries.Models.Catalog
{
    public class GetRecommendedProductsQuery : IRequest<IList<Product>>
    {
        public string[] CustomerRoleIds { get; set; }
    }
}
