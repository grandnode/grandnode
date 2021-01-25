using Grand.Domain.Catalog;
using Grand.Domain.Data;
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
    public class GetLowStockProductsQueryHandler : IRequestHandler<GetLowStockProducts, (IList<Product> products, IList<ProductAttributeCombination> combinations)>
    {
        private readonly IRepository<Product> _productRepository;

        public GetLowStockProductsQueryHandler(IRepository<Product> productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<(IList<Product> products, IList<ProductAttributeCombination> combinations)> Handle(GetLowStockProducts request, CancellationToken cancellationToken)
        {
            //Track inventory for product
            //simple products
            var query_simple_products = from p in _productRepository.Table
                                        where p.LowStock &&
                                        ((p.ProductTypeId == (int)ProductType.SimpleProduct || p.ProductTypeId == (int)ProductType.BundledProduct) 
                                        && p.ManageInventoryMethodId == (int)ManageInventoryMethod.ManageStock)
                                        select p;

            if (!string.IsNullOrEmpty(request.VendorId))
                query_simple_products = query_simple_products.Where(x => x.VendorId == request.VendorId);

            if (!string.IsNullOrEmpty(request.StoreId))
                query_simple_products = query_simple_products.Where(x => x.Stores.Contains(request.StoreId));

            var products = await query_simple_products.ToListAsync();

            //Track inventory for product by product attributes
            var query2_1 = from p in _productRepository.Table
                           where
                           p.ManageInventoryMethodId == (int)ManageInventoryMethod.ManageStockByAttributes &&
                           (request.VendorId == "" || p.VendorId == request.VendorId) &&
                           (request.StoreId == "" || p.Stores.Contains(request.StoreId))
                           from c in p.ProductAttributeCombinations
                           select new ProductAttributeCombination() {
                               ProductId = p.Id,
                               StockQuantity = c.StockQuantity,
                               Attributes = c.Attributes,
                               AllowOutOfStockOrders = c.AllowOutOfStockOrders,
                               Id = c.Id,
                               Gtin = c.Gtin,
                               ManufacturerPartNumber = c.ManufacturerPartNumber,
                               NotifyAdminForQuantityBelow = c.NotifyAdminForQuantityBelow,
                               OverriddenPrice = c.OverriddenPrice,
                               Sku = c.Sku
                           };

            var query2_2 = from c in query2_1
                           where c.StockQuantity <= 0
                           select c;

            var combinations = await query2_2.ToListAsync();

            return (products, combinations);
        }
    }
}
