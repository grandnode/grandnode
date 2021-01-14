using Grand.Domain.Tax;
using Grand.Admin.Models.Tax;

namespace Grand.Admin.Extensions
{
    public static class TaxCategoryMappingExtensions
    {
        public static TaxCategoryModel ToModel(this TaxCategory entity)
        {
            return entity.MapTo<TaxCategory, TaxCategoryModel>();
        }

        public static TaxCategory ToEntity(this TaxCategoryModel model)
        {
            return model.MapTo<TaxCategoryModel, TaxCategory>();
        }

        public static TaxCategory ToEntity(this TaxCategoryModel model, TaxCategory destination)
        {
            return model.MapTo(destination);
        }
    }
}