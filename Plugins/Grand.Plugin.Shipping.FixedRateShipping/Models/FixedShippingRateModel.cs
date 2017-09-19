using Grand.Framework.Mvc.ModelBinding;

namespace Grand.Plugin.Shipping.FixedRateShipping.Models
{
    public class FixedShippingRateModel
    {
        public string ShippingMethodId { get; set; }

        [GrandResourceDisplayName("Plugins.Shipping.FixedRateShipping.Fields.ShippingMethodName")]
        public string ShippingMethodName { get; set; }

        [GrandResourceDisplayName("Plugins.Shipping.FixedRateShipping.Fields.Rate")]
        public decimal Rate { get; set; }
    }
}