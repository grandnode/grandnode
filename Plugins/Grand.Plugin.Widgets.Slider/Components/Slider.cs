using Microsoft.AspNetCore.Mvc;
using Grand.Core.Caching;
using Grand.Plugin.Widgets.Slider.Models;
using Grand.Services.Localization;
using Grand.Services.Media;
using System.Linq;
using Grand.Plugin.Widgets.Slider.Services;
using Grand.Plugin.Widgets.Slider.Domain;
using System.Collections.Generic;

namespace Grand.Plugin.Widgets.Slider.ViewComponents
{
    [ViewComponent(Name = "Grand.Plugin.Widgets.Slider")]
    public class SliderViewComponent : ViewComponent
    {
        private readonly IPictureService _pictureService;
        private readonly ICacheManager _cacheManager;
        private readonly ISliderService _sliderService;

        public const string PICTURE_URL_MODEL_KEY = "Grand.plugins.widgets.slider.pictureurl-{0}";

        public SliderViewComponent(
            IPictureService pictureService,
            ICacheManager cacheManager,
            ISliderService sliderService)
        {
            this._pictureService = pictureService;
            this._cacheManager = cacheManager;
            this._sliderService = sliderService;
        }

        protected string GetPictureUrl(string pictureId)
        {

            string cacheKey = string.Format(PICTURE_URL_MODEL_KEY, pictureId);
            return _cacheManager.Get(cacheKey, () =>
            {
                var url = _pictureService.GetPictureUrl(pictureId, showDefaultPicture: false);
                if (url == null)
                    url = "";

                return url;
            });
        }

        protected void PrepareModel(IList<PictureSlider> sliders, PublicInfoModel model)
        {
            int i = 1;
            foreach (var item in sliders)
            {
                model.Slide.Add(new PublicInfoModel.Slider()
                {
                    Link = item.Link,
                    PictureUrl = GetPictureUrl(item.PictureId),
                    Name = item.GetLocalized(x => x.Name),
                    Description = item.GetLocalized(x => x.Description),
                    CssClass = i == 1 ? "active" : ""
                });
                i++;
            }

        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData = null)
        {

            var model = new PublicInfoModel();
            if (widgetZone == SliderDefaults.WidgetZoneHomePage)
            {
                var slides = _sliderService.GetPictureSliders(SliderType.HomePage);
                PrepareModel(slides, model);
            }
            if (widgetZone == SliderDefaults.WidgetZoneCategoryPage)
            {
                var slides = _sliderService.GetPictureSliders(SliderType.Category, additionalData.ToString());
                PrepareModel(slides, model);
            }
            if (widgetZone == SliderDefaults.WidgetZoneManufacturerPage)
            {
                var slides = _sliderService.GetPictureSliders(SliderType.Manufacturer, additionalData.ToString());
                PrepareModel(slides, model);
            }

            if (!model.Slide.Any())
                return Content("");

            return View("/Plugins/Widgets.Slider/Views/PublicInfo.cshtml", model);
        }
    }
}