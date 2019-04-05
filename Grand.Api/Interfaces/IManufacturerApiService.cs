using Grand.Api.DTOs.Catalog;
using MongoDB.Driver.Linq;
using System.Threading.Tasks;

namespace Grand.Api.Interfaces
{
    public interface IManufacturerApiService
    {
        Task<ManufacturerDto> GetById(string id);
        IMongoQueryable<ManufacturerDto> GetManufacturers();
        Task<ManufacturerDto> InsertOrUpdateManufacturer(ManufacturerDto model);
        Task<ManufacturerDto> InsertManufacturer(ManufacturerDto model);
        Task<ManufacturerDto> UpdateManufacturer(ManufacturerDto model);
        Task DeleteManufacturer(ManufacturerDto model);
    }
}
