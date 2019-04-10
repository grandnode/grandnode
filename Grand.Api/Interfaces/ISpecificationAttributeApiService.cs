using Grand.Api.DTOs.Catalog;
using MongoDB.Driver.Linq;
using System.Threading.Tasks;

namespace Grand.Api.Interfaces
{
    public interface ISpecificationAttributeApiService
    {
        Task<SpecificationAttributeDto> GetById(string id);
        IMongoQueryable<SpecificationAttributeDto> GetSpecificationAttributes();
        Task<SpecificationAttributeDto> InsertOrUpdateSpecificationAttribute(SpecificationAttributeDto model);
        Task<SpecificationAttributeDto> InsertSpecificationAttribute(SpecificationAttributeDto model);
        Task<SpecificationAttributeDto> UpdateSpecificationAttribute(SpecificationAttributeDto model);
        Task DeleteSpecificationAttribute(SpecificationAttributeDto model);
    }
}
