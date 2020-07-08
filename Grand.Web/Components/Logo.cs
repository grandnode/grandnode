using Grand.Core;
using Grand.Core.Caching;
using Grand.Domain.Stores;
using Grand.Framework.Components;
using Grand.Framework.Themes;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Web.Infrastructure.Cache;
using Grand.Web.Models.Common;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class LogoViewComponent : BaseViewComponent
    {
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly ICacheManager _cacheManager;
        private readonly IPictureService _pictureService;
        private readonly IThemeContext _themeContext;

        private readonly StoreInformationSettings _storeInformationSettings;

        public LogoViewComponent(IStoreContext storeContext,
            IWorkContext workContext,
            ICacheManager cacheManager,
            IPictureService pictureService,
            IThemeContext themeContext,
            StoreInformationSettings storeInformationSettings)
        {
            _storeContext = storeContext;
            _workContext = workContext;
            _cacheManager = cacheManager;
            _pictureService = pictureService;
            _themeContext = themeContext;
            _storeInformationSettings = storeInformationSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await PrepareLogo();
            return View(model);
        }

        private async Task<LogoModel> PrepareLogo()
        {
            var model = new LogoModel {
                StoreName = _storeContext.CurrentStore.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id)
            };

            var cacheKey = string.Format(ModelCacheEventConst.STORE_LOGO_PATH, _storeContext.CurrentStore.Id, _themeContext.WorkingThemeName);
            model.LogoPath = await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var logo = "";
                var picture = await _pictureService.GetPictureById(_storeInformationSettings.LogoPictureId);
                if (picture != null)
                {
                    logo = await _pictureService.GetPictureUrl(picture, showDefaultPicture: false);
                }
                if (string.IsNullOrEmpty(logo))
                {
                    //use default logo
                    logo = string.Format("{0}://{1}/Themes/{2}/Content/images/logo.png", HttpContext.Request.Scheme, HttpContext.Request.Host, _themeContext.WorkingThemeName);
                }
                return logo;
            });
            return model;
        }
    }
}