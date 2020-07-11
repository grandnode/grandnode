
using Grand.Domain.Configuration;

namespace Grand.Plugin.Shipping.ByWeight
{
    public class ShippingByWeightSettings : ISettings
    {
        public bool LimitMethodsToCreated { get; set; }
    }
}