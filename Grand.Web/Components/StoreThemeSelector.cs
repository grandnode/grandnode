using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;
using Grand.Core.Domain;

namespace Grand.Web.ViewComponents
{
    public class StoreThemeSelectorViewComponent : ViewComponent
    {
        private readonly ICommonWebService _commonWebService;
        private readonly StoreInformationSettings _storeInformationSettings;
        public StoreThemeSelectorViewComponent(ICommonWebService commonWebService,
            StoreInformationSettings storeInformationSettings)
        {
            this._commonWebService = commonWebService;
            this._storeInformationSettings = storeInformationSettings;
        }

        public IViewComponentResult Invoke()
        {
            if (!_storeInformationSettings.AllowCustomerToSelectTheme)
                return Content("");
            var model = _commonWebService.PrepareStoreThemeSelector();
            return View(model);


        }
    }
}