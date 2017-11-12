using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;

namespace Grand.Web.ViewComponents
{
    public class TopMenuViewComponent : ViewComponent
    {
        private readonly ICatalogWebService _catalogWebService;

        public TopMenuViewComponent(ICatalogWebService catalogWebService)
        {
            this._catalogWebService = catalogWebService;
        }

        public IViewComponentResult Invoke()
        {
            var model = _catalogWebService.PrepareTopMenu();
            return View(model);

        }
    }
}