using Grand.Domain.Catalog;
using Grand.Web.Areas.Admin.Models.Catalog;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class SpecificationAttributeMappingExtensions
    {
        public static SpecificationAttributeModel ToModel(this SpecificationAttribute entity)
        {
            return entity.MapTo<SpecificationAttribute, SpecificationAttributeModel>();
        }

        public static SpecificationAttribute ToEntity(this SpecificationAttributeModel model)
        {
            return model.MapTo<SpecificationAttributeModel, SpecificationAttribute>();
        }

        public static SpecificationAttribute ToEntity(this SpecificationAttributeModel model, SpecificationAttribute destination)
        {
            return model.MapTo(destination);
        }

        //attribute options
        public static SpecificationAttributeOptionModel ToModel(this SpecificationAttributeOption entity)
        {
            return entity.MapTo<SpecificationAttributeOption, SpecificationAttributeOptionModel>();
        }

        public static SpecificationAttributeOption ToEntity(this SpecificationAttributeOptionModel model)
        {
            return model.MapTo<SpecificationAttributeOptionModel, SpecificationAttributeOption>();
        }

        public static SpecificationAttributeOption ToEntity(this SpecificationAttributeOptionModel model, SpecificationAttributeOption destination)
        {
            return model.MapTo(destination);
        }
    }
}