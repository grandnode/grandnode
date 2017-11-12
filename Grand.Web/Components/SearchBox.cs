using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;

namespace Grand.Web.ViewComponents
{
    public class SearchBoxViewComponent : ViewComponent
    {
        private readonly ICatalogWebService _catalogWebService;

        public SearchBoxViewComponent(ICatalogWebService catalogWebService)
        {
            this._catalogWebService = catalogWebService;
        }

        public IViewComponentResult Invoke()
        {
            var model = _catalogWebService.PrepareSearchBox();
            return View(model);
        }
    }
}