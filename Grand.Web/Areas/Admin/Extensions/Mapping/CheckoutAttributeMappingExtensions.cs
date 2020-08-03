using Grand.Domain.Orders;
using Grand.Web.Areas.Admin.Models.Orders;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class CheckoutAttributeMappingExtensions
    {
        //attributes
        public static CheckoutAttributeModel ToModel(this CheckoutAttribute entity)
        {
            return entity.MapTo<CheckoutAttribute, CheckoutAttributeModel>();
        }

        public static CheckoutAttribute ToEntity(this CheckoutAttributeModel model)
        {
            return model.MapTo<CheckoutAttributeModel, CheckoutAttribute>();
        }

        public static CheckoutAttribute ToEntity(this CheckoutAttributeModel model, CheckoutAttribute destination)
        {
            return model.MapTo(destination);
        }
        //checkout attribute value
        public static CheckoutAttributeValueModel ToModel(this CheckoutAttributeValue entity)
        {
            return entity.MapTo<CheckoutAttributeValue, CheckoutAttributeValueModel>();
        }

        public static CheckoutAttributeValue ToEntity(this CheckoutAttributeValueModel model)
        {
            return model.MapTo<CheckoutAttributeValueModel, CheckoutAttributeValue>();
        }

        public static CheckoutAttributeValue ToEntity(this CheckoutAttributeValueModel model, CheckoutAttributeValue destination)
        {
            return model.MapTo(destination);
        }
    }
}