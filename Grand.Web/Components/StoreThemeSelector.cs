using Grand.Core.Domain;
using Grand.Framework.Components;
using Grand.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class StoreThemeSelectorViewComponent : BaseViewComponent
    {
        private readonly ICommonViewModelService _commonViewModelService;
        private readonly StoreInformationSettings _storeInformationSettings;
        public StoreThemeSelectorViewComponent(ICommonViewModelService commonViewModelService,
            StoreInformationSettings storeInformationSettings)
        {
            this._commonViewModelService = commonViewModelService;
            this._storeInformationSettings = storeInformationSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!_storeInformationSettings.AllowCustomerToSelectTheme)
                return Content("");
            var model = await Task.Run(() => _commonViewModelService.PrepareStoreThemeSelector());
            return View(model);


        }
    }
}