using Grand.Api.DTOs.Catalog;
using MongoDB.Driver.Linq;

namespace Grand.Api.Services
{
    public interface IProductAttributeApiService
    {
        ProductAttributeDto GetById(string id);
        IMongoQueryable<ProductAttributeDto> GetProductAttributes();
        ProductAttributeDto InsertProductAttribute(ProductAttributeDto model);
        ProductAttributeDto UpdateProductAttribute(ProductAttributeDto model);
        void DeleteProductAttribute(ProductAttributeDto model);
    }
}
