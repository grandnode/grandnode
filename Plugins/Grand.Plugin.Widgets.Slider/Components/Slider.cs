using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Grand.Core;
using Grand.Core.Caching;
using Grand.Plugin.Widgets.Slider.Infrastructure.Cache;
using Grand.Plugin.Widgets.Slider.Models;
using Grand.Services.Configuration;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Stores;
using System.Linq;

namespace Grand.Plugin.Widgets.Slider.ViewComponents
{
    [ViewComponent(Name = "Grand.Plugin.Widgets.Slider")]
    public class SliderViewComponent : ViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IPictureService _pictureService;
        private readonly ISettingService _settingService;
        private readonly ICacheManager _cacheManager;
        private readonly ILocalizationService _localizationService;

        public SliderViewComponent(IWorkContext workContext,
            IStoreContext storeContext,
            IStoreService storeService,
            IPictureService pictureService,
            ISettingService settingService,
            ICacheManager cacheManager,
            ILocalizationService localizationService)
        {
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._storeService = storeService;
            this._pictureService = pictureService;
            this._settingService = settingService;
            this._cacheManager = cacheManager;
            this._localizationService = localizationService;
        }

        protected string GetPictureUrl(string pictureId)
        {

            string cacheKey = string.Format(ModelCacheEventConsumer.PICTURE_URL_MODEL_KEY, pictureId);
            return _cacheManager.Get(cacheKey, () =>
            {
                var url = _pictureService.GetPictureUrl(pictureId, showDefaultPicture: false);
                if (url == null)
                    url = "";

                return url;
            });
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData = null)
        {

            var sliderSettings = _settingService.LoadSetting<SliderSettings>(_storeContext.CurrentStore.Id);

            var model = new PublicInfoModel();
            model.Slide.Add(new PublicInfoModel.Slider()
            {
                PictureUrl = GetPictureUrl(sliderSettings.Picture1Id),
                Text = sliderSettings.Text1,
                Link = sliderSettings.Link1,
                CssClass = "active",
            });

            model.Slide.Add(new PublicInfoModel.Slider()
            {
                PictureUrl = GetPictureUrl(sliderSettings.Picture2Id),
                Text = sliderSettings.Text2,
                Link = sliderSettings.Link2,
                CssClass = "",
            });

            model.Slide.Add(new PublicInfoModel.Slider()
            {
                PictureUrl = GetPictureUrl(sliderSettings.Picture3Id),
                Text = sliderSettings.Text3,
                Link = sliderSettings.Link3,
                CssClass = "",
            });

            model.Slide.Add(new PublicInfoModel.Slider()
            {
                PictureUrl = GetPictureUrl(sliderSettings.Picture4Id),
                Text = sliderSettings.Text4,
                Link = sliderSettings.Link4,
                CssClass = "",
            });

            model.Slide.Add(new PublicInfoModel.Slider()
            {
                PictureUrl = GetPictureUrl(sliderSettings.Picture5Id),
                Text = sliderSettings.Text5,
                Link = sliderSettings.Link5,
            });

            if(model.Slide.Where(x=> string.IsNullOrEmpty(x.PictureUrl)).Count() ==0 )
                return Content("");

            return View("/Plugins/Widgets.Slider/Views/PublicInfo.cshtml", model);
        }
    }
}