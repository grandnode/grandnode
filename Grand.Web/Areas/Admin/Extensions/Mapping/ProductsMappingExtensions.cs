using Grand.Domain.Catalog;
using Grand.Services.Helpers;
using Grand.Web.Areas.Admin.Models.Catalog;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class ProductsMappingExtensions
    {
        public static ProductModel ToModel(this Product entity, IDateTimeHelper dateTimeHelper)
        {
            var product = entity.MapTo<Product, ProductModel>();
            product.MarkAsNewStartDateTime = entity.MarkAsNewStartDateTimeUtc.ConvertToUserTime(dateTimeHelper);
            product.MarkAsNewEndDateTime = entity.MarkAsNewEndDateTimeUtc.ConvertToUserTime(dateTimeHelper);
            product.AvailableStartDateTime = entity.AvailableStartDateTimeUtc.ConvertToUserTime(dateTimeHelper);
            product.AvailableEndDateTime = entity.AvailableEndDateTimeUtc.ConvertToUserTime(dateTimeHelper);
            product.PreOrderAvailabilityStartDateTime = entity.PreOrderAvailabilityStartDateTimeUtc.ConvertToUserTime(dateTimeHelper);
            return product;

        }

        public static Product ToEntity(this ProductModel model, IDateTimeHelper dateTimeHelper)
        {
            var product = model.MapTo<ProductModel, Product>();
            product.MarkAsNewStartDateTimeUtc = model.MarkAsNewStartDateTime.ConvertToUtcTime(dateTimeHelper);
            product.MarkAsNewEndDateTimeUtc = model.MarkAsNewEndDateTime.ConvertToUtcTime(dateTimeHelper);
            product.AvailableStartDateTimeUtc = model.AvailableStartDateTime.ConvertToUtcTime(dateTimeHelper);
            product.AvailableEndDateTimeUtc = model.AvailableEndDateTime.ConvertToUtcTime(dateTimeHelper);
            product.PreOrderAvailabilityStartDateTimeUtc = model.PreOrderAvailabilityStartDateTime.ConvertToUtcTime(dateTimeHelper);

            return product;
        }

        public static Product ToEntity(this ProductModel model, Product destination, IDateTimeHelper dateTimeHelper)
        {
            var product = model.MapTo(destination);
            product.MarkAsNewStartDateTimeUtc = model.MarkAsNewStartDateTime.ConvertToUtcTime(dateTimeHelper);
            product.MarkAsNewEndDateTimeUtc = model.MarkAsNewEndDateTime.ConvertToUtcTime(dateTimeHelper);
            product.AvailableStartDateTimeUtc = model.AvailableStartDateTime.ConvertToUtcTime(dateTimeHelper);
            product.AvailableEndDateTimeUtc = model.AvailableEndDateTime.ConvertToUtcTime(dateTimeHelper);
            product.PreOrderAvailabilityStartDateTimeUtc = model.PreOrderAvailabilityStartDateTime.ConvertToUtcTime(dateTimeHelper);
            return product;
        }

        public static ProductAttributeValue ToEntity(this PredefinedProductAttributeValue model)
        {
            return model.MapTo<PredefinedProductAttributeValue, ProductAttributeValue>();
        }
    }
}