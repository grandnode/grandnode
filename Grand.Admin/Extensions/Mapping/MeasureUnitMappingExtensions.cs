using Grand.Domain.Directory;
using Grand.Admin.Models.Directory;

namespace Grand.Admin.Extensions
{
    public static class MeasureUnitMappingExtensions
    {
        public static MeasureUnitModel ToModel(this MeasureUnit entity)
        {
            return entity.MapTo<MeasureUnit, MeasureUnitModel>();
        }

        public static MeasureUnit ToEntity(this MeasureUnitModel model)
        {
            return model.MapTo<MeasureUnitModel, MeasureUnit>();
        }

        public static MeasureUnit ToEntity(this MeasureUnitModel model, MeasureUnit destination)
        {
            return model.MapTo(destination);
        }
    }
}