using Grand.Core.Domain;
using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.ViewComponents
{
    public class StoreThemeSelectorViewComponent : BaseViewComponent
    {
        private readonly ICommonViewModelService _commonViewModelService;
        private readonly StoreInformationSettings _storeInformationSettings;
        public StoreThemeSelectorViewComponent(ICommonViewModelService commonViewModelService,
            StoreInformationSettings storeInformationSettings)
        {
            _commonViewModelService = commonViewModelService;
            _storeInformationSettings = storeInformationSettings;
        }

        public IViewComponentResult Invoke()
        {
            if (!_storeInformationSettings.AllowCustomerToSelectTheme)
                return Content("");
            var model = _commonViewModelService.PrepareStoreThemeSelector();
            return View(model);
        }
    }
}