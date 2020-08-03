using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Plugins;
using Grand.Domain.Data;
using Grand.Plugin.Widgets.Slider.Domain;
using Grand.Services.Cms;
using Grand.Services.Localization;
using Grand.Services.Media;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;

        public SliderPlugin(IPictureService pictureService,
            IWebHelper webHelper,
            IRepository<PictureSlider> pictureSliderRepository,
            ILocalizationService localizationService,
            ILanguageService languageService)
        {
            _pictureService = pictureService;
            _webHelper = webHelper;
            _pictureSliderRepository = pictureSliderRepository;
            _localizationService = localizationService;
            _languageService = languageService;
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
        public override async Task Install()
        {
            //pictures
            var sampleImagesPath = CommonHelper.MapPath("~/Plugins/Widgets.Slider/Content/slider/sample-images/");
            var byte1 = File.ReadAllBytes(sampleImagesPath + "banner1.jpg");
            var byte2 = File.ReadAllBytes(sampleImagesPath + "banner2.png");

            var pic1 = await _pictureService.InsertPicture(byte1, "image/jpeg", "banner_1", validateBinary: false);
            var pic2 = await _pictureService.InsertPicture(byte2, "image/png", "banner_2", validateBinary: false);

            await _pictureSliderRepository.InsertAsync(new PictureSlider()
            {
                DisplayOrder = 0,
                Link = "",
                Name = "Sample slider 1",
                FullWidth = true,
                Published = true,
                PictureId = pic1.Id,
                Description = "<div class=\"row slideRow justify-content-start\"><div class=\"col-lg-6 d-flex flex-column justify-content-center align-items-center\"><div style=\"display: flex; flex-direction: column; align-items: center; justify-content: center;\"><div class=\"animated fadeInDown delay-1s\" style=\"font-size: 30px; font-weight: 300; line-height: 1; opacity: .9; padding: 15px 0 0 0;\">exclusive - modern - elegant</div><div class=\"animated zoomIn delay-0-5s\" style=\"font-size: 92px; font-weight: 500; line-height: 1; opacity: .9;\">Furniture</div><div class=\"animated fadeInUp delay-1s\" style=\"font-size: 26px; font-weight: 500; opacity: .9; padding: 0 0 15px 0;\">Go to collection and see more...</div><a class=\"btn btn-info animated fadeInUp delay-1-5s\" style=\"width: 100%; margin: 15px 0 0 0; background: #87C255; border-color: #87C255;\"> SHOP NOW </a></div></div></div>"
            });

            await _pictureSliderRepository.InsertAsync(new PictureSlider()
            {
                DisplayOrder = 1,
                Link = _webHelper.GetStoreLocation(false),
                Name = "Sample slider 2",
                FullWidth = true,
                Published = true,
                PictureId = pic2.Id,
                Description = "<div class=\"row slideRow\"><div class=\"col-md-6 offset-md-6 col-12 offset-0 d-flex flex-column justify-content-center align-items-start px-0 pr-md-3\"><div class=\"slide-title text-dark animated bounceInRight delay-0-5s\"><h2 class=\"mt-0\">Apple MacBook Pro 13-inch</h2></div><div class=\"slide-content animated bounceInRight delay-1s\"><p class=\"mb-0\"><span style=\"color: #999999;\">A groundbreaking Retina display. A new force-sensing trackpad. All-flash architecture. Powerful dual-core and quad-core Intel processors.</span></p></div><div class=\"slide-price animated fadeInRight delay-1-5s d-inline-flex align-items-center justify-content-start w-100 mt-2\"><p class=\"actual\">1,800.00</p><p class=\"old-price\">$2,200.00</p></div><div class=\"slide-button animated bounceInUp delay-2s mt-3\"><a class=\"btn btn-outline-info\" href=\"/apple-macbook-pro-13-inch\">See More</a></div></div></div>",
            });

            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Added", "Slider added");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Addnew", "Add new slider");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.AvailableStores", "Available stores");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.AvailableStores.Hint", "Select stores for which the slider will be shown.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Backtolist", "Back to list");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Category", "Category");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Category.Hint", "Select the category where slider should appear.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Category.Required", "Category is required");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Description", "Description");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Description.Hint", "Enter the description of the slider");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.DisplayOrder", "Display Order");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.DisplayOrder.Hint", "The slider display order. 1 represents the first item in the list.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Edit", "Edit slider");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Edited", "Slider edited");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Fields.Displayorder", "Display Order");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Fields.Link", "Link");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Fields.ObjectType", "Slider type");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Fields.Picture", "Picture");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Fields.Published", "Published");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Fields.Title", "Title");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.FullWidth", "Full width");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.FullWidth.hint", "Full width");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Info", "Info");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.LimitedToStores", "Limited to stores");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.LimitedToStores.Hint", "Determines whether the slider is available only at certain stores.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Link", "URL");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Link.Hint", "Enter URL. Leave empty if you don't want this picture to be clickable.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Manage", "Manage Bootstrap Slider");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Manufacturer", "Manufacturer");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Manufacturer.Hint", "Select the manufacturer where slider should appear.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Manufacturer.Required", "Manufacturer is required");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Name", "Name");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Name.Hint", "Enter the name of the slider");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Name.Required", "Name is required");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Picture", "Picture");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Picture.Required", "Picture is required");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Published", "Published");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Published.Hint", "Specify it should be visible or not");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.SliderType", "Slider type");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.SliderType.Hint", "Choose the slider type. Home page, category or manufacturer page.");
            await this.AddOrUpdatePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Stores", "Stores");


            await base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override async Task Uninstall()
        {

            //clear repository
            await _pictureSliderRepository.DeleteAsync(_pictureSliderRepository.Table.ToList());

            //locales
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Added");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Addnew");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.AvailableStores");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.AvailableStores.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Backtolist");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Category");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Category.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Category.Required");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Description");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Description.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.DisplayOrder");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.DisplayOrder.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Edit");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Edited");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Fields.Displayorder");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Fields.Link");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Fields.ObjectType");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Fields.Picture");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Fields.Published");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Fields.Title");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Info");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.LimitedToStores");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.LimitedToStores.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Link");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Link.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Manage");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Manufacturer");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Manufacturer.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Manufacturer.Required");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Name");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Name.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Name.Required");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Picture");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Picture.Required");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Published");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Published.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.SliderType");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.SliderType.Hint");
            await this.DeletePluginLocaleResource(_localizationService, _languageService, "Plugins.Widgets.Slider.Stores");

            await base.Uninstall();
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
