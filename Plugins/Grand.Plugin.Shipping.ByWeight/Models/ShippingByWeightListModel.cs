using Grand.Framework;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;

namespace Grand.Plugin.Shipping.ByWeight.Models
{
    public class ShippingByWeightListModel : BaseGrandModel
    {
        [GrandResourceDisplayName("Plugins.Shipping.ByWeight.Fields.LimitMethodsToCreated")]
        public bool LimitMethodsToCreated { get; set; }
    }
}