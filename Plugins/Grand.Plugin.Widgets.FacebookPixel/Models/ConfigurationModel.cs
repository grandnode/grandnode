using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;

namespace Grand.Plugin.Widgets.FacebookPixel.Models
{
    public class ConfigurationModel : BaseGrandModel
    {
        public string ActiveStoreScopeConfiguration { get; set; }
        
        [GrandResourceDisplayName("Plugins.Widgets.FacebookPixel.PixelId")]
        public string PixelId { get; set; }
        public bool PixelId_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Plugins.Widgets.FacebookPixel.PixelScript")]
        public string PixelScript { get; set; }
        public bool PixelScript_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Plugins.Widgets.FacebookPixel.AddToCartScript")]
        public string AddToCartScript { get; set; }
        public bool AddToCartScript_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Plugins.Widgets.FacebookPixel.DetailsOrderScript")]
        public string DetailsOrderScript { get; set; }
        public bool DetailsOrderScript_OverrideForStore { get; set; }

    }
}