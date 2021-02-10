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
    public class GetSuggestedProductsQueryHandler : IRequestHandler<GetSuggestedProductsQuery, IList<Product>>
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly ICacheBase _cacheBase;
        private readonly IRepository<CustomerTagProduct> _customerTagProductRepository;

        #endregion

        public GetSuggestedProductsQueryHandler(
            IProductService productService,
            ICacheBase cacheManager,
            IRepository<CustomerTagProduct> customerTagProductRepository)
        {
            _productService = productService;
            _cacheBase = cacheManager;
            _customerTagProductRepository = customerTagProductRepository;
        }

        public async Task<IList<Product>> Handle(GetSuggestedProductsQuery request, CancellationToken cancellationToken)
        {
            return await _cacheBase.GetAsync(string.Format(CacheKey.PRODUCTS_CUSTOMER_TAG, string.Join(",", request.CustomerTagIds)), async () =>
            {
                var query = from cr in _customerTagProductRepository.Table
                            where request.CustomerTagIds.Contains(cr.CustomerTagId)
                            orderby cr.DisplayOrder
                            select cr.ProductId;

                var productIds = await query.Take(request.ProductsNumber).ToListAsync();

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
