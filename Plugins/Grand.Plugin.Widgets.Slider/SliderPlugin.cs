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
                Link = _webHelper.GetStoreLocation(false),
                Name = "Sample slider 1",
                Published = true,
                PictureId = _pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "banner1.jpg"), "image/pjpeg", "banner_1").Id,
            });
            _pictureSliderRepository.Insert(new PictureSlider()
            {
                DisplayOrder = 1,
                Link = _webHelper.GetStoreLocation(false),
                Name = "Sample slider 2",
                Published = true,
                PictureId = _pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "banner2.jpg"), "image/pjpeg", "banner_2").Id,
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
