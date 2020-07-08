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
    public class GetRecommendedProductsQueryHandler : IRequestHandler<GetRecommendedProductsQuery, IList<Product>>
    {
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : customer ID
        /// </remarks>
        private const string PRODUCTS_CUSTOMER_ROLE = "Grand.product.cr-{0}";

        private readonly IProductService _productService;
        private readonly ICacheManager _cacheManager;
        private readonly IRepository<CustomerRoleProduct> _customerRoleProductRepository;

        public GetRecommendedProductsQueryHandler(
            IProductService productService,
            ICacheManager cacheManager,
            IRepository<CustomerRoleProduct> customerRoleProductRepository)
        {
            _productService = productService;
            _cacheManager = cacheManager;
            _customerRoleProductRepository = customerRoleProductRepository;
        }

        public async Task<IList<Product>> Handle(GetRecommendedProductsQuery request, CancellationToken cancellationToken)
        {
            return await _cacheManager.GetAsync(string.Format(PRODUCTS_CUSTOMER_ROLE, string.Join(",", request.CustomerRoleIds)), async () =>
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
