using Grand.Domain.Catalog;
using Grand.Admin.Models.Catalog;

namespace Grand.Admin.Extensions
{
    public static class CategoryMappingExtensions
    {
        public static CategoryModel ToModel(this Category entity)
        {
            return entity.MapTo<Category, CategoryModel>();
        }

        public static Category ToEntity(this CategoryModel model)
        {
            return model.MapTo<CategoryModel, Category>();
        }

        public static Category ToEntity(this CategoryModel model, Category destination)
        {
            return model.MapTo(destination);
        }

    }
}