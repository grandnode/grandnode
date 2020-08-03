using Grand.Domain.Catalog;
using Grand.Services.Helpers;
using Grand.Web.Areas.Admin.Models.Catalog;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class TierPriceMappingExtensions
    {
        public static ProductModel.TierPriceModel ToModel(this TierPrice entity, IDateTimeHelper dateTimeHelper)
        {
            var tierprice = entity.MapTo<TierPrice, ProductModel.TierPriceModel>();
            tierprice.StartDateTime = entity.StartDateTimeUtc.ConvertToUserTime(dateTimeHelper);
            tierprice.EndDateTime = entity.EndDateTimeUtc.ConvertToUserTime(dateTimeHelper);
            return tierprice;
        }

        public static TierPrice ToEntity(this ProductModel.TierPriceModel model, IDateTimeHelper dateTimeHelper)
        {
            var tierprice = model.MapTo<ProductModel.TierPriceModel, TierPrice>();
            tierprice.StartDateTimeUtc = model.StartDateTime.ConvertToUtcTime(dateTimeHelper);
            tierprice.EndDateTimeUtc = model.EndDateTime.ConvertToUtcTime(dateTimeHelper);
            return tierprice;
        }

        public static TierPrice ToEntity(this ProductModel.TierPriceModel model, TierPrice destination, IDateTimeHelper dateTimeHelper)
        {
            var tierprice = model.MapTo(destination);
            tierprice.StartDateTimeUtc = model.StartDateTime.ConvertToUtcTime(dateTimeHelper);
            tierprice.EndDateTimeUtc = model.EndDateTime.ConvertToUtcTime(dateTimeHelper);
            return tierprice;
        }
    }
}