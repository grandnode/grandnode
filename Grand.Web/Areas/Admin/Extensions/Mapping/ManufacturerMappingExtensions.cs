using Grand.Domain.Catalog;
using Grand.Web.Areas.Admin.Models.Catalog;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class ManufacturerMappingExtensions
    {
        public static ManufacturerModel ToModel(this Manufacturer entity)
        {
            return entity.MapTo<Manufacturer, ManufacturerModel>();
        }

        public static Manufacturer ToEntity(this ManufacturerModel model)
        {
            return model.MapTo<ManufacturerModel, Manufacturer>();
        }

        public static Manufacturer ToEntity(this ManufacturerModel model, Manufacturer destination)
        {
            return model.MapTo(destination);
        }
    }
}