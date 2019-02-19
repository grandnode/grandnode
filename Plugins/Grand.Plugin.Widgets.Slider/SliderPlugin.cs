using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Plugins;
using Grand.Plugin.Widgets.Slider.Domain;
using Grand.Services.Cms;
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
        private readonly IWebHelper _webHelper;
        private readonly IRepository<PictureSlider> _pictureSliderRepository;

        public SliderPlugin(IPictureService pictureService, 
            IWebHelper webHelper,
            IRepository<PictureSlider> pictureSliderRepository)
        {
            this._pictureService = pictureService;
            this._webHelper = webHelper;
            this._pictureSliderRepository = pictureSliderRepository;
        }

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>Widget zones</returns>
        public IList<string> GetWidgetZones()
        {
            return new List<string>
            {
                SliderDefaults.WidgetZoneHomePage,
                SliderDefaults.WidgetZoneCategoryPage,
                SliderDefaults.WidgetZoneManufacturerPage
            };
        }

        
        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //pictures
            var sampleImagesPath = CommonHelper.MapPath("~/Plugins/Widgets.Slider/Content/slider/sample-images/");

            _pictureSliderRepository.Insert(new PictureSlider() {
                DisplayOrder = 0,
                Link = "",
                Name = "Sample slider 1",
                Published = true,
                PictureId = "",
                Description = "<div class=\"row slideRow\"><div class=\"col-lg-6 offset-lg-6 col-12 offset-0 d-flex flex-column justify-content-center align-items-center px-0\"><div class=\"slide-title animated fadeInRight delay-1s\"><h2 class=\"mb-0 text-white\">GrandNode 4.40 Release</h2></div><div class=\"slide-content mt-3 animated fadeInRight delay-1-5s\"><p>Open Source Cross Platform E-Commerce Solution based on .NET Core 2.2</p></div><div class=\"slide-button mt-3 animated fadeInRight delay-2s\"><a class=\"btn btn-outline-white\" href=\"https://grandnode.com\">Read More</a></div></div></div><p><img src=\"/Plugins/Widgets.Slider/Content/slider/sample-images/banner1.jpg\" alt=\"\" /></p>"
            });
            _pictureSliderRepository.Insert(new PictureSlider()
            {
                DisplayOrder = 1,
                Link = _webHelper.GetStoreLocation(false),
                Name = "Sample slider 2",
                Published = true,
                PictureId = "",
                Description = "<div class=\"row slideRow\"><div class=\"container\"><div class=\"row\"><div class=\"col-6 d-flex flex-column justify-content-center align-item-center px-0\"><div class=\"animated zoomIn delay-0-5s\"><img style=\"max-width: 500px;\" src=\"/Plugins/Widgets.Slider/Content/slider/sample-images/banner2.png\" alt=\"\" /></div></div><div class=\"col-6 d-flex flex-column justify-content-center align-items-start px-0\"><div class=\"slide-title text-dark animated bounceInRight delay-0-5s\"><h2 class=\"mt-0\">Apple MacBook Pro 13-inch</h2></div><div class=\"slide-content animated bounceInRight delay-1s\"><p class=\"mb-0\">A groundbreaking Retina display. A new force-sensing trackpad. All-flash architecture. Powerful dual-core and quad-core Intel processors. Together, these features take the notebook to a new level of performance. And they will do the same for you in everything you create.</p></div><div class=\"slide-price animated fadeInRight delay-1-5s d-inline-flex align-items-center justify-content-start w-100  mt-2\"><p class=\"actual\">$1,800.00</p><p class=\"old-price\">$2,200.00</p></div><div class=\"slide-button animated bounceInUp delay-2s mt-3\"><a class=\"btn btn-outline-info\" href=\"/apple-macbook-pro-13-inch\">See More</a></div></div></div></div></div>",
            });

            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Added", "Slider added");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Addnew", "Add new slider");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.AvailableStores", "Available stores");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.AvailableStores.Hint", "Select stores for which the slider will be shown.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Backtolist", "Back to list");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Category", "Category");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Category.Hint", "Select the category where slider should appear.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Category.Required", "Category is required");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Description", "Description");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Description.Hint", "Enter the description of the slider");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.DisplayOrder", "Display Order");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.DisplayOrder.Hint", "The slider display order. 1 represents the first item in the list.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Edit", "Edit slider");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Edited", "Slider edited");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Fields.Displayorder", "Display Order");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Fields.Link", "Link");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Fields.ObjectType", "Slider type");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Fields.Picture", "Picture");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Fields.Published", "Published");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Fields.Title", "Title");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Info", "Info");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.LimitedToStores", "Limited to stores");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.LimitedToStores.Hint", "Determines whether the slider is available only at certain stores.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Link", "URL");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Link.Hint", "Enter URL. Leave empty if you don't want this picture to be clickable.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Manage", "Manage Bootstrap Slider");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Manufacturer", "Manufacturer");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Manufacturer.Hint", "Select the manufacturer where slider should appear.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Manufacturer.Required", "Manufacturer is required");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Name", "Name");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Name.Hint", "Enter the name of the slider");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Name.Required", "Name is required");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Picture", "Picture");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Picture.Required", "Picture is required");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Published", "Published");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Published.Hint", "Specify it should be visible or not");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.SliderType", "Slider type");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.SliderType.Hint", "Choose the slider type. Home page, category or manufacturer page.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Widgets.Slider.Stores", "Stores");


            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {

            //clear repository
            _pictureSliderRepository.DeleteAsync(_pictureSliderRepository.Table.ToList());

            //locales
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Added");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Addnew");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.AvailableStores");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.AvailableStores.Hint");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Backtolist");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Category");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Category.Hint");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Category.Required");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Description");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Description.Hint");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.DisplayOrder");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.DisplayOrder.Hint");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Edit");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Edited");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Fields.Displayorder");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Fields.Link");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Fields.ObjectType");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Fields.Picture");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Fields.Published");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Fields.Title");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Info");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.LimitedToStores");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.LimitedToStores.Hint");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Link");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Link.Hint");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Manage");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Manufacturer");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Manufacturer.Hint");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Manufacturer.Required");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Name");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Name.Hint");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Name.Required");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Picture");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Picture.Required");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Published");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Published.Hint");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.SliderType");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.SliderType.Hint");
            this.DeletePluginLocaleResource("Plugins.Widgets.Slider.Stores");

            base.Uninstall();
        }

        public void GetPublicViewComponent(string widgetZone, out string viewComponentName)
        {
            viewComponentName = "Grand.Plugin.Widgets.Slider";
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/WidgetsSlider/Configure";
        }
    }
}
