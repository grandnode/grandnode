using System.Collections.Generic;
using System.IO;
using System.Web.Routing;
using Grand.Core;
using Grand.Core.Plugins;
using Grand.Services.Cms;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Grand.Services.Media;

namespace Grand.Plugin.Widgets.Slider
{
    /// <summary>
    /// PLugin
    /// </summary>
    public class SliderPlugin : BasePlugin, IWidgetPlugin
    {
        private readonly IPictureService _pictureService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;

        public SliderPlugin(IPictureService pictureService, 
            ISettingService settingService, IWebHelper webHelper)
        {
            this._pictureService = pictureService;
            this._settingService = settingService;
            this._webHelper = webHelper;
        }

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>Widget zones</returns>
        public IList<string> GetWidgetZones()
        {
            return new List<string> { "home_page_top" };
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "WidgetsSlider";
            routeValues = new RouteValueDictionary { { "Namespaces", "Grand.Plugin.Widgets.Slider.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for displaying widget
        /// </summary>
        /// <param name="widgetZone">Widget zone where it's displayed</param>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetDisplayWidgetRoute(string widgetZone, out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PublicInfo";
            controllerName = "WidgetsSlider";
            routeValues = new RouteValueDictionary
            {
                {"Namespaces", "Grand.Plugin.Widgets.Slider.Controllers"},
                {"area", null},
                {"widgetZone", widgetZone}
            };
        }
        
        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //pictures
            var sampleImagesPath = CommonHelper.MapPath("~/Plugins/Widgets.Slider/Content/slider/sample-images/");

            //settings
            var settings = new SliderSettings
            {
                Picture1Id = _pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "banner1.jpg"), "image/pjpeg", "banner_1").Id,
                Text1 = "Sample text 1",
                Link1 = _webHelper.GetStoreLocation(false),
                Picture2Id = _pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "banner2.jpg"), "image/pjpeg", "banner_2").Id,
                Text2 = "Sample text 2",
                Link2 = _webHelper.GetStoreLocation(false),
            };
            _settingService.SaveSetting(settings);


            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Picture1", "Picture 1");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Picture2", "Picture 2");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Picture3", "Picture 3");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Picture4", "Picture 4");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Picture5", "Picture 5");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Picture", "Picture");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Picture.Hint", "Upload picture.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Text", "Comment");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Text.Hint", "Enter comment for picture. Leave empty if you don't want to display any text.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Link", "URL");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Link.Hint", "Enter URL. Leave empty if you don't want this picture to be clickable.");

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<SliderSettings>();

            //locales
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Picture1");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Picture2");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Picture3");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Picture4");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Picture5");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Picture");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Picture.Hint");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Text");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Text.Hint");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Link");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Link.Hint");
            
            base.Uninstall();
        }
    }
}
