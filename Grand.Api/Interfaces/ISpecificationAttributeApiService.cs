using Grand.Api.DTOs.Catalog;
using MongoDB.Driver.Linq;

namespace Grand.Api.Interfaces
{
    public interface ISpecificationAttributeApiService
    {
        SpecificationAttributeDto GetById(string id);
        IMongoQueryable<SpecificationAttributeDto> GetSpecificationAttributes();
        SpecificationAttributeDto InsertOrUpdateSpecificationAttribute(SpecificationAttributeDto model);
        SpecificationAttributeDto InsertSpecificationAttribute(SpecificationAttributeDto model);
        SpecificationAttributeDto UpdateSpecificationAttribute(SpecificationAttributeDto model);
        void DeleteSpecificationAttribute(SpecificationAttributeDto model);
    }
}
