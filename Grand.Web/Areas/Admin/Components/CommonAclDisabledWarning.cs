using Grand.Domain.Catalog;
using Grand.Framework.Components;
using Grand.Services.Configuration;
using Grand.Services.Stores;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Components
{
    public class CommonAclDisabledWarningViewComponent : BaseViewComponent
    {
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;
        private readonly CatalogSettings _catalogSettings;

        public CommonAclDisabledWarningViewComponent(ISettingService settingService, IStoreService storeService, CatalogSettings catalogSettings)
        {
            _settingService = settingService;
            _storeService = storeService;
            _catalogSettings = catalogSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            //action displaying notification (warning) to a store owner that "ACL rules" feature is ignored
            //default setting
            bool enabled = _catalogSettings.IgnoreAcl;
            if (!enabled)
            {
                //overridden settings
                var stores = await _storeService.GetAllStores();
                foreach (var store in stores)
                {
                    if (!enabled)
                    {
                        var catalogSettings = _settingService.LoadSetting<CatalogSettings>(store.Id);
                        enabled = catalogSettings.IgnoreAcl;
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