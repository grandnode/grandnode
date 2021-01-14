using Grand.Domain.Catalog;
using Grand.Admin.Models.Catalog;

namespace Grand.Admin.Extensions
{
    public static class ProductAttributeMappingMappingExtensions
    {
        public static ProductModel.ProductAttributeMappingModel ToModel(this ProductAttributeMapping entity)
        {
            return entity.MapTo<ProductAttributeMapping, ProductModel.ProductAttributeMappingModel>();
        }

        public static ProductAttributeMapping ToEntity(this ProductModel.ProductAttributeMappingModel model)
        {
            return model.MapTo<ProductModel.ProductAttributeMappingModel, ProductAttributeMapping>();
        }

        public static ProductAttributeMapping ToEntity(this ProductModel.ProductAttributeMappingModel model, ProductAttributeMapping destination)
        {
            return model.MapTo(destination);
        }
    }
}