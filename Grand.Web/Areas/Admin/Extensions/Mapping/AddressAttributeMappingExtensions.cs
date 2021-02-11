using Grand.Core.Mapper;
using Grand.Domain.Common;
using Grand.Web.Areas.Admin.Models.Common;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class AddressAttributeMappingExtensions
    {
        //attributes
        public static AddressAttributeModel ToModel(this AddressAttribute entity)
        {
            return entity.MapTo<AddressAttribute, AddressAttributeModel>();
        }

        public static AddressAttribute ToEntity(this AddressAttributeModel model)
        {
            return model.MapTo<AddressAttributeModel, AddressAttribute>();
        }

        public static AddressAttribute ToEntity(this AddressAttributeModel model, AddressAttribute destination)
        {
            return model.MapTo(destination);
        }

        //attributes value
        public static AddressAttributeValueModel ToModel(this AddressAttributeValue entity)
        {
            return entity.MapTo<AddressAttributeValue, AddressAttributeValueModel>();
        }
        public static AddressAttributeValue ToEntity(this AddressAttributeValueModel model)
        {
            return model.MapTo<AddressAttributeValueModel, AddressAttributeValue>();
        }

        public static AddressAttributeValue ToEntity(this AddressAttributeValueModel model, AddressAttributeValue destination)
        {
            return model.MapTo(destination);
        }
    }
}