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


        [GrandResourceDisplayName("Plugins.Widgets.FacebookPixel.AllowToDisableConsentCookie")]
        public bool AllowToDisableConsentCookie { get; set; }
        public bool AllowToDisableConsentCookie_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Plugins.Widgets.FacebookPixel.ConsentDefaultState")]
        public bool ConsentDefaultState { get; set; }
        public bool ConsentDefaultState_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Plugins.Widgets.FacebookPixel.ConsentName")]
        public string ConsentName { get; set; }
        public bool ConsentName_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Plugins.Widgets.FacebookPixel.ConsentDescription")]
        public string ConsentDescription { get; set; }
        public bool ConsentDescription_OverrideForStore { get; set; }

    }
}