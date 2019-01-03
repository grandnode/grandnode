using Grand.Api.DTOs.Catalog;
using MongoDB.Driver.Linq;

namespace Grand.Api.Services
{
    public interface IProductApiService
    {
        ProductDto GetById(string id);
        IMongoQueryable<ProductDto> GetProducts();
        ProductDto InsertOrUpdateProduct(ProductDto model);
        ProductDto InsertProduct(ProductDto model);
        ProductDto UpdateProduct(ProductDto model);
        void DeleteProduct(ProductDto model);
        void UpdateStock(ProductDto model, string warehouseId, int stock);
        
        void InsertProductCategory(ProductDto product, ProductCategoryDto model);
        void UpdateProductCategory(ProductDto product, ProductCategoryDto model);
        void DeleteProductCategory(ProductDto product, string categoryId);

        void InsertProductManufacturer(ProductDto product, ProductManufacturerDto model);
        void UpdateProductManufacturer(ProductDto product, ProductManufacturerDto model);
        void DeleteProductManufacturer(ProductDto product, string manufacturerId);
    }
}
