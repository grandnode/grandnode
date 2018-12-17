using Grand.Api.Model.Catalog;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Api.Services
{
    public interface ICategoryApiService
    {
        Category GetById(string id);
        IMongoQueryable<Category> GetCategories();
        Category InsertCategory(Category model);
        Category UpdateCategory(Category model);
        void DeleteCategory(Category model);
    }
}
