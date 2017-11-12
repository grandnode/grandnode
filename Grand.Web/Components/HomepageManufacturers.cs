using Microsoft.AspNetCore.Mvc;
using Grand.Web.Services;
using System.Linq;

namespace Grand.Web.ViewComponents
{
    public class HomepageManufacturersViewComponent : ViewComponent
    {
        private readonly ICatalogWebService _catalogWebService;

        public HomepageManufacturersViewComponent(ICatalogWebService catalogWebService)
        {
            this._catalogWebService = catalogWebService;
        }

        public IViewComponentResult Invoke()
        {
            var model = _catalogWebService.PrepareHomepageManufacturers();
            if (!model.Any())
                return Content("");

            return View(model);
        }
    }
}