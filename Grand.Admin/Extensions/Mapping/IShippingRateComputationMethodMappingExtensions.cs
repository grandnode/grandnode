using Grand.Services.Shipping;
using Grand.Admin.Models.Shipping;

namespace Grand.Admin.Extensions
{
    public static class IShippingRateComputationMethodMappingExtensions
    {
        public static ShippingRateComputationMethodModel ToModel(this IShippingRateComputationMethod entity)
        {
            return entity.MapTo<IShippingRateComputationMethod, ShippingRateComputationMethodModel>();
        }
    }
}