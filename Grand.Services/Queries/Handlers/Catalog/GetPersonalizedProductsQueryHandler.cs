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
    public class GetPersonalizedProductsQueryHandler : IRequestHandler<GetPersonalizedProductsQuery, IList<Product>>
    {
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : customer ID
        /// </remarks>
        private const string PRODUCTS_CUSTOMER_PERSONAL = "Grand.product.personal-{0}";

        private readonly IProductService _productService;
        private readonly ICacheManager _cacheManager;
        private readonly IRepository<CustomerProduct> _customerProductRepository;

        public GetPersonalizedProductsQueryHandler(
            IProductService productService,
            ICacheManager cacheManager,
            IRepository<CustomerProduct> customerProductRepository)
        {
            _productService = productService;
            _cacheManager = cacheManager;
            _customerProductRepository = customerProductRepository;
        }

        public async Task<IList<Product>> Handle(GetPersonalizedProductsQuery request, CancellationToken cancellationToken)
        {
            return await _cacheManager.GetAsync(string.Format(PRODUCTS_CUSTOMER_PERSONAL, request.CustomerId), async () =>
            {
                var query = from cr in _customerProductRepository.Table
                            where cr.CustomerId == request.CustomerId
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
