using Grand.Api.DTOs.Catalog;
using MongoDB.Driver.Linq;

namespace Grand.Api.Interfaces
{
    public interface IProductAttributeApiService
    {
        ProductAttributeDto GetById(string id);
        IMongoQueryable<ProductAttributeDto> GetProductAttributes();
        ProductAttributeDto InsertOrUpdateProductAttribute(ProductAttributeDto model);
        ProductAttributeDto InsertProductAttribute(ProductAttributeDto model);
        ProductAttributeDto UpdateProductAttribute(ProductAttributeDto model);
        void DeleteProductAttribute(ProductAttributeDto model);
    }
}
