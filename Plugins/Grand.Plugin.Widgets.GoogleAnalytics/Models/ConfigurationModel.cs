﻿using System.Web.Mvc;
using Grand.Web.Framework;
using Grand.Web.Framework.Mvc;

namespace Grand.Plugin.Widgets.GoogleAnalytics.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public string ActiveStoreScopeConfiguration { get; set; }
        
        [NopResourceDisplayName("Plugins.Widgets.GoogleAnalytics.GoogleId")]
        [AllowHtml]
        public string GoogleId { get; set; }
        public bool GoogleId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.GoogleAnalytics.TrackingScript")]
        [AllowHtml]
        //tracking code
        public string TrackingScript { get; set; }
        public bool TrackingScript_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.GoogleAnalytics.EcommerceScript")]
        [AllowHtml]
        public string EcommerceScript { get; set; }
        public bool EcommerceScript_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.GoogleAnalytics.EcommerceDetailScript")]
        [AllowHtml]
        public string EcommerceDetailScript { get; set; }
        public bool EcommerceDetailScript_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.GoogleAnalytics.IncludingTax")]
        public bool IncludingTax { get; set; }
        public bool IncludingTax_OverrideForStore { get; set; }

    }
}