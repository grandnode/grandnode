using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Plugin.Shipping.ByWeight.Models
{
    public class ShippingByWeightListModel : BaseNopModel
    {
        [GrandResourceDisplayName("Plugins.Shipping.ByWeight.Fields.LimitMethodsToCreated")]
        public bool LimitMethodsToCreated { get; set; }
    }
}