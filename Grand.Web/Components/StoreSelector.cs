using Grand.Framework.Components;
using Grand.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.ViewComponents
{
    public class StoreSelectorViewComponent : BaseViewComponent
    {
        private readonly ICommonViewModelService _commonViewModelService;

        public StoreSelectorViewComponent(ICommonViewModelService commonViewModelService)
        {
            this._commonViewModelService = commonViewModelService;
        }

        public IViewComponentResult Invoke()
        {
            var model = _commonViewModelService.PrepareStoreSelector();
            if(model == null || model.AvailableStores.Count == 1)
                Content("");

            return View(model);
        }
    }
}