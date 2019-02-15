using Grand.Api.DTOs.Catalog;
using MongoDB.Driver.Linq;

namespace Grand.Api.Interfaces
{
    public interface ICategoryApiService
    {
        CategoryDto GetById(string id);
        IMongoQueryable<CategoryDto> GetCategories();
        CategoryDto InsertOrUpdateCategory(CategoryDto model);
        CategoryDto InsertCategory(CategoryDto model);
        CategoryDto UpdateCategory(CategoryDto model);
        void DeleteCategory(CategoryDto model);
    }
}
