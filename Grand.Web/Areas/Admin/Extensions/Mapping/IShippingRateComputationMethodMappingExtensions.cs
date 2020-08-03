using Grand.Services.Shipping;
using Grand.Web.Areas.Admin.Models.Shipping;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class IShippingRateComputationMethodMappingExtensions
    {
        public static ShippingRateComputationMethodModel ToModel(this IShippingRateComputationMethod entity)
        {
            return entity.MapTo<IShippingRateComputationMethod, ShippingRateComputationMethodModel>();
        }
    }
}