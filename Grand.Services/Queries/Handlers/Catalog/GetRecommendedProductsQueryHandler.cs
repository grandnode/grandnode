using Grand.Core.Caching;
using Grand.Domain.Data;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Services.Catalog;
using Grand.Services.Queries.Models.Catalog;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grand.Core.Caching.Constants;

namespace Grand.Services.Queries.Handlers.Catalog
{
    public class GetRecommendedProductsQueryHandler : IRequestHandler<GetRecommendedProductsQuery, IList<Product>>
    {
       
        private readonly IProductService _productService;
        private readonly ICacheBase _cacheBase;
        private readonly IRepository<CustomerRoleProduct> _customerRoleProductRepository;

        public GetRecommendedProductsQueryHandler(
            IProductService productService,
            ICacheBase cacheManager,
            IRepository<CustomerRoleProduct> customerRoleProductRepository)
        {
            _productService = productService;
            _cacheBase = cacheManager;
            _customerRoleProductRepository = customerRoleProductRepository;
        }

        public async Task<IList<Product>> Handle(GetRecommendedProductsQuery request, CancellationToken cancellationToken)
        {
            return await _cacheBase.GetAsync(string.Format(CacheKey.PRODUCTS_CUSTOMER_ROLE, string.Join(",", request.CustomerRoleIds), request.StoreId), async () =>
            {
                var query = from cr in _customerRoleProductRepository.Table
                            where request.CustomerRoleIds.Contains(cr.CustomerRoleId)
                            orderby cr.DisplayOrder
                            select cr.ProductId;

                var productIds = await query.ToListAsync();

                var products = new List<Product>();
                var ids = await _productService.GetProductsByIds(productIds.Distinct().ToArray());
                foreach (var product in ids)
                    if (product.Published)
                        products.Add(product);

                return products;
            });

        }
    }
}
