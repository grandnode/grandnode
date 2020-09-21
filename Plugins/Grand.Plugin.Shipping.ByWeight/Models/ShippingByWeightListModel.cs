using Grand.Core.ModelBinding;
using Grand.Core.Models;

namespace Grand.Plugin.Shipping.ByWeight.Models
{
    public class ShippingByWeightListModel : BaseModel
    {
        [GrandResourceDisplayName("Plugins.Shipping.ByWeight.Fields.LimitMethodsToCreated")]
        public bool LimitMethodsToCreated { get; set; }
    }
}