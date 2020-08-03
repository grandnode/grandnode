using Microsoft.AspNetCore.Mvc;
using Grand.Core.Caching;
using Grand.Plugin.Widgets.Slider.Models;
using Grand.Services.Localization;
using Grand.Services.Media;
using System.Linq;
using Grand.Plugin.Widgets.Slider.Services;
using Grand.Plugin.Widgets.Slider.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grand.Core;

namespace Grand.Plugin.Widgets.Slider.ViewComponents
{
    [ViewComponent(Name = "Grand.Plugin.Widgets.Slider")]
    public class SliderViewComponent : ViewComponent
    {
        private readonly IPictureService _pictureService;
        private readonly ISliderService _sliderService;
        private readonly IWorkContext _workContext;

        public SliderViewComponent(
            IPictureService pictureService,
            ICacheManager cacheManager,
            ISliderService sliderService,
            IWorkContext workContext)
        {
            _pictureService = pictureService;
            _sliderService = sliderService;
            _workContext = workContext;
        }

        protected async Task<string> GetPictureUrl(string pictureId)
        {
            var url = await _pictureService.GetPictureUrl(pictureId, showDefaultPicture: false);
            if (url == null)
                url = "";

            return url;
        }

        protected async Task PrepareModel(IList<PictureSlider> sliders, PublicInfoModel model)
        {
            int i = 1;
            foreach (var item in sliders.OrderBy(x => x.DisplayOrder))
            {
                model.Slide.Add(new PublicInfoModel.Slider() {
                    Link = item.Link,
                    PictureUrl = await GetPictureUrl(item.PictureId),
                    Name = item.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                    Description = item.GetLocalized(x => x.Description, _workContext.WorkingLanguage.Id),
                    FullWidth = item.FullWidth,
                    CssClass = i == 1 ? "active" : ""
                });
                i++;
            }

        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData = null)
        {

            var model = new PublicInfoModel();
            if (widgetZone == SliderDefaults.WidgetZoneHomePage)
            {
                var slides = await _sliderService.GetPictureSliders(SliderType.HomePage);
                await PrepareModel(slides, model);
            }
            if (widgetZone == SliderDefaults.WidgetZoneCategoryPage)
            {
                var slides = await _sliderService.GetPictureSliders(SliderType.Category, additionalData.ToString());
                await PrepareModel(slides, model);
            }
            if (widgetZone == SliderDefaults.WidgetZoneManufacturerPage)
            {
                var slides = await _sliderService.GetPictureSliders(SliderType.Manufacturer, additionalData.ToString());
                await PrepareModel(slides, model);
            }

            if (!model.Slide.Any())
                return Content("");

            return View("/Plugins/Widgets.Slider/Views/PublicInfo.cshtml", model);
        }
    }
}