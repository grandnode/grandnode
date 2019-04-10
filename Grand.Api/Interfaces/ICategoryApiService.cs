using Grand.Api.DTOs.Catalog;
using MongoDB.Driver.Linq;
using System.Threading.Tasks;

namespace Grand.Api.Interfaces
{
    public interface ICategoryApiService
    {
        Task<CategoryDto> GetById(string id);
        IMongoQueryable<CategoryDto> GetCategories();
        Task<CategoryDto> InsertOrUpdateCategory(CategoryDto model);
        Task<CategoryDto> InsertCategory(CategoryDto model);
        Task<CategoryDto> UpdateCategory(CategoryDto model);
        Task DeleteCategory(CategoryDto model);
    }
}
