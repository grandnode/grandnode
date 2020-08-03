using Grand.Domain;
using Grand.Domain.Catalog;
using Grand.Services.Catalog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class ProductList
    {
        public static async Task<IPagedList<Product>> PrepareProductList(this IProductService productService, string searchCategoryId, 
            string searchManufacturerId, string searchStoreId, string searchVendorId, int productTypeId,
            string searchProductName, int pageIndex, int pageSize)
        {
            var searchCategoryIds = new List<string>();
            if (!String.IsNullOrEmpty(searchCategoryId))
                searchCategoryIds.Add(searchCategoryId);

            var products = (await productService.SearchProducts(
                categoryIds: searchCategoryIds,
                manufacturerId: searchManufacturerId,
                storeId: searchStoreId,
                vendorId: searchVendorId,
                productType: productTypeId > 0 ? (ProductType?)productTypeId : null,
                keywords: searchProductName,
                pageIndex: pageIndex - 1,
                pageSize: pageSize,
                showHidden: true
                )).products;

            return products;
        }
    }
}
