using Grand.Domain.Catalog;
using Grand.Web.Areas.Admin.Models.Catalog;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class ProductAttributeMappingExtensions
    {
        public static ProductAttributeModel ToModel(this ProductAttribute entity)
        {
            return entity.MapTo<ProductAttribute, ProductAttributeModel>();
        }

        public static ProductAttribute ToEntity(this ProductAttributeModel model)
        {
            return model.MapTo<ProductAttributeModel, ProductAttribute>();
        }

        public static ProductAttribute ToEntity(this ProductAttributeModel model, ProductAttribute destination)
        {
            return model.MapTo(destination);
        }

        //value
        public static PredefinedProductAttributeValue ToEntity(this PredefinedProductAttributeValueModel model)
        {
            return model.MapTo<PredefinedProductAttributeValueModel, PredefinedProductAttributeValue>();
        }
        public static PredefinedProductAttributeValueModel ToModel(this PredefinedProductAttributeValue entity)
        {
            return entity.MapTo<PredefinedProductAttributeValue, PredefinedProductAttributeValueModel>();
        }
        public static PredefinedProductAttributeValue ToEntity(this PredefinedProductAttributeValueModel model, PredefinedProductAttributeValue destination)
        {
            return model.MapTo(destination);
        }
    }
}