using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;

namespace Grand.Web.ViewComponents
{
    public class StoreSelectorViewComponent : ViewComponent
    {
        private readonly ICommonWebService _commonWebService;

        public StoreSelectorViewComponent(ICommonWebService commonWebService)
        {
            this._commonWebService = commonWebService;
        }

        public IViewComponentResult Invoke()
        {
            var model = _commonWebService.PrepareStoreSelector();
            if(model == null || model.AvailableStores.Count == 1)
                Content("");

            return View(model);
        }
    }
}