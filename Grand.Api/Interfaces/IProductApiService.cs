using Grand.Api.DTOs.Catalog;
using MongoDB.Driver.Linq;
using System.Threading.Tasks;

namespace Grand.Api.Interfaces
{
    public interface IProductApiService
    {
        Task<ProductDto> GetById(string id);
        IMongoQueryable<ProductDto> GetProducts();
        Task<ProductDto> InsertOrUpdateProduct(ProductDto model);
        Task<ProductDto> InsertProduct(ProductDto model);
        Task<ProductDto> UpdateProduct(ProductDto model);
        Task DeleteProduct(ProductDto model);
        Task UpdateStock(ProductDto model, string warehouseId, int stock);

        Task InsertProductCategory(ProductDto product, ProductCategoryDto model);
        Task UpdateProductCategory(ProductDto product, ProductCategoryDto model);
        Task DeleteProductCategory(ProductDto product, string categoryId);

        Task InsertProductManufacturer(ProductDto product, ProductManufacturerDto model);
        Task UpdateProductManufacturer(ProductDto product, ProductManufacturerDto model);
        Task DeleteProductManufacturer(ProductDto product, string manufacturerId);

        Task InsertProductPicture(ProductDto product, ProductPictureDto model);
        Task UpdateProductPicture(ProductDto product, ProductPictureDto model);
        Task DeleteProductPicture(ProductDto product, string pictureId);

        Task InsertProductSpecification(ProductDto product, ProductSpecificationAttributeDto model);
        Task UpdateProductSpecification(ProductDto product, ProductSpecificationAttributeDto model);
        Task DeleteProductSpecification(ProductDto product, string id);

        Task InsertProductTierPrice(ProductDto product, ProductTierPriceDto model);
        Task UpdateProductTierPrice(ProductDto product, ProductTierPriceDto model);
        Task DeleteProductTierPrice(ProductDto product, string id);

    }
}
