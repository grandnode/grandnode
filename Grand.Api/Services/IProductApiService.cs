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
    }
}
