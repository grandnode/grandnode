using Grand.Domain.Catalog;
using MediatR;
using System.Collections.Generic;

namespace Grand.Services.Queries.Models.Catalog
{
    public class GetLowStockProducts : IRequest<(IList<Product> products, IList<ProductAttributeCombination> combinations)>
    {
        public string VendorId { get; set; }
        public string StoreId { get; set; }
    }
}
