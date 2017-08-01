using System.ComponentModel.DataAnnotations;
using Grand.Framework;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;

namespace Grand.Plugin.Widgets.Slider.Models
{
    public class ConfigurationModel : BaseGrandModel
    {
        public string ActiveStoreScopeConfiguration { get; set; }

        [GrandResourceDisplayName("Plugins.Widgets.Slider.Picture")]
        [UIHint("Picture")]
        public string Picture1Id { get; set; }
        public bool Picture1Id_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Plugins.Widgets.Slider.Text")]        
        public string Text1 { get; set; }
        public bool Text1_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Plugins.Widgets.Slider.Link")]        
        public string Link1 { get; set; }
        public bool Link1_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Plugins.Widgets.Slider.Picture")]
        [UIHint("Picture")]
        public string Picture2Id { get; set; }
        public bool Picture2Id_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Plugins.Widgets.Slider.Text")]        
        public string Text2 { get; set; }
        public bool Text2_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Plugins.Widgets.Slider.Link")]        
        public string Link2 { get; set; }
        public bool Link2_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Plugins.Widgets.Slider.Picture")]
        [UIHint("Picture")]
        public string Picture3Id { get; set; }
        public bool Picture3Id_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Plugins.Widgets.Slider.Text")]        
        public string Text3 { get; set; }
        public bool Text3_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Plugins.Widgets.Slider.Link")]        
        public string Link3 { get; set; }
        public bool Link3_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Plugins.Widgets.Slider.Picture")]
        [UIHint("Picture")]
        public string Picture4Id { get; set; }
        public bool Picture4Id_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Plugins.Widgets.Slider.Text")]        
        public string Text4 { get; set; }
        public bool Text4_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Plugins.Widgets.Slider.Link")]        
        public string Link4 { get; set; }
        public bool Link4_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Plugins.Widgets.Slider.Picture")]
        [UIHint("Picture")]
        public string Picture5Id { get; set; }
        public bool Picture5Id_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Plugins.Widgets.Slider.Text")]        
        public string Text5 { get; set; }
        public bool Text5_OverrideForStore { get; set; }

        [GrandResourceDisplayName("Plugins.Widgets.Slider.Link")]        
        public string Link5 { get; set; }
        public bool Link5_OverrideForStore { get; set; }
    }
}