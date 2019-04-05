using Grand.Api.DTOs.Catalog;
using MongoDB.Driver.Linq;
using System.Threading.Tasks;

namespace Grand.Api.Interfaces
{
    public interface IProductAttributeApiService
    {
        Task<ProductAttributeDto> GetById(string id);
        IMongoQueryable<ProductAttributeDto> GetProductAttributes();
        Task<ProductAttributeDto> InsertOrUpdateProductAttribute(ProductAttributeDto model);
        Task<ProductAttributeDto> InsertProductAttribute(ProductAttributeDto model);
        Task<ProductAttributeDto> UpdateProductAttribute(ProductAttributeDto model);
        Task DeleteProductAttribute(ProductAttributeDto model);
    }
}
