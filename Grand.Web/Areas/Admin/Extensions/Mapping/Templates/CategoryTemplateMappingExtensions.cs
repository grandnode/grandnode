using Grand.Domain.Catalog;
using Grand.Web.Areas.Admin.Models.Templates;

namespace Grand.Web.Areas.Admin.Extensions
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