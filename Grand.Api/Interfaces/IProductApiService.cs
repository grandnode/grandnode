using Grand.Api.DTOs.Catalog;
using MongoDB.Driver.Linq;

namespace Grand.Api.Interfaces
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

        void InsertProductPicture(ProductDto product, ProductPictureDto model);
        void UpdateProductPicture(ProductDto product, ProductPictureDto model);
        void DeleteProductPicture(ProductDto product, string pictureId);

        void InsertProductSpecification(ProductDto product, ProductSpecificationAttributeDto model);
        void UpdateProductSpecification(ProductDto product, ProductSpecificationAttributeDto model);
        void DeleteProductSpecification(ProductDto product, string id);

    }
}
