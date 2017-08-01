using Grand.Core.Domain.Catalog;
using Grand.Services.Configuration;
using Grand.Services.Stores;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.Areas.Admin.Components
{
    public class CommonMultistoreDisabledWarningViewComponent : ViewComponent
    {
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;
        private readonly CatalogSettings _catalogSettings;

        public CommonMultistoreDisabledWarningViewComponent(ISettingService settingService, IStoreService storeService, CatalogSettings catalogSettings)
        {
            this._settingService = settingService;
            this._storeService = storeService;
            this._catalogSettings = catalogSettings;
        }

        public IViewComponentResult Invoke()//original Action name: MultistoreDisabledWarning
        {
            //action displaying notification (warning) to a store owner that "limit per store" feature is ignored
            //default setting
            bool enabled = _catalogSettings.IgnoreStoreLimitations;
            if (!enabled)
            {
                //overridden settings
                var stores = _storeService.GetAllStores();
                foreach (var store in stores)
                {
                    if (!enabled)
                    {
                        var catalogSettings = _settingService.LoadSetting<CatalogSettings>(store.Id);
                        enabled = catalogSettings.IgnoreStoreLimitations;
                    }
                }
            }

            //This setting is disabled. No warnings.
            if (!enabled)
                return Content("");

            return View();
        }
    }
}