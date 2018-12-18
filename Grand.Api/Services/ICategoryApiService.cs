using Grand.Api.DTOs;
using Grand.Api.DTOs.Catalog;
using MongoDB.Driver.Linq;

namespace Grand.Api.Services
{
    public interface ICategoryApiService
    {
        CategoryDTO GetById(string id);
        IMongoQueryable<CategoryDTO> GetCategories();
        CategoryDTO InsertCategory(CategoryDTO model);
        CategoryDTO UpdateCategory(CategoryDTO model);
        void DeleteCategory(CategoryDTO model);
    }
}
