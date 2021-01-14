using Grand.Domain.Catalog;
using Grand.Admin.Models.Templates;

namespace Grand.Admin.Extensions
{
    public static class CategoryTemplateMappingExtensions
    {
        public static CategoryTemplateModel ToModel(this CategoryTemplate entity)
        {
            return entity.MapTo<CategoryTemplate, CategoryTemplateModel>();
        }

        public static CategoryTemplate ToEntity(this CategoryTemplateModel model)
        {
            return model.MapTo<CategoryTemplateModel, CategoryTemplate>();
        }

        public static CategoryTemplate ToEntity(this CategoryTemplateModel model, CategoryTemplate destination)
        {
            return model.MapTo(destination);
        }
    }
}