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

namespace Grand.Services.Queries.Handlers.Catalog
{
    public class GetSuggestedProductsQueryHandler : IRequestHandler<GetSuggestedProductsQuery, IList<Product>>
    {
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : customer ID
        /// </remarks>
        private const string PRODUCTS_CUSTOMER_TAG = "Grand.product.ct-{0}";

        #region Fields

        private readonly IProductService _productService;
        private readonly ICacheManager _cacheManager;
        private readonly IRepository<CustomerTagProduct> _customerTagProductRepository;

        #endregion

        public GetSuggestedProductsQueryHandler(
            IProductService productService,
            ICacheManager cacheManager,
            IRepository<CustomerTagProduct> customerTagProductRepository)
        {
            _productService = productService;
            _cacheManager = cacheManager;
            _customerTagProductRepository = customerTagProductRepository;
        }

        public async Task<IList<Product>> Handle(GetSuggestedProductsQuery request, CancellationToken cancellationToken)
        {
            return await _cacheManager.GetAsync(string.Format(PRODUCTS_CUSTOMER_TAG, string.Join(",", request.CustomerTagIds)), async () =>
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
